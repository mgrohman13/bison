using ClassLibrary1.Pieces.Terrain;
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
        private readonly IKillable killable;
        private readonly IMovable movable;
        private readonly IRepair repair;
        private readonly IBuilder.IBuildExtractor buildExtractor;
        private readonly IBuilder.IBuildFoundation BuildFoundation;

        public Piece Piece => this;

        private Constructor(Map.Tile tile, double vision, IKillable.Values killable, IMovable.Values movable, IRepair.Values repair)
            : base(tile, vision)
        {
            this.killable = new Killable(this, killable);
            this.movable = new Movable(this, movable);
            this.repair = new Repair(this, repair);
            this.buildExtractor = new Builder.BuildExtractor(this);
            this.BuildFoundation = new Builder.BuildFoundation(this);
            SetBehavior(this.killable, this.movable, this.repair, this.buildExtractor, this.BuildFoundation);
        }
        internal static Constructor NewConstructor(Map.Tile tile, bool? defType)
        {
            double hits = 50;
            double moveInc = 3, moveMax = 7, moveLimit = 12;
            double armor = 0, shieldInc = 0, shieldMax = 0, shieldLimit = 0;
            if (!defType.HasValue)
                defType = Game.Rand.Bool();
            if (defType.Value)
                armor = .3;
            else
            {
                shieldInc = 1;
                shieldMax = 25;
                shieldLimit = 50;
            }

            double repairRange = Game.Rand.GaussianOE(Math.PI, .21, .26, 1);
            double repairRate = .065 * Math.Pow(2.6 / repairRange, .78);

            double vision = 4.5;

            double researchMult = Math.Pow(tile.Map.Game.Player.GetResearchMult(), .4);
            hits *= researchMult;
            if (armor > 0)
                armor = 1 - Math.Pow(1 - armor, researchMult);
            shieldInc *= researchMult;
            shieldMax *= researchMult;
            shieldLimit *= researchMult;
            moveInc *= researchMult;
            moveMax *= researchMult;
            moveLimit *= researchMult;
            repairRange *= researchMult;
            repairRate *= researchMult;
            vision *= researchMult;

            Constructor obj = new(tile, vision, new(hits, .39, armor, shieldInc, shieldMax, shieldLimit), new(moveInc, moveMax, moveLimit), new(repairRange, repairRate));
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass)
        {
            double researchMult = Math.Pow(game.Player.GetResearchMult(), .3);
            energy = 750 / researchMult;
            mass = 750 / researchMult;
        }

        double IKillable.IRepairable.RepairCost => GetRepairCost();
        public double GetRepairCost()
        {
            Cost(Game, out double energy, out double mass);
            return Consts.GetRepairCost(energy, mass);
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            energyUpk += Consts.BaseConstructorUpkeep;
        }

        public override string ToString()
        {
            return "Constructor " + PieceNum;
        }
    }
}
