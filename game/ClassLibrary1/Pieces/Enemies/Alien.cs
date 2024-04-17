using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static ClassLibrary1.Pieces.IKillable;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public class Alien : EnemyPiece, IRepairable, IDeserializationCallback
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        //private readonly bool _autoRepair;
        private readonly double _energy;

        private double _morale;
        private bool targeted;
        //private List<Point> _path;

        //cant use these tiles as references...
        private Tile lastMove = null, curMove = null;
        private int numAtts = 0;
        private readonly List<Tuple<Tile, Tile>> lastAttacks = new();

        //should be Point?
        public Tile LastMove => lastMove;
        public List<Tuple<Tile, Tile>> LastAttacks => lastAttacks;

        //var defs = killable.AllDefenses.Select(d => new Values(d.Type, d.DefenseMax));
        //var atts = attacker.Attacks.Select(a => new IAttacker.Values(a.Type, a.AttackMax, a.Range));
        //var move = new IMovable.Values(movable.MoveInc, movable.MoveMax, movable.MoveLimit);
        //MechBlueprint.CalcCost(Enemy.GetDifficulty(Game), 0, defs, killable.Resilience, atts, move, out double e, out double m);
        double IRepairable.RepairCost => Consts.GetRepairCost(this, _energy, 0);
        bool IRepairable.AutoRepair => !Tile.Visible;// Game.Rand.Bool(.078);// _autoRepair;

        public List<Point> PathToCore { get; private set; }// private
        public List<Point> RetreatPath { get; private set; }// private

        public double Morale => _morale;// private - hide behind research

        private Alien(Tile tile, List<Point> pathToCore, double energy,
            IEnumerable<Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
            : base(tile, pathToCore == null || Game.Rand.Bool() ? AIState.Patrol : AIState.Rush)
        {
            //this._autoRepair = Game.Rand.Bool(.078);//
            this._energy = energy;
            this._morale = Game.Rand.Weighted(.91);
            this.targeted = false;
            this.PathToCore = pathToCore;
            this.RetreatPath = new() { Tile.Location };

            this.killable = new Killable(this, killable, resilience);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);

            OnDeserialization(this);
        }
        internal static Alien NewAlien(Tile tile, List<Point> pathToCore, double energy,
            IEnumerable<Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(tile, pathToCore, energy, killable, resilience, attacks, movable);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        public void OnDeserialization(object sender)
        {
            ((Killable)this.killable).OnDeserialization(this);
            ((Attacker)this.attacker).OnDeserialization(this);
            this.attacker.Event.AttackEvent += Attacker_AttackEvent;
            this.killable.Event.DamagedEvent += Killable_DamagedEvent; 
        }

        private void Attacker_AttackEvent(object sender, Attacker.AttackEventArgs e)
        {
            this.lastAttacks.Add(Tuple.Create(e.From, e.To));
            this.numAtts++;
        }

        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            this.targeted = true;
            this._morale *= DefPct();
        }
        protected override void OnDeath(EnemyPiece enemyPiece)
        {
            double cost = Consts.EnemyEnergy;
            double offset = enemyPiece is Hive ? Consts.EnemyEnergy : Enemy.GetEneryIncome(Game);
            if (enemyPiece is Hive hive)
                cost = 1.69 * hive.Cost + offset;
            else if (enemyPiece is IRepairable reparable)
                cost = reparable.RepairCost;

            offset += ((IRepairable)this).RepairCost;
            double distance = Tile.GetDistance(enemyPiece.Tile);

            double pct = offset / (cost + offset);
            pct = Math.Pow(pct, Math.Sqrt(Consts.CaveSize / (distance + 1)));

            this._morale *= Game.Rand.Weighted(pct);
        }

        internal override AIState TurnState(double difficulty, Dictionary<Tile, double> playerAttacks, HashSet<Tile> moveTiles, HashSet<IKillable> killables,
            out List<Point> path)
        {
            AIState state = base.TurnState(difficulty, playerAttacks, moveTiles, killables, out path);

            if (MoraleCheck(0, false))
                state = AIState.Retreat;

            switch (state)
            {
                case AIState.Retreat:
                    state = AIState.Retreat;
                    if (MoraleCheck(1, true))
                        goto case AIState.Fight;
                    if (RetreatPath == null || !RetreatPath.Any() || Game.Map.GetTile(RetreatPath[^1]).Visible || !SeePath(RetreatPath))
                        RetreatPath = Game.Map.PathFindRetreat(Tile, GetRetreatTo(), GetPathFindingMovement(), GetCurDefenseValue(), playerAttacks);
                    break;
                case AIState.Fight:
                    state = AIState.Fight;
                    if (MoraleCheck(1 / difficulty, true))
                        if (!PlayerPresent())
                            goto case AIState.Patrol;
                    break;
                case AIState.Patrol:
                    state = AIState.Patrol;
                    if (MoraleCheck(1 + (3.9 / difficulty / difficulty), true))
                        goto case AIState.Rush;
                    if (MoraleCheck(.5 / difficulty, true) && SeePath())
                        goto case AIState.Rush;
                    //if (MoraleCheck(.125, false))
                    if (PlayerPresent())
                        goto case AIState.Fight;
                    break;
                case AIState.Rush:
                    state = AIState.Rush;
                    if (MoraleCheck(.25, false))
                        goto case AIState.Fight;
                    if (!SeePath())
                        if (MoraleCheck(.5, false))
                            goto case AIState.Fight;
                        else if (MoraleCheck(.75 / difficulty, true))
                            PathToCore = Game.Map.PathFind(Tile, GetPathFindingMovement(), _ => true);
                        else
                            goto case AIState.Patrol;
                    break;
            }

            if (state == AIState.Retreat)
                path = RetreatPath;
            else if (state == AIState.Rush)
                path = PathToCore;

            this._state = state;
            return state;

            bool SeePath(List<Point> path = null) => (path ?? PathToCore).Any(p => moveTiles.Contains(Game.Map.GetTile(p)));
            bool PlayerPresent() => targeted || killables.Any() || playerAttacks.ContainsKey(Tile);
            Tile GetRetreatTo()
            {
                Tile retreat = null;
                if (Game.Rand.Bool())
                {
                    retreat = RetreatPath?.Where(p => !Game.Map.GetTile(p).Visible).OrderBy(p => Tile.GetDistance(p)).Select(Game.Map.GetTile).FirstOrDefault();
                    if (retreat == null)
                        ;
                }
                if (retreat == null)
                    retreat = Game.Map.FindNonVisibleTile(Tile);
                return retreat;
            }
        }
        private bool MoraleCheck(double check, bool sign)
        {
            double morale = _morale;
            if (sign)
                morale = Game.Rand.OE(morale);
            else
                morale = Math.Sqrt(morale) - Game.Rand.OE(1 - morale);
            return sign ? morale > check : morale < check;
        }

        internal override void StartTurn()
        {
            base.StartTurn();

            this.lastMove = Tile.Visible ? curMove : null;
            this.curMove = Tile;

            int remove = lastAttacks.Count - numAtts;
            if (remove > 0)
                this.lastAttacks.RemoveRange(0, remove);
            this.numAtts = 0;

            double pct = DefPct();
            pct = 1 - .65 * pct;
            this._morale = float.Epsilon + Math.Pow(_morale, Game.Rand.Range(pct, 1));
            if (_morale > 1)
                ; //not currenly a problem...
            this.targeted = false;
        }
        private double DefPct()
        {
            double pct = Consts.GetDamagedValue(this, 1, 0);
            pct *= pct;
            double defCur = killable.AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur));
            double defMax = killable.AllDefenses.Sum(d => Consts.StatValue(d.DefenseMax));
            pct *= Math.Sqrt(defCur / defMax);
            return Math.Sqrt(pct);
        }

        public bool CanRepair() => Consts.CanRepair(this);

        private double GetCurDefenseValue() => killable.AllDefenses.Sum(d => Consts.StatValue(d.DefenseCur));
        private double GetPathFindingMovement() =>
            GetPathFindingMovement(new(movable.MoveInc, movable.MoveMax, movable.MoveLimit));
        internal static double GetPathFindingMovement(IMovable.Values movable) =>
            (movable.MoveInc + movable.MoveMax) / 2.0;

        public override string ToString()
        {
            return "Alien " + PieceNum;
        }
    }
}
