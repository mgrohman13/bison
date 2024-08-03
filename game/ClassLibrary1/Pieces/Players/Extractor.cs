using ClassLibrary1.Pieces.Terrain;
using System;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Extractor : PlayerPiece, IKillable.IRepairable
    {
        public readonly Resource Resource;

        public double Sustain => Resource.Sustain * GetValues(Game).SustainMult;

        private Extractor(Tile tile, Resource Resource, Values values)
            : base(tile, values.Vision)
        {
            SetBehavior(new Killable(this, values.Killable, values.Resilience));

            this.Resource = Resource;
        }
        internal static Extractor NewExtractor(Resource resource)
        {
            Tile tile = resource.Tile;
            resource.Die();

            Extractor obj = new(tile, resource, GetValues(resource.Game));
            resource.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(out int energy, out int mass, Resource resource)
        {
            resource.GetCost(GetValues(resource.Game).CostMult, out energy, out mass);
        }

        internal override void OnResearch(Research.Type type)
        {
            Values values = GetValues(Game);

            this.Vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(new[] { values.Killable }, values.Resilience);
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass, Resource);
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.ExtractorAutoRepair);
        public bool CanRepair() => Consts.CanRepair(this);

        internal override void Die()
        {
            Die(true);
        }
        private void Die(bool resource)
        {
            Tile tile = this.Tile;
            base.Die();

            if (resource && VanishStr() > Game.Rand.GaussianOE(6.5, .39, .39))
            {
                Resource.SetTile(tile);
            }
            else
            {
                Resource.GetCost(1, out int energy, out int mass);
                Treasure.NewTreasure(tile, energy + mass * Consts.MechMassDiv);
            }
        }

        internal override void StartTurn()
        {
            if (VanishStr() < Math.Min(Game.Rand.OEInt(), Game.Rand.OE()))
                Die(false);
        }
        private double VanishStr()
        {
            double energyInc, massInc, researchInc;
            energyInc = massInc = researchInc = 0;
            Resource.GenerateResources(ref energyInc, ref massInc, ref researchInc);
            return Math.Abs(energyInc) + Math.Abs(massInc) + Math.Abs(researchInc);
        }


        internal override void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
            base.GenerateResources(ref energyInc, ref massInc, ref researchInc);
            Resource.GenerateResources(this, GetValues(Game).ValueMult, ref energyInc, ref massInc, ref researchInc);
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            Values values = GetValues(Game);
            Resource.Extract(this, values.ValueMult, values.SustainMult);

            //will end up being slightly cheaper to repair than in GetUpkeep because of extracted resource value
            base.EndTurn(ref energyUpk, ref massUpk);
        }

        public override string ToString()
        {
            return Resource.GetResourceName() + " Extractor " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .3;//resilienceBase

            private double costMult, vision, valueMult, sustainMult;//, resilience;
            private IKillable.Values killable;
            public Values()
            {
                this.killable = new(DefenseType.Hits, -1);
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeExtractorValue(1);
            }

            public double Resilience => resilience;
            public double CostMult => costMult;
            public double Vision => vision;
            public double ValueMult => valueMult;
            public double SustainMult => sustainMult;
            public IKillable.Values Killable => killable;

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.ExtractorValue)
                    UpgradeExtractorValue(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                this.costMult = ResearchUpgValues.Calc(UpgType.ExtractorCost, researchMult);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                double defAvg = ResearchUpgValues.Calc(UpgType.ExtractorDefense, researchMult);
                int defense = Game.Rand.Round(defAvg);
                this.vision = ResearchUpgValues.Calc(UpgType.ExtractorVision, researchMult);
                this.killable = new(DefenseType.Hits, defense);
            }
            private void UpgradeExtractorValue(double researchMult)
            {
                //this.resilience = Consts.GetPct(resilienceBase, Math.Pow(researchMult, Extractor_Resilience));

                this.valueMult = ResearchUpgValues.Calc(UpgType.ExtractorValue, researchMult);
                this.sustainMult = ResearchUpgValues.Calc(UpgType.ExtractorSustain, researchMult);
            }
        }
    }
}
