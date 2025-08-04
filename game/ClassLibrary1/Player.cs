using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Players;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1
{
    [Serializable]
    public class Player : Side
    {
        private Core _core;
        public readonly Research Research;
        private readonly IEnumerable<IUpgradeValues> upgradeValues;

        new public IReadOnlyList<Piece> Pieces => base.Pieces;
        new public IEnumerable<T> PiecesOfType<T>() where T : class, IBehavior
        {
            return base.PiecesOfType<T>();
        }
        public Core Core => _core;
        new public int Energy => base.Energy;
        new public int Mass => base.Mass;

        internal Player(Game game)
            : base(game, 0, 1000)
        {
            this.Research = new(game);
            this.upgradeValues = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && typeof(IUpgradeValues).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .OfType<IUpgradeValues>()
                .ToArray();
        }
        internal void NewGame(Point constructorOffset)
        {
            const double stdDev = Consts.PathWidth / 1.3;
            Tile tile;
            do
            {
                tile = Game.Map.GetTile(Game.Rand.GaussianInt(stdDev), Game.Rand.GaussianInt(stdDev));

                if (tile != null)
                {
                    var checkTiles = Tile.GetPointsInRangeUnblocked(Game.Map, tile.Location, Core.START_VISION);
                    checkTiles = checkTiles.Union(Tile.GetPointsInRangeUnblocked(Game.Map, new(tile.X + constructorOffset.X, tile.Y + constructorOffset.Y), Constructor.BASE_VISION));
                    if (checkTiles.Select(Game.Map.GetTile).Any(t => t == null || t.Piece != null))
                        tile = null;
                }
            }
            while (tile == null);

            this._core = Core.NewCore(tile);
        }

        internal T GetUpgradeValues<T>() where T : IUpgradeValues
        {
            return upgradeValues.OfType<T>().Single();
        }
        internal void OnResearch(Research.Type type, double researchMult)
        {
            foreach (IUpgradeValues values in Game.Rand.Iterate(upgradeValues))
                values.Upgrade(type, researchMult);
            foreach (PlayerPiece piece in Game.Rand.Iterate(Pieces.Cast<PlayerPiece>()))
                piece.OnResearch(type);
        }

        public bool CanBurnMass()
        {
            return Research.HasType(Research.Type.BurnMass);
        }
        public bool CanFabricateMass()
        {
            return Research.HasType(Research.Type.FabricateMass);
        }
        public bool CanScrapResearch()
        {
            return Research.HasType(Research.Type.ScrapResearch);
        }
        public void Trade(int burnMass, int fabricateMass, int scrapResearch)
        {
            if (burnMass <= 0 || !CanBurnMass())
                burnMass = 0;
            if (fabricateMass <= 0 || !CanFabricateMass())
                fabricateMass = 0;
            if (scrapResearch <= 0 || !CanScrapResearch())
                scrapResearch = 0;

            if (Research.HasScrap(scrapResearch) && Spend(fabricateMass * Consts.EnergyPerFabricateMass, burnMass * Consts.BurnMassPerEnergy))
            {
                Research.Scrap(scrapResearch);
                this._energy += burnMass;
                this._mass += fabricateMass + scrapResearch * Consts.MassForScrapResearch;
            }
        }

        internal void AddResources(int energy, int mass)
        {
            this._energy += energy;
            this._mass += mass;
        }
        internal override bool Spend(int energy, int mass)
        {
            bool has = Has(energy, mass);
            if (has)
                AddResources(-energy, -mass);
            return has;
        }
        public bool Has(double energy, double mass)
        {
            return ((Energy >= energy || energy <= 0) && (Mass >= mass || mass <= 0));
        }

        public Dictionary<Type, double[]> GetIncomeDetails()
        {
            Dictionary<Type, double[]> details = new();
            foreach (PlayerPiece p in Pieces.Cast<PlayerPiece>())
            {
                double energyInc, massInc, researchInc, energyUpk, massUpk, researchUpk;
                energyInc = massInc = researchInc = energyUpk = massUpk = researchUpk = 0;
                p.GenerateResources(ref energyInc, ref massInc, ref researchInc);
                p.GetUpkeep(ref energyUpk, ref massUpk);

                static void MoveNeg(ref double v1, ref double v2)
                {
                    if (v1 < 0)
                    {
                        v2 -= v1;
                        v1 = 0;
                    }
                }
                MoveNeg(ref energyInc, ref energyUpk);
                MoveNeg(ref energyUpk, ref energyInc);
                MoveNeg(ref massInc, ref massUpk);
                MoveNeg(ref massUpk, ref massInc);
                MoveNeg(ref researchInc, ref researchUpk);
                MoveNeg(ref researchUpk, ref researchInc);

                Type type = p.GetType();
                if (!details.TryGetValue(type, out double[] row))
                    details[type] = row = new double[7];
                row[0]++;
                row[1] += energyInc;
                row[2] += -energyUpk;
                row[3] += massInc;
                row[4] += -massUpk;
                row[5] += researchInc;
                row[6] += -researchUpk;
            }
            return details;
        }

        public void GetIncome(out double energyInc, out double massInc, out double researchInc)
        {
            energyInc = massInc = researchInc = 0;
            foreach (PlayerPiece piece in Game.Rand.Iterate(Pieces.Cast<PlayerPiece>()))
                piece.GetIncome(ref energyInc, ref massInc, ref researchInc);
            PostProcess(ref energyInc, ref massInc, ref researchInc);
        }
        internal void GenerateResources(out double energyInc, out double massInc, out double researchInc)
        {
            energyInc = massInc = researchInc = 0;
            foreach (PlayerPiece piece in Game.Rand.Iterate(Pieces.Cast<PlayerPiece>()))
                piece.GenerateResources(ref energyInc, ref massInc, ref researchInc);
        }
        internal new void StartTurn()
        {
            base.StartTurn();
        }
        internal Research.Type? EndTurn()
        {
            GenerateResources(out double energyInc, out double massInc, out double researchInc);

            base.EndTurn(out double energyUpk, out double massUpk);
            PostProcess(ref energyInc, ref massInc, ref researchInc);

            this._energy = Consts.Income(Energy, energyInc - energyUpk);
            this._mass = Consts.Income(Mass, massInc - massUpk);

            return this.Research.AddResearch(researchInc, out _);
        }

        private static void PostProcess(ref double energyInc, ref double massInc, ref double researchInc)
        {
            if (researchInc < 0)
            {
                energyInc += researchInc * Consts.MassForScrapResearch * Consts.EnergyPerFabricateMass;
                researchInc = 0;
            }
        }
    }
}
