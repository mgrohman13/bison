﻿using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Constructor : PlayerPiece, IKillable.IRepairable
    {
        private readonly bool _defenseType;
        private readonly double _rangeMult;
        public Piece Piece => this;

        private Constructor(Map.Tile tile, Values values, bool starter)
            : base(tile, values.Vision)
        {
            this._defenseType = Game.Rand.Bool();
            this._rangeMult = 1;
            if (!starter)
                this._rangeMult = Game.Rand.GaussianOE(values.RepairRange, .21, .26, 1) / values.RepairRange;

            SetBehavior(
                new Killable(this, values.GetKillable(Game.Player.Research, _defenseType)),
                new Movable(this, values.Movable),
                new Repair(this, values.GetRepair(_rangeMult)),
                new Builder.BuildExtractor(this, values.GetRepair(_rangeMult).Builder));
            Unlock(Game.Player.Research);
        }
        internal static Constructor NewConstructor(Map.Tile tile, bool starter)
        {
            Constructor obj = new(tile, GetValues(tile.Map.Game), starter);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock(Game.Player.Research);
            Values values = GetValues(Game);
            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _defenseType));
            GetBehavior<IMovable>().Upgrade(values.Movable);
            GetBehavior<IRepair>().Upgrade(values.GetRepair(_rangeMult));
        }
        private void Unlock(Research research)
        {
            Values values = GetValues(Game);
            if (!HasBehavior<IBuilder.IBuildTurret>() && research.HasType(Research.Type.Turret))
                SetBehavior(new Builder.BuildTurret(this, values.GetRepair(_rangeMult).Builder));
            if (!HasBehavior<IBuilder.IBuildExtractor>() && research.HasType(Research.Type.Factory))
                SetBehavior(new Builder.BuildFactory(this, values.GetRepair(_rangeMult).Builder));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out double energy, out double mass);
                return Consts.GetRepairCost(energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => false;

        internal override void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            base.GetUpkeep(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
        }

        public override string ToString()
        {
            return "Constructor " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private double energy, mass, vision;
            private IKillable.Values killable;
            private IMovable.Values movable;
            private IRepair.Values repair;
            public Values()
            {
                UpgradeConstructorCost(1);
                UpgradeConstructorDefense(1);
                UpgradeConstructorMove(1);
                UpgradeConstructorRepair(1);
            }

            public double Energy => energy;
            public double Mass => mass;
            public double Vision => vision;
            public IMovable.Values Movable => movable;
            public double RepairRange => repair.Builder.Range;
            public IKillable.Values GetKillable(Research research, bool defenseType)
            {
                double armor = research.HasType(Research.Type.ConstructorDefense) && !defenseType ? killable.Armor : 0;
                bool shields = research.HasType(Research.Type.ConstructorDefense) && defenseType;
                double shieldInc = shields ? killable.ShieldInc : 0;
                double shieldMax = shields ? killable.ShieldMax : 0;
                double shieldLimit = shields ? killable.ShieldLimit : 0;
                return new IKillable.Values(killable.HitsMax, killable.Resilience, armor, shieldInc, shieldMax, shieldLimit);
            }
            public IRepair.Values GetRepair(double rangeMult)
            {
                IRepair.Values repair = this.repair;
                double range = repair.Builder.Range * rangeMult;
                double rate = Consts.GetPct(repair.Rate, 1 / Math.Pow(rangeMult, .65));
                return new(new(range), rate);
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.ConstructorCost)
                    UpgradeConstructorCost(researchMult);
                else if (type == Research.Type.ConstructorDefense)
                    UpgradeConstructorDefense(researchMult);
                else if (type == Research.Type.ConstructorMove)
                    UpgradeConstructorMove(researchMult);
                else if (type == Research.Type.ConstructorRepair)
                    UpgradeConstructorRepair(researchMult);
            }
            private void UpgradeConstructorCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .5);
                this.energy = 750 / researchMult;
                this.mass = 750 / researchMult;
            }
            private void UpgradeConstructorDefense(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .8);
                double armor = Consts.GetPct(1 / 3.0, researchMult);
                double shieldInc = 1.69 * researchMult;
                double shieldMax = 26 * researchMult;
                double shieldLimit = 52 * researchMult;
                this.vision = 4.5 * researchMult;
                this.killable = new(50, .35, armor, shieldInc, shieldMax, shieldLimit);
            }
            private void UpgradeConstructorMove(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .4);
                double moveInc = 3 * researchMult;
                double moveMax = 7 * researchMult;
                double moveLimit = 12 * researchMult;
                this.movable = new(moveInc, moveMax, moveLimit);
            }
            private void UpgradeConstructorRepair(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .5);
                double repairRange = 3.5 * researchMult;
                double repairRate = Consts.GetPct(.05, researchMult);
                this.repair = new(new(repairRange), repairRate);
            }
        }
    }
}
