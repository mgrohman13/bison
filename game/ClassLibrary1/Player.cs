using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Players;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Player : Side
    {
        private Core _core;
        public readonly Research Research;
        private readonly IEnumerable<IUpgradeValues> upgradeValues;

        private double _researchRand, _researchRound;

        new public IReadOnlyList<Piece> Pieces => base.Pieces;
        new public IEnumerable<T> PiecesOfType<T>() where T : class, IBehavior
            => base.PiecesOfType<T>();

        public Core Core => _core;
        new public int Energy => base.Energy;
        new public int Mass => base.Mass;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Must be serializable")]
        internal Player(Game game)
            : base(game, 0, 1000)
        {
            this.Research = new(game);
            this.upgradeValues = Game.Rand.Iterate(AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && typeof(IUpgradeValues).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .OfType<IUpgradeValues>()).ToList();
            foreach (var upgradeValue in upgradeValues)
                upgradeValue.Init(game);
        }
        internal void NewGame(Consts consts, Point constructorOffset)
        {
            double stdDev = consts.PathWidth / 1.3;
            Tile tile;
            do
            {
                tile = Game.Map.GetTile(Game.Rand.GaussianInt(stdDev), Game.Rand.GaussianInt(stdDev));

                if (tile != null)
                {
                    var checkTiles = Tile.GetPointsInRange(new(tile.X + constructorOffset.X, tile.Y + constructorOffset.Y), Constructor.START_VISION);
                    if (checkTiles.Select(Game.Map.GetTile).Any(t => t == null || t.Piece != null))
                        tile = null;
                }
            }
            while (tile == null);

            this._core = Core.NewCore(tile);
        }

        private IEnumerable<PlayerPiece> IteratePieces() => Game.Rand.Iterate(Pieces.Cast<PlayerPiece>());

        internal T GetUpgradeValues<T>() where T : IUpgradeValues
        {
            return upgradeValues.OfType<T>().Single();
        }
        internal void OnResearch(Research.Type type, double researchMult)
        {
            foreach (IUpgradeValues values in Game.Rand.Iterate(upgradeValues))
                values.Upgrade(Game, type, researchMult);
            foreach (PlayerPiece piece in IteratePieces())
                piece.OnResearch(type);
        }

        public bool CanDisband() => Research.HasType(Research.Type.Disband);

        public bool CanBurnMass() => Research.HasType(Research.Type.BurnMass);
        public bool CanFabricateMass() => Research.HasType(Research.Type.FabricateMass);
        public bool CanScrapResearch() => Research.HasType(Research.Type.ScrapResearch);
        public void Trade(int burnMass, int fabricateMass, int scrapResearch)
        {
            if (burnMass <= 0 || !CanBurnMass())
                burnMass = 0;
            if (fabricateMass <= 0 || !CanFabricateMass())
                fabricateMass = 0;
            if (scrapResearch <= 0 || !CanScrapResearch())
                scrapResearch = 0;

            if (Research.HasScrap(scrapResearch) && Spend(fabricateMass * Game.Consts.EnergyPerFabricateMass, burnMass * Game.Consts.BurnMassPerEnergy))
            {
                Research.Scrap(scrapResearch);
                this._energy += burnMass;
                this._mass += fabricateMass + scrapResearch * Game.Consts.MassForScrapResearch;
            }
        }

        internal override void AddResources(double energy, double mass = 0)
        {
            this._energy += Game.Rand.Round(energy);
            this._mass += Game.Rand.Round(mass);
        }
        internal bool Spend(int energy, int mass)
        {
            bool has = Has(energy, mass);
            if (has)
                AddResources(-energy, -mass);
            return has;
        }
        public bool Has(double energy, double mass) =>
            ((Energy >= energy || energy <= 0) && (Mass >= mass || mass <= 0));

        public Dictionary<Type, double[]> GetIncomeDetails()
        {
            Dictionary<Type, double[]> details = [];
            foreach (PlayerPiece p in IteratePieces())
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

        public void GetIncome(out double energyInc, out double massInc, out int researchInc) =>
            GetIncome(out energyInc, out massInc, out researchInc, out _);
        public void GetIncome(out double energyInc, out double massInc, out int researchInc, out double researchAvg)
        {
            energyInc = massInc = researchAvg = 0;
            foreach (PlayerPiece piece in IteratePieces())
                piece.GetIncome(ref energyInc, ref massInc, ref researchAvg);
            PostProcess(ref energyInc, researchAvg, out researchInc);
        }
        internal void GenerateResources(out double energyInc, out double massInc, out double researchInc)
        {
            energyInc = massInc = researchInc = 0;
            foreach (PlayerPiece piece in IteratePieces())
                piece.GenerateResources(ref energyInc, ref massInc, ref researchInc);
        }
        internal new void StartTurn()
        {
            base.StartTurn();
            this._energy = Consts.IncomeRounding(Energy);
            this._mass = Consts.IncomeRounding(Mass);
            this._researchRand = Game.Rand.Gaussian();
            this._researchRound = Game.Rand.NextDouble();
        }
        internal Research.Type? EndTurn()
        {
            GenerateResources(out double energyInc, out double massInc, out double researchAvg);

            base.EndTurn(out double energyUpk, out double massUpk);
            PostProcess(ref energyInc, researchAvg, out int researchInc);

            this._energy = Game.Consts.Income(Energy, energyInc - energyUpk);
            this._mass = Game.Consts.Income(Mass, massInc - massUpk);

            return this.Research.AddResearch(researchInc);
        }

        private void PostProcess(ref double energyInc, double researchAvg, out int researchInc)
        {
            if (researchAvg < 0)
            {
                energyInc += researchAvg * Game.Consts.MassPerResearchConversion * Game.Consts.EnergyMassRatio;
                researchAvg = 0;
            }
            researchInc = MTRandom.Round(researchAvg + _researchRand * Game.Consts.IncomeDev(researchAvg), _researchRound);
        }
    }
}
