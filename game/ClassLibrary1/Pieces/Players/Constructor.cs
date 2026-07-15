using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Constructor : PlayerPiece, IKillable.IRepairable
    {
        public const double BASE_VISION = 6.5, MOVE_RAMP = 1.3, BASE_MOVE_INC = 4.5, BASE_MOVE_MAX = 10 * MOVE_RAMP;
        public static double Resilience => Values.Resilience;

        private bool _canUpgrade;
        private readonly bool _defenseType;
        private readonly double _defMult, _rangeMult, _rounding;

        public bool CanUpgrade => _canUpgrade;

        private Constructor(Tile tile, Values values, bool starter)
            : base(tile, starter ? BASE_VISION : values.Vision)
        {
            this._canUpgrade = !starter;

            this._defenseType = Game.Rand.Bool();
            this._defMult = Game.Rand.GaussianCapped(1, .091, .65);

            this._rangeMult = 1;
            this._rounding = 1;
            if (!starter)
            {
                this._rangeMult = Game.Rand.GaussianOE(values.Range, .13, .065, Attack.MELEE_RANGE) / values.Range;
                this._rounding = Game.Rand.NextDouble();
            }

            SetBehavior(
                    new Killable(this, new IKillable.Values(DefenseType.Hits, ResearchUpgValues.ConstructorStartDef), Values.Resilience),
                    new Movable(this, Values.GetStartMovable(), 0),
                    new Builder.BuildExtractor(this, new(ResearchUpgValues.ConstructorStartRange)));
            Unlock();
        }
        internal static Constructor NewConstructor(Tile tile, bool starter)
        {
            Constructor obj = new(tile, GetValues(tile.Map.Game), starter);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }
        internal override void Cost(out int energy, out int mass) =>
            Cost(Game, out energy, out mass);

        internal override void OnResearch(Research.Type type)
        {
            _canUpgrade |= Values.CanUpgrade(type);
            Unlock();
        }
        private bool Upgrade()
        {
            //check blocks
            if (CanUpgrade && Side.PiecesOfType<IBuilder.IBuildConstructor>().Any(b => Tile.GetDistance(b.Piece.Tile) <= b.Range))
            {
                Values values = GetValues(Game);

                GetBehavior<IKillable>().Upgrade(values.GetKillable(Game, _defenseType, _defMult, _rounding), Values.Resilience);
                GetBehavior<IMovable>().Upgrade(values.GetMovable(_rangeMult, _rounding));
                Builder.UpgradeAll(this, values.GetBuilder(_rangeMult));

                this.Vision = values.Vision;
                this._canUpgrade = false;

                return true;
            }
            return false;
        }
        private void Unlock()
        {
            Research research = Game.Player.Research;

            if (!HasBehavior<IBuilder.IBuildOutpost>() && research.HasType(Research.Type.Outpost))
                SetBehavior(new Builder.BuildOutpost(this, new()));
            if (!HasBehavior<IBuilder.IBuildFactory>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, new()));
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, new()));
            if (!HasBehavior<IBuilder.IBuildGenerator>() && research.HasType(Research.Type.AmbientGenerator))
                SetBehavior(new Builder.BuildGenerator(this, new()));
            if (!HasBehavior<IBuilder.IBuildDrone>() && research.HasType(Research.Type.RepairDrone))
                SetBehavior(new Builder.BuildDrone(this, new()));

            //must upg to an existing builder so the range doesn't change until upgraded
            Builder.UpgradeAll(this, new(GetBehavior<IBuilder.IBuildExtractor>()));

            Upgrade();
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass);
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => false;
        public bool CanRepair() => Consts.CanRepair(this);

        internal override void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            base.GetUpkeep(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
            Upgrade();
        }

        public override string ToString()
        {
            return "Constructor " + PieceNum;
        }

        internal static IMovable.Values GetMove(double mult, double incAvg, double maxAvg, double limitAvg, double maxRound, double limitRound)
        {
            int max = MTRandom.Round(maxAvg * mult, maxRound);
            int limit = MTRandom.Round(limitAvg * mult, limitRound);
            if (max >= limit)
                limit = max + 1;

            double inc;
            bool loop;
            do
            {
                double m = Math.Sqrt(maxAvg / max * Math.Sqrt(limitAvg / limit));
                inc = incAvg * mult * Math.Sqrt(m);

                loop = inc + 1 > max;
                if (loop)
                {
                    limit += Game.Rand.Round(limit / (double)max);
                    max++;
                }
            } while (loop);

            return new IMovable.Values(inc, max, limit);
        }

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            private const double resilience = .4;

            private int energy, mass;
            private double def, vision, range, moveInc, moveMax, moveLimit;

            public Values()
            {
                UpgradeConstructorCost(1);
                UpgradeConstructorDefense(1);
                UpgradeConstructorMove(1);
            }

            public static double Resilience => resilience;
            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double Range => range;

            public List<IKillable.Values> GetKillable(Game game, bool defenseType, double defMult, double rounding)
            {
                bool hasDef = game.Player.Research.HasType(Research.Type.ConstructorDefense);
                if (!hasDef)
                    defMult = 1;

                int hits = MTRandom.Round(this.def * defMult, rounding);
                hits = Math.Max(hits, 1);
                List<IKillable.Values> defenses = [new IKillable.Values(DefenseType.Hits, hits)];

                if (hasDef)
                {
                    double avg = this.def * Consts.StatValueInverse(Math.Sqrt(Consts.StatValue(this.def) / Consts.StatValue(hits)));
                    int def = MTRandom.Round((defenseType ? 1.13 : 1.69) * avg, rounding);
                    def = Math.Max(def, 1);
                    defenses.Add(new IKillable.Values(defenseType ? DefenseType.Shield : DefenseType.Armor, def));
                }

                return defenses;
            }
            public IMovable.Values GetMovable(double rangeMult, double rounding)
            {
                return GetMove(1 / Math.Sqrt(rangeMult), this.moveInc, this.moveMax, this.moveLimit, Consts.MAX_ROUND - rounding, rounding);
            }
            public IBuilder.Values GetBuilder(double rangeMult)
            {
                double range = this.range * rangeMult;
                range = Math.Max(range, 1);
                return new(range);
            }
            public static IMovable.Values GetStartMovable()
            {
                GetStartMovable(out double inc, out double max, out double limit);
                return new(inc, (int)Math.Round(max), (int)Math.Round(limit));
            }

            public static bool CanUpgrade(Research.Type type) => type switch
            {
                Research.Type.ConstructorCost => false,
                Research.Type.ConstructorDefense => true,
                Research.Type.ConstructorMove => true,
                _ => false
            };
            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.ConstructorCost)
                    UpgradeConstructorCost(researchMult);
                else if (type == Research.Type.ConstructorDefense)
                    UpgradeConstructorDefense(researchMult);
                else if (type == Research.Type.ConstructorMove)
                    UpgradeConstructorMove(researchMult);
            }
            private void UpgradeConstructorCost(double researchMult)
            {
                double costMult = ResearchUpgValues.Calc(UpgType.ConstructorCost, researchMult);
                this.energy = this.mass = Game.Rand.Round(1250 * costMult);
            }
            private void UpgradeConstructorDefense(double researchMult)
            {
                this.def = ResearchUpgValues.Calc(UpgType.ConstructorDefense, researchMult);
            }
            private void UpgradeConstructorMove(double researchMult)
            {
                this.vision = ResearchUpgValues.Calc(UpgType.ConstructorVision, researchMult);
                this.range = ResearchUpgValues.Calc(UpgType.ConstructorRange, researchMult);

                double moveMult = ResearchUpgValues.Calc(UpgType.ConstructorMove, researchMult) / BASE_MOVE_INC;
                GetStartMovable(out double inc, out double max, out double limit);
                this.moveInc = inc * moveMult;
                this.moveMax = max * moveMult;
                this.moveLimit = limit * moveMult;
            }
            private static void GetStartMovable(out double inc, out double max, out double limit)
            {
                inc = BASE_MOVE_INC;
                max = BASE_MOVE_MAX / MOVE_RAMP;
                limit = 2 * max;
            }
        }
    }
}
