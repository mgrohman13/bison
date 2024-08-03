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

        //var defs = killable.AllDefenses.Select(d => new Values(d.Type, d.DefenseMax));
        //var atts = attacker.Attacks.Select(a => new IAttacker.Values(a.Type, a.AttackMax, a.Range));
        //var move = new IMovable.Values(movable.MoveInc, movable.MoveMax, movable.MoveLimit);
        //MechBlueprint.CalcCost(Enemy.GetDifficulty(Game), 0, defs, killable.Resilience, atts, move, out double e, out double m);
        double IRepairable.RepairCost => Consts.GetRepairCost(this, _energy, 0);
        bool IRepairable.AutoRepair => !Tile.Visible;

        private List<Point> PathToCore { get; set; }
        private List<Point> RetreatPath { get; set; }

        //private double Morale => _morale;// hide behind research

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

        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);
            if (killable != null)
            {
                ((Killable)killable).OnDeserialization(this);
                killable.Event.DamagedEvent += Killable_DamagedEvent;
            }
            if (attacker != null)
            {
                //((Attacker)attacker).OnDeserialization(this);
                //attacker.Event.AttackEvent += Attacker_AttackEvent;
            }
        }

        //private void Attacker_AttackEvent(object sender, Attacker.AttackEventArgs e)
        //{
        //    this.lastAttacks.Add(Tuple.Create(e.From, e.To));
        //    this.numAtts++;
        //}

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

        internal override AIState TurnState(double difficulty, bool clearPaths, Dictionary<Tile, double> playerAttacks, HashSet<Tile> moveTiles, HashSet<IKillable> killables,
            out List<Point> path)
        {
            if (clearPaths)
                PathToCore.Clear();

            AIState state = base.TurnState(difficulty, clearPaths, playerAttacks, moveTiles, killables, out path);
            killable.GetHitsRepair(out double hitsInc, out _);
            var armor = killable.Protection.SingleOrDefault(d => d.Type == CombatTypes.DefenseType.Armor && d.DefenseCur < d.DefenseMax);
            if (armor != null)
                hitsInc += armor.GetRegen();

            if (MoraleCheck(0, false))
                state = AIState.Retreat;

            switch (state)
            {
                case AIState.Heal:
                    state = AIState.Heal;
                    if (hitsInc <= 0 || PlayerThreat())
                        goto case AIState.Retreat;
                    break;
                case AIState.Retreat:
                    state = AIState.Retreat;
                    if (hitsInc > 0 && !PlayerThreat())
                        goto case AIState.Heal;
                    if (MoraleCheck(1, true))
                        goto case AIState.Fight;
                    if (moveTiles.Any() && NeedsRetreatPath())
                        RetreatPath = Game.Map.PathFindRetreat(Tile, GetRetreatTo(), GetPathFindingMovement(), GetCurDefenseValue(), playerAttacks, ValidRetreatTile);
                    break;
                case AIState.Fight:
                    state = AIState.Fight;
                    if (MoraleCheck(1 / Math.Sqrt(difficulty), true) && !PlayerThreat())
                        goto case AIState.Patrol;
                    break;
                case AIState.Patrol:
                    state = AIState.Patrol;
                    if (MoraleCheck(1 + (3.9 / difficulty / difficulty), true))
                        goto case AIState.Rush;
                    if (PlayerThreat())
                        goto case AIState.Fight;
                    if (hitsInc > 0)
                        goto case AIState.Heal;
                    if (PlayerPassive())
                        goto case AIState.Harass;
                    if (MoraleCheck(.5 / difficulty, true) && SeePath())
                        goto case AIState.Rush;
                    break;
                case AIState.Harass:
                    state = AIState.Harass;
                    if (MoraleCheck(PlayerThreat() ? .6 : .4, false))
                        goto case AIState.Fight;
                    if (!PlayerPassive())
                        if (MoraleCheck(.8, true))
                            goto case AIState.Rush;
                        else
                            goto case AIState.Patrol;
                    break;
                case AIState.Rush:
                    state = AIState.Rush;
                    if (PlayerPassive())
                        goto case AIState.Harass;
                    if (MoraleCheck((PlayerThreat() ? .75 : .25) / difficulty, false))
                        goto case AIState.Fight;
                    if (moveTiles.Any() && !SeePath())
                        if (MoraleCheck(.5, true))
                            PathToCore = Game.Map.PathFindCore(Tile, GetPathFindingMovement(), _ => true);
                        else
                            goto case AIState.Patrol;
                    break;
                default: throw new Exception();
            }

            if (state == AIState.Retreat)
                path = RetreatPath;
            else if (state == AIState.Rush)
                path = PathToCore;

            this._state = state;
            return state;

            bool PlayerThreat() => targeted || killables.Any(k => k.Piece.HasBehavior<IAttacker>()) || playerAttacks.ContainsKey(Tile);
            bool PlayerPassive() => !PlayerThreat() && killables.Any();
            bool SeePath(List<Point> path = null) => (path ?? PathToCore).Any(p => moveTiles.Contains(Game.Map.GetTile(p)));
            bool NeedsRetreatPath() => RetreatPath == null || !RetreatPath.Any() || !ValidRetreat(RetreatPath[^1]) || !SeePath(RetreatPath);
            bool ValidRetreat(Point point) => ValidRetreatTile(Game.Map.GetTile(point));
            bool ValidRetreatTile(Tile tile) => tile != null && (Game.TEST_MAP_GEN.HasValue || Game.GameOver || !tile.ShowMove()) && !playerAttacks.ContainsKey(tile);
            Tile GetRetreatTo()
            {
                if (RetreatPath != null && Game.Rand.Bool())
                    return Game.Rand.Iterate(RetreatPath.Where(ValidRetreat)).Select(Game.Map.GetTile).FirstOrDefault();
                return null;
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

            double pct = DefPct();
            pct = 1 - .65 * pct * pct * pct;
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
            GetPathFindingMovement(new(movable));
        internal static double GetPathFindingMovement(IMovable.Values movable) =>
            (movable.MoveInc + movable.MoveMax) / 2.0;

        public override string ToString()
        {
            return "Alien " + PieceNum;
        }
    }
}
