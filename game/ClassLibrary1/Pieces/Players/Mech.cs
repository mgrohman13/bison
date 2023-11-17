﻿using System;
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
        public MechBlueprint Blueprint { get; private set; }

        private Mech(Map.Tile tile, MechBlueprint blueprint)
            : base(tile, blueprint.Vision)
        {
            this.Blueprint = blueprint;
            SetBehavior(new Killable(this, blueprint.Killable), new Attacker(this, blueprint.Attacks), new Movable(this, blueprint.Movable));
        }
        internal static Mech NewMech(Map.Tile tile, MechBlueprint blueprint)
        {
            Mech obj = new(tile, blueprint);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }

        public bool CanUpgrade(out MechBlueprint upgradeTo, out int energy, out int mass)
        {
            upgradeTo = Blueprint.UpgradeTo;
            energy = mass = 0;
            if (upgradeTo != null)
            {
                while (upgradeTo.UpgradeTo != null)
                    upgradeTo = upgradeTo.UpgradeTo;

                energy = upgradeTo.Energy - Blueprint.Energy;
                mass = upgradeTo.Mass - Blueprint.Mass;

                return Game.Player.Has(energy, mass) && Side.PiecesOfType<IBuilder.IBuildMech>().Any(b => Tile.GetDistance(b.Piece.Tile) <= b.Range);
            }
            return false;
        }
        public bool Upgrade()
        {
            if (CanUpgrade(out MechBlueprint upgradeTo, out int energy, out int mass) && Game.Player.Spend(energy, mass))
            {
                this._vision = upgradeTo.Vision;
                GetBehavior<IKillable>().Upgrade(upgradeTo.Killable);
                GetBehavior<IAttacker>().Upgrade(upgradeTo.Attacks);
                GetBehavior<IMovable>().Upgrade(upgradeTo.Movable);
                this.Blueprint = upgradeTo;
                return true;
            }
            return false;
        }

        internal override void OnResearch(Research.Type type)
        {
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                return Consts.GetRepairCost(Blueprint.Energy, Blueprint.Mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => false;

        internal override void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            base.GetUpkeep(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseMechUpkeep;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            energyUpk += Consts.BaseMechUpkeep;
        }

        public string GetName()
        {
            return string.Format("Mech {0}", PieceNum);
        }
        public string GetBlueprintName()
        {
            return string.Format(" ({0})", Blueprint);
        }
        public override string ToString()
        {
            return GetName() + GetBlueprintName();
        }
    }
}