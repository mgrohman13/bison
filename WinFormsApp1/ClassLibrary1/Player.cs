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
        private readonly List<IUpgradeValues> upgradeValues;

        public IReadOnlyCollection<Piece> Pieces => _pieces;
        public Core Core => _core;
        new public double Energy => base.Energy;
        new public double Mass => base.Mass;

        internal Player(Game game)
            : base(game, 0, 1750)
        {
            this.Research = new(game);
            this.upgradeValues = new List<IUpgradeValues>();
        }
        internal void CreateCore()
        {
            this._core = Core.NewCore(Game);
        }

        internal T GetUpgradeValues<T>(Func<T> Create) where T : IUpgradeValues
        {
            T values = upgradeValues.OfType<T>().SingleOrDefault();
            if (values == null)
                SetUpgradeValues(values = Create());
            return values;
        }
        internal void SetUpgradeValues(IUpgradeValues value)
        {
            upgradeValues.RemoveAll(v => v.GetType() == value.GetType());
            upgradeValues.Add(value);
        }
        internal void OnResearch(Research.Type type, double researchMult)
        {
            foreach (PlayerPiece piece in Pieces.OfType<PlayerPiece>())
                piece.OnResearch(type, researchMult);
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

        public void GetIncome(out double energyInc, out double energyUpk, out double massInc, out double massUpk, out double researchInc, out double researchUpk)
        {
            energyInc = energyUpk = massInc = massUpk = researchInc = researchUpk = 0;
            foreach (PlayerPiece piece in Pieces)
                piece.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
        }
        internal override void EndTurn()
        {
            GetIncome(out double energyInc, out double energyUpk, out double massInc, out double massUpk, out double researchInc, out double researchUpk);
            this._energy += energyInc - energyUpk;
            this._mass += massInc - massUpk;
            this.Research.AddResearch(researchInc - researchUpk);

            base.EndTurn();
        }
    }
}
