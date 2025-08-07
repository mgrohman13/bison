using System;
using System.Linq;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Mech : PlayerPiece, IKillable.IRepairable
    {
        public MechBlueprint Blueprint { get; private set; }

        private Mech(Tile tile, MechBlueprint blueprint)
            : base(tile, blueprint.Vision)
        {
            this.Blueprint = blueprint;
            SetBehavior(new Killable(this, blueprint.Killable, blueprint.Resilience), new Attacker(this, blueprint.Attacker), new Movable(this, blueprint.Movable));
        }
        internal static Mech NewMech(Tile tile, MechBlueprint blueprint)
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

                Defense hits = GetBehavior<IKillable>().Hits;
                double hp = hits.DefenseCur / (double)hits.DefenseMax * upgradeTo.Hits.Defense;
                //check blocks
                if (hp >= 1)
                    return Game.Player.Has(energy, mass) && Side.PiecesOfType<IBuilder.IBuildMech>().Any(b => Tile.GetDistance(b.Piece.Tile) <= b.Range);
            }
            return false;
        }
        public bool Upgrade()
        {
            if (CanUpgrade(out MechBlueprint upgradeTo, out int energy, out int mass) && Game.Player.Spend(energy, mass))
            {
                this.Vision = upgradeTo.Vision;
                GetBehavior<IKillable>().Upgrade(upgradeTo.Killable, upgradeTo.Resilience);
                GetBehavior<IAttacker>().Upgrade(upgradeTo.Attacker);
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
                return Consts.GetRepairCost(this, Blueprint.Energy, Blueprint.Mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => false;
        public bool CanRepair() => Consts.CanRepair(this);

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

        public string Name => $"Mech {PieceNum}";
        public string BlueprintName => Blueprint.ToString();
        public string BlueprintNum => Blueprint.BlueprintNum;
        public override string ToString() => $"{Name} ({BlueprintName})";
    }
}
