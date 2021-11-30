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
    public class Player : ISide
    {
        private readonly ISide side;

        private Core _core;

        private double _energy, _mass, _research;
        public double Energy => _energy;
        public double Mass => _mass;
        public double Research => _research;

        internal Player(Game game)
        {
            this.side = new Side(game);
            this._energy = 1000;
            this._mass = 1000;
            this._research = 0;
        }
        internal void CreateCore()
        {
            this._core = Core.NewCore(Game);
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
        internal void EndTurn()
        {
            GetIncome(out double energyInc, out double energyUpk, out double massInc, out double massUpk, out double researchInc, out double researchUpk);
            this._energy += energyInc - energyUpk;
            this._mass += massInc - massUpk;
            this._research += researchInc - researchUpk;

            side.EndTurn();
        }

        #region ISide

        public Game Game => side.Game;
        public Core Core => _core;
        IReadOnlyCollection<Piece> ISide.Pieces => Pieces;
        public IReadOnlyCollection<Piece> Pieces => side.Pieces;

        internal double GetResearchMult()
        {
            return (Research + Consts.ResearchFactor) / Consts.ResearchFactor;
        }

        void ISide.AddPiece(Piece piece)
        {
            AddPiece(piece);
        }

        internal void AddPiece(Piece piece)
        {
            side.AddPiece(piece);
        }

        void ISide.RemovePiece(Piece piece)
        {
            RemovePiece(piece);
        }
        internal void RemovePiece(Piece piece)
        {
            side.RemovePiece(piece);
        }

        void ISide.EndTurn()
        {
            EndTurn();
        }

        #endregion ISide
    }
}
