using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1
{
    [Serializable]
    public class Player : Side
    {
        private Core _core;
        public readonly Research Research;
        private readonly IEnumerable<IUpgradeValues> upgradeValues;

        new public IReadOnlyCollection<Piece> Pieces => base.Pieces;
        new public IEnumerable<T> PiecesOfType<T>() where T : class, IBehavior
        {
            return base.PiecesOfType<T>();
        }
        public Core Core => _core;
        new public double Energy => base.Energy;
        new public double Mass => base.Mass;

        internal Player(Game game)
            : base(game, 0, 1750)
        {
            this.Research = new(game);
            this.upgradeValues = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && typeof(IUpgradeValues).IsAssignableFrom(t))
                .Select(Activator.CreateInstance)
                .OfType<IUpgradeValues>()
                .ToArray();
        }
        internal void CreateCore()
        {
            this._core = Core.NewCore(Game);
        }

        internal T GetUpgradeValues<T>() where T : IUpgradeValues
        {
            return upgradeValues.OfType<T>().Single();
        }
        internal void OnResearch(Research.Type type, double researchMult)
        {
            foreach (IUpgradeValues values in Game.Rand.Iterate(upgradeValues))
                values.Upgrade(type, researchMult);
            foreach (PlayerPiece piece in Game.Rand.Iterate(Pieces))
                piece.OnResearch(type);
        }

        internal bool Spend(double energy, double mass)
        {
            bool has = Has(energy, mass);
            if (has)
            {
                this._energy -= energy;
                this._mass -= mass;
            }
            return has;
        }
        public bool Has(double energy, double mass)
        {
            return (Energy >= energy && Mass >= mass);
        }

        public void GetIncome(out double energyInc, out double energyUpk, out double massInc, out double massUpk, out double researchInc)
        {
            energyInc = energyUpk = massInc = massUpk = researchInc = 0;
            foreach (PlayerPiece piece in Game.Rand.Iterate(Pieces))
                piece.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc);
        }
        internal override Research.Type? EndTurn()
        {
            GetIncome(out double energyInc, out double _, out double massInc, out double _, out double researchInc);
            this._energy += energyInc;
            this._mass += massInc;

            base.EndTurn();

            return this.Research.AddResearch(researchInc);
        }
    }
}
