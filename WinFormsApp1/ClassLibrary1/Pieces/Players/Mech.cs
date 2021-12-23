using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Mech : PlayerPiece, IKillable.IRepairable
    {
        public Piece Piece => this;
        public readonly MechBlueprint blueprint;

        private Mech(Map.Tile tile, MechBlueprint blueprint)
            : base(tile, blueprint.Vision)
        {
            this.blueprint = blueprint;
            SetBehavior(new Killable(this, blueprint.Killable), new Attacker(this, blueprint.Attacks), new Movable(this, blueprint.Movable));
        }
        internal static Mech NewMech(Map.Tile tile, MechBlueprint blueprint)
        {
            Mech obj = new(tile, blueprint);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        internal override void OnResearch(Research.Type type)
        {
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                blueprint.Cost(out double energy, out double mass);
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
            energyUpk += Consts.BaseMechUpkeep;
        }

        public override string ToString()
        {
            return "Mech " + PieceNum;
        }
    }
}
