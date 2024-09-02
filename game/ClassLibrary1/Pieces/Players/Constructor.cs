using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Constructor : PlayerPiece, IKillable.IRepairable
    {
        public const double BASE_VISION = 6.5, MOVE_RAMP = 1.3, BASE_MOVE_INC = 4.5, BASE_MOVE_MAX = 10 * MOVE_RAMP;

        private bool _canUpgrade;
        private readonly bool _defenseType;
        private readonly double _defMult, _rangeMult, _rounding;

        private Constructor(Tile tile, Values values, bool starter)
            : base(tile, values.Vision)
        {
            this._canUpgrade = false;

            this._defenseType = Game.Rand.Bool();
            this._defMult = 1;
            this._rangeMult = 1;
            this._rounding = Game.Rand.NextDouble();
            if (!starter)
            {
                this._defMult = Game.Rand.GaussianCapped(1, .091, .65);
                this._rangeMult = Game.Rand.GaussianOE(values.BuilderRange, .13, .065, Attack.MELEE_RANGE) / values.BuilderRange;
            }

            IMovable.Values movable = values.GetMovable(_rangeMult, _rounding);
            SetBehavior(
                new Killable(this, values.GetKillable(Game, _defenseType, _defMult, _rounding), Values.Resilience),
                new Movable(this, movable, 0),// starter ? movable.MoveMax - movable.MoveInc - .5 : 0),
                new Builder.BuildExtractor(this, values.GetBuilder(_rangeMult)));
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

        public bool CanUpgrade => _canUpgrade;
        internal override void OnResearch(Research.Type type)
        {
            switch (type)
            {
                case Research.Type.ConstructorDefense:
                case Research.Type.ConstructorMove:
                    _canUpgrade = true;
                    break;
            }
            Upgrade();
            UnlockBuilds(GetValues(Game));
        }
        private bool Upgrade()
        {
            //check blocks
            if (CanUpgrade && Side.PiecesOfType<IBuilder.IBuildConstructor>().Any(b => b.Piece is not Constructor && Tile.GetDistance(b.Piece.Tile) <= b.Range))
            {
                Unlock();
                Values values = GetValues(Game);

                this.Vision = values.Vision;
                GetBehavior<IKillable>().Upgrade(values.GetKillable(Game, _defenseType, _defMult, _rounding), Values.Resilience);
                GetBehavior<IMovable>().Upgrade(values.GetMovable(_rangeMult, _rounding));
                //if (HasBehavior<IRepair>())
                //    GetBehavior<IRepair>().Upgrade(values.GetRepair(Game, _rangeMult, _rounding));
                Builder.UpgradeAll(this, values.GetBuilder(_rangeMult));

                _canUpgrade = false;
                return true;
            }
            return false;
        }
        private void Unlock()
        {
            Values values = GetValues(Game);
            UnlockBuilds(values);
            //if (!HasBehavior<IRepair>() && Game.Player.Research.HasType(Research.Type.RepairDrone))
            //    SetBehavior(new Repair(this, values.GetRepair(Game, _rangeMult, _rounding)));
        }
        private void UnlockBuilds(Values values)
        {
            Research research = Game.Player.Research;
            IBuilder.Values GetBuilder(Values values) => values.GetBuilder(_rangeMult);

            if (!HasBehavior<IBuilder.IBuildFactory>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, GetBuilder(values)));
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, GetBuilder(values)));
            if (!HasBehavior<IBuilder.IBuildGenerator>() && research.HasType(Research.Type.AmbientGenerator))
                SetBehavior(new Builder.BuildGenerator(this, GetBuilder(values)));
            if (!HasBehavior<IBuilder.IBuildDrone>() && research.HasType(Research.Type.RepairDrone))
                SetBehavior(new Builder.BuildDrone(this, GetBuilder(values)));

            Builder.UpgradeAll(this, new(GetBehavior<IBuilder.IBuildExtractor>()));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out int energy, out int mass);
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

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .4;

            private int energy, mass;
            private double vision;//, repairRate;
            private double hits, shield, armor;
            private IMovable.Values movable;
            //private IRepair.Values repair;
            private IBuilder.Values builder;
            public Values()
            {
                UpgradeConstructorCost(1);
                UpgradeConstructorDefense(1);
                UpgradeConstructorMove(1);
                //UpgradeConstructorRepair(1);
            }

            public static double Resilience => resilience;
            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double BuilderRange => builder.Range;
            public IKillable.Values[] GetKillable(Game game, bool defenseType, double defMult, double rounding)
            {
                int hits = MTRandom.Round(this.hits * defMult, rounding);
                double mult = Math.Sqrt(Consts.StatValueInverse(Consts.StatValue(this.hits) / Consts.StatValue(hits)));
                int def = MTRandom.Round((defenseType ? this.shield : this.armor) * mult, rounding);

                List<IKillable.Values> defenses = new() { new IKillable.Values(DefenseType.Hits, hits) };
                if (game.Player.Research.HasType(Research.Type.ConstructorDefense))
                    defenses.Add(new IKillable.Values(defenseType ? DefenseType.Shield : DefenseType.Armor, def));

                return defenses.ToArray();
            }
            public IMovable.Values GetMovable(double rangeMult, double rounding)
            {
                double mult = 1 / Math.Sqrt(rangeMult);
                double avgMax = 1 + (movable.MoveMax - 1) * mult;
                double avgLimit = 1 + (movable.MoveLimit - 1) * mult;
                int max = MTRandom.Round(avgMax, rounding);
                int limit = MTRandom.Round(avgLimit, rounding);
                avgMax = Math.Sqrt(avgMax / max);
                avgLimit = Math.Sqrt(avgLimit / limit);
                mult *= Math.Sqrt(avgMax * avgLimit);
                double inc = movable.MoveInc * Math.Sqrt(mult);
                return new IMovable.Values(inc, max, limit);
            }
            public IBuilder.Values GetBuilder(double rangeMult)//, double rounding)
            {
                IBuilder.Values builder = this.builder;
                //int rate = Math.Max(1, MTRandom.Round(repairRate / rangeMult, rounding));
                //if (!game.Player.Research.HasType(Research.Type.RepairDrone))
                //    rate = 1;
                double range = builder.Range * rangeMult;
                return new(range);
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.ConstructorCost)
                    UpgradeConstructorCost(researchMult);
                else if (type == Research.Type.ConstructorDefense)
                    UpgradeConstructorDefense(researchMult);
                else if (type == Research.Type.ConstructorMove)
                    UpgradeConstructorMove(researchMult);
                //else if (type == Research.Type.RepairDrone)
                //    UpgradeConstructorRepair(researchMult);
            }
            private void UpgradeConstructorCost(double researchMult)
            {
                double costMult = ResearchUpgValues.Calc(UpgType.ConstructorCost, researchMult);
                this.energy = this.mass = Game.Rand.Round(1250 * costMult);
            }
            private void UpgradeConstructorDefense(double researchMult)
            {
                this.hits = Gen(1);//DefenseType.Hits
                this.shield = Gen(1.23);//DefenseType.Shield
                this.armor = Gen(1.69);//DefenseType.Armor

                double Gen(double mult) =>
                    mult * ResearchUpgValues.Calc(UpgType.ConstructorDefense, researchMult);
            }
            private void UpgradeConstructorMove(double researchMult)
            {
                double range = ResearchUpgValues.Calc(UpgType.ConstructorRange, researchMult);
                this.builder = new(range);

                this.vision = ResearchUpgValues.Calc(UpgType.ConstructorVision, researchMult);

                double moveMult = ResearchUpgValues.Calc(UpgType.ConstructorMove, researchMult) / BASE_MOVE_INC;

                double max = BASE_MOVE_MAX / MOVE_RAMP * moveMult;
                double limit = 20 * moveMult;
                int moveMax = Game.Rand.Round(max);
                int moveLimit = Game.Rand.Round(limit);
                double moveInc = BASE_MOVE_INC * moveMult * Math.Pow((max * 2 + limit) / (moveMax * 2 + moveLimit), 1 / 3.9);
                this.movable = new(moveInc, moveMax, moveLimit);
            }
            //private void UpgradeConstructorRepair(double researchMult)
            //{
            //    double repairMult = ResearchUpgValues.Calc(UpgType.ConstructorRepair, researchMult);
            //    double repairRange = 5.0 * Math.Sqrt(repairMult);
            //    repairRate = 1 * repairMult;
            //    this.repair = new(new(repairRange), 1);
            //}
        }
    }
}
