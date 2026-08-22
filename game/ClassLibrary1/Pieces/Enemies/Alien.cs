using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using IRepairable = ClassLibrary1.Pieces.Behavior.Combat.IKillable.IRepairable;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Alien : EnemyPiece, IRepairable, IDeserializationCallback
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        //private readonly bool _autoRepair;
        private readonly double _energy, _research;

        private double _morale;
        private bool _targeted;
        //private List<Point> _path;

        //var defs = killable.AllDefenses.Select(d => new Values(d.Type, d.DefenseMax));
        //var atts = attacker.Attacks.Select(a => new IAttacker.Values(a.Type, a.AttackMax, a.Range));
        //var move = new IMovable.Values(movable.MoveInc, movable.MoveMax, movable.MoveLimit);
        //MechBlueprint.CalcCost(Enemy.GetDifficulty(Game), 0, defs, killable.Resilience, atts, move, out double e, out double m);
        double IRepairable.RepairCost => Consts.GetRepairCost(this, Cost, 0);
        bool IRepairable.AutoRepair => !Tile.Visible;

        internal double Morale => _morale;
        private List<Point> PathToCore { get; set; }
        private List<Point> RetreatPath { get; set; }

        //private double Morale => _morale;// hide behind research

        private Alien(Tile tile, List<Point> pathToCore, double energy, int research,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
            : base(tile, Game.Rand.Bool() ? AIState.Patrol : AIState.Rush)
        {
            //this._autoRepair = Game.Rand.Bool(.078);//
            this._energy = energy;
            this._research = research;
            this._morale = Game.Rand.Weighted(.91);
            this._targeted = false;
            this.PathToCore = pathToCore;
            this.RetreatPath = [Tile.Location];

            this.killable = new Killable(this, killable, resilience);
            this.attacker = new Attacker(this, attacks);
            this.movable = new Movable(this, movable);
            SetBehavior(this.killable, this.attacker, this.movable);

            OnDeserialization(this);
        }
        internal static Alien NewAlien(Tile tile, List<Point> pathToCore, double energy, int research,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
        {
            Alien obj = new(tile, pathToCore, energy, research, killable, resilience, attacks, movable);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        internal override double Cost => _energy
            * Research.GetResearchMult(Game.Consts, _research) / Research.GetResearchMult(Game.Consts, Game.Enemy.Research.GetBlueprintLevel()); //Math.Sqrt()

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
            this._targeted = true;
            this._morale *= Game.Rand.Weighted(DefPct());
        }
        protected override void OnDeath(EnemyPiece enemyPiece) //?
        {
            double offset = Game.Enemy.IncomeReference() * 3.9;
            double cost = enemyPiece.Cost;

            offset += this.Cost;
            double pct = offset / (cost + offset);

            pct = Math.Pow(pct, EventDistMult(enemyPiece.Tile, false, .91));

            this._morale *= Game.Rand.Weighted(pct);
        }
        internal static void IncMorale(EnemyPiece source, double moraleMult, bool distOffset, double distPow, params Tile[] tiles)
        {
            moraleMult = Math.Pow(moraleMult, 1.69 / tiles.Length);
            foreach (Alien alien in Game.Rand.Iterate(source.Side.PiecesOfType<Alien>()))
                if (alien != source)
                    foreach (Tile tile in Game.Rand.Iterate(tiles))
                    {
                        double distMult = alien.EventDistMult(tile, distOffset, distPow);
                        alien.IncMorale(Game.Rand.Range(1, moraleMult), distMult);
                    }
        }
        private void IncMorale(double moraleMult, double distMult)
        {
            double pct = DefPct();

            bool neg = moraleMult < 1;
            if (neg)
            {
                pct = 1 / pct;
                moraleMult = 1 / moraleMult;
            }
            moraleMult--;

            moraleMult *= pct * pct * distMult;
            if (Game.Rand.Bool())
                moraleMult *= distMult;

            moraleMult++;
            if (neg)
                moraleMult = 1 / moraleMult;

            if (neg)
                this._morale *= moraleMult;
            else
                this._morale = 1 - (1 - Morale) / moraleMult;

            if (_state != AIState.Heal || Game.Rand.Bool())
            {
                moraleMult *= moraleMult;
                if (moraleMult < 1 && Game.Rand.Bool(1 - moraleMult))
                {
                    if (MoraleCheck(.5 / moraleMult, false))
                        _state = AIState.Retreat;
                }
                else if (moraleMult > 1 && Game.Rand.Bool(1 - 1 / moraleMult))
                {
                    if (MoraleCheck(.75 / moraleMult, true))
                        _state = AIState.Rush;
                    else if (MoraleCheck(.25 / moraleMult, true))
                        _state = AIState.Fight;
                }
            }
        }
        private double EventDistMult(Tile tile, bool offset, double pow) =>
            Math.Pow(Game.Consts.CaveSize / (Tile.GetDistance(tile) + (offset ? Game.Consts.CaveSize : 1)), pow);

        internal static void ModState(EnemyPiece source, AIState prev, AIState state, Tile orig, Tile cur)
        {
            if (orig == cur)
                orig = null;
            if (prev != state)
                foreach (Alien alien in Game.Rand.Iterate(source.Side.PiecesOfType<Alien>()))
                    if (alien != source && alien.State != state && (alien.State != AIState.Heal || Game.Rand.Bool()))
                        foreach (Tile tile in Game.Rand.Iterate([orig, cur,]))
                            if (tile != null)
                            {
                                double check = Game.Rand.OE();
                                if (alien.State == prev)
                                    check = Game.Rand.DoubleHalf(check);
                                if (check < alien.EventDistMult(tile, false, 2.1))
                                {
                                    static double Morale(AIState s) => Game.Rand.GaussianCapped(((int)s + 1) * .13, .169);
                                    double sm = Morale(prev);
                                    double am = Morale(alien.State);
                                    double nm = Morale(state);
                                    double boost = (nm - sm) * 2;
                                    if (boost < 0)
                                        boost = -1 / --boost;
                                    else
                                        boost++;
                                    double needed = nm - am;
                                    if (needed >= 0 ? alien.MoraleCheck(needed / boost, true) : alien.MoraleCheck(1 - needed * boost, false))
                                        alien._state = state;
                                }
                            }
        }

        internal override AIState TurnState(double difficulty, double aggression, bool clearPaths, Dictionary<Tile, double> playerAttacks, HashSet<Tile> moveTiles, HashSet<IKillable> killables,
            out List<Point> path)
        {
            aggression *= Morale * (Morale + 1);
            if (clearPaths)
                PathToCore?.Clear();

            AIState state = base.TurnState(difficulty, aggression, clearPaths, playerAttacks, moveTiles, killables, out path);

            if (MoraleCheck(0, false))
                state = AIState.Retreat;

            switch (state)
            {
                case AIState.Heal:
                    state = AIState.Heal;
                    if (!killable.IsRepairing() || PlayerThreat())
                        goto case AIState.Retreat;
                    break;
                case AIState.Retreat:
                    state = AIState.Retreat;
                    if (killable.IsRepairing() && !PlayerThreat())
                        goto case AIState.Heal;
                    if (ValidRetreatTile(Tile))
                        RecoverMorale();
                    if (MoraleCheck(1 / DefPct() / DefPct() / Math.Sqrt(aggression), true))
                        goto case AIState.Patrol;
                    if (moveTiles.Count > 0 && NeedsRetreatPath())
                        RetreatPath = Game.Map.PathFindRetreat(Tile, GetRetreatTiles(), GetPathFindingMovement(), killable.CurDefenseValue, playerAttacks, ValidRetreatTile);
                    break;
                case AIState.Fight:
                    state = AIState.Fight;
                    if (MoraleCheck(DefPct(), true))
                        goto case AIState.Patrol;
                    break;
                case AIState.Patrol:
                    state = AIState.Patrol;
                    if (killable.IsRepairing())
                        goto case AIState.Heal;
                    if (Game.Rand.Bool(1 - DefPct()))
                        goto case AIState.Retreat;
                    if (MoraleCheck((1 + 3 / aggression) / Math.Sqrt(aggression), true))
                        goto case AIState.Rush;
                    if (PlayerThreat())
                        goto case AIState.Fight;
                    if (PlayerPassive())
                        goto case AIState.Harass;
                    if (MoraleCheck(Game.Rand.Range(1, 2), true) && SeePath())
                        goto case AIState.Rush;
                    break;
                case AIState.Harass:
                    state = AIState.Harass;
                    if (MoraleCheck(PlayerThreat() ? .6 : .4, false))
                        goto case AIState.Fight;
                    if (!PlayerThreat() && !PlayerPassive())
                        if (MoraleCheck(1 / DefPct(), true))
                            goto case AIState.Rush;
                        else
                            goto case AIState.Patrol;
                    break;
                case AIState.Rush:
                    state = AIState.Rush;
                    if (PlayerPassive())
                        goto case AIState.Harass;
                    if (MoraleCheck((PlayerThreat() ? .7 : .3) / Math.Sqrt(DefPct()) / aggression, false))
                        goto case AIState.Fight;
                    if (moveTiles.Count > 0 && !SeePath())
                        if (MoraleCheck(.5 / aggression, true))
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

            //Tile.GetLinePoints
            bool PlayerThreat() => _targeted || playerAttacks.ContainsKey(Tile) || killables.Any(k => k.Piece.HasBehavior<IAttacker>());
            bool PlayerPassive() => !PlayerThreat() && killables.Count > 0;
            bool SeePath(List<Point> path = null) => (path ?? PathToCore).Any(p => moveTiles.Contains(Game.Map.GetTile(p)));
            bool NeedsRetreatPath() => !ValidRetreatTile(Tile) && (RetreatPath == null || RetreatPath.Count == 0 || !ValidRetreat(RetreatPath[^1]) || !SeePath(RetreatPath));
            bool ValidRetreat(Point point) => ValidRetreatTile(Game.Map.GetTile(point));
            bool ValidRetreatTile(Tile tile) => this.ValidRetreatTile(tile, playerAttacks);
            IEnumerable<Tile> GetRetreatTiles() => RetreatPath?.Where(ValidRetreat).Select(Game.Map.GetTile);
            bool MoraleCheck(double check, bool sign) => this.MoraleCheck(check, sign, difficulty);
        }
        internal bool ValidRetreatTile(Tile tile, Dictionary<Tile, double> playerAttacks) =>
            tile != null && !playerAttacks.ContainsKey(tile) && (tile.Piece == null || tile.Piece == this)
                && (Game.TEST_MAP_GEN.HasValue || Game.GameOver || (!tile.Visible && !tile.ShowMove()));
        private bool MoraleCheck(double check, bool sign, double difficulty = 1)
        {
            double morale = Morale;
            if (sign)
            {
                double mult = Math.Sqrt((difficulty - 1) / difficulty);
                morale = Game.Rand.OE(morale) + Game.Rand.OE(morale * morale * mult);
            }
            else
            {
                morale = Math.Sqrt(morale) - Game.Rand.OE(1 - morale);
            }
            return sign ? morale > check : morale < check;
        }

        internal override void StartTurn()
        {
            base.StartTurn();
            this._targeted = false;
            RecoverMorale();
        }
        internal void RecoverMorale()
        {
            if (Morale < 0)
            {
                Debug.WriteLine($"!!! {this} morale: {Morale}");
                this._morale = 0;
            }

            double pct = DefPct();
            pct = 1 - .65 * pct * pct * pct;
            this._morale = float.Epsilon + Math.Pow(Morale, Game.Rand.Range(pct, 1));

            if (Morale > 1)
            {
                Debug.WriteLine($"!!! {this} morale: {Morale}");
                this._morale = 1;
            }
        }

        private double DefPct()
        {
            double pct = Consts.GetDamagedValue(this, 1, 0);
            pct *= Math.Sqrt(killable.CurDefenseValue / killable.MaxDefenseValue);
            return pct;
        }

        public bool CanRepair() => Consts.CanRepair(this);

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
