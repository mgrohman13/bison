using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static ClassLibrary1.Pieces.Behavior.Combat.CombatTypes;
using static ClassLibrary1.ResearchUpgValues;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Mech : PlayerPiece, IKillable.IRepairable
    {
        private bool _canCombine;

        public MechBlueprint Blueprint { get; private set; }
        public bool CanCombine => _canCombine;

        private Mech(Tile tile, MechBlueprint blueprint)
            : base(tile, blueprint.Vision)
        {
            this._canCombine = tile.Map.Game.Player.Research.HasType(Research.Type.CombineMechs);
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

                //refund is based on research level difference, centered around UpgRefundValue when difference=ResearchFactor
                double refund = Math.Sqrt((upgradeTo.ResearchLevel - Blueprint.ResearchLevel) / Game.Consts.ResearchFactor);
                if (refund > 1)
                    refund = Game.Consts.UpgRefundValue / refund;
                else
                    refund = 1 - (1 - Game.Consts.UpgRefundValue) * Math.Sqrt(refund);

                //use unrelated double-precision values for rounding entropy
                MTRandom rounding = new(MTRandom.GenerateSeed([Blueprint.Resilience, upgradeTo.Resilience, Blueprint.Vision, upgradeTo.Vision,]));
                energy = rounding.Round(upgradeTo.Energy - Blueprint.Energy * refund);
                mass = rounding.Round(upgradeTo.Mass - Blueprint.Mass * refund);

                Defense hits = GetBehavior<IKillable>().Hits;
                double hp = hits.DefenseCur / (double)hits.DefenseMax * upgradeTo.Hits.Defense;
                //check blocks
                return hp >= 1 && Game.Player.Has(energy, mass)
                    && Side.PiecesOfType<IBuilder.IBuildMech>().Any(b => Tile.GetDistance(b.Piece.Tile) <= b.Range);
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
        public bool CanCombineNow() => GetCombine() != null;
        private Mech GetCombine()
        {
            if (CanCombine)
            {
                var choices = Tile.GetAdjacentTiles().Select(t => t.Piece)
                    .Where(p => p?.Side == Side).OfType<Mech>().Where(m => m.CanCombine && m != this);
                if (choices.Any())
                    return Game.Rand.SelectValue(choices);
            }
            return null;
        }
        public bool Combine()
        {
            Mech other = GetCombine();
            if (other != null)
            {
                Values values = Game.Player.GetUpgradeValues<Values>();
                int level = Game.Rand.RangeInt(Game.Rand.RangeInt(Blueprint.ResearchLevel, other.Blueprint.ResearchLevel), values.ResearchLevel);
                this.Blueprint = MechBlueprint.Combine(Game, this.Blueprint, other.Blueprint, level, values.Discount);

                IKillable killable = GetBehavior<IKillable>();
                IAttacker attacker = GetBehavior<IAttacker>();
                IMovable movable = GetBehavior<IMovable>();

                List<int> curDef = new(Blueprint.Killable.Count);
                IKillable killable2 = other.GetBehavior<IKillable>();
                var allDefs = killable.Protection.Concat(killable2.Protection);
                for (int a = 0; a < Blueprint.Killable.Count; a++)
                {
                    var k = Blueprint.Killable[a];
                    DefenseType type = k.Type;
                    double avg;
                    if (type == DefenseType.Hits)
                        avg = Consts.StatValue(k.Defense) * ((Consts.StatValue(killable.Hits.DefenseCur) + Consts.StatValue(killable2.Hits.DefenseCur))
                            / (Consts.StatValue(killable.Hits.DefenseMax) + Consts.StatValue(killable2.Hits.DefenseMax)));
                    else
                        avg = allDefs.Where(a => a.Type == type).Sum(a => (double?)Consts.StatValue(a.DefenseCur)) ?? 0;
                    curDef.Add(Game.Rand.Round(Consts.StatValueInverse(avg)));
                }

                List<int> curAtt = new(Blueprint.Attacker.Count);
                var allAtts = attacker.Attacks.Concat(other.GetBehavior<IAttacker>().Attacks);
                for (int b = 0; b < Blueprint.Attacker.Count; b++)
                {
                    var a = Blueprint.Attacker[b];
                    AttackType type = a.Type;
                    bool ranged = a.Range > Attack.MELEE_RANGE;
                    double avg = allAtts.Where(a => a.Type == type && ranged == a.Range > Attack.MELEE_RANGE)
                        .Sum(a => (double?)Consts.StatValue(a.AttackCur)) ?? 0;
                    curAtt.Add(Game.Rand.Round(Consts.StatValueInverse(avg)));
                }

                killable.Upgrade(Blueprint.Killable, Blueprint.Resilience, curDef);
                attacker.Upgrade(Blueprint.Attacker, curAtt);
                movable.Upgrade(Blueprint.Movable, movable.MoveCur + other.GetBehavior<IMovable>().MoveCur);

                other.Die();

                _canCombine = false;
                return true;
            }
            return false;
        }

        internal override void OnResearch(Research.Type type)
        {
            if (type == Research.Type.CombineMechs)
                _canCombine = true;
        }

        internal override void Cost(out int energy, out int mass)
        {
            energy = Blueprint.Energy;
            mass = Blueprint.Mass;
        }

        double IKillable.IRepairable.RepairCost => Consts.GetRepairCost(this, Blueprint.Energy, Blueprint.Mass);
        bool IKillable.IRepairable.AutoRepair => false;
        public bool CanRepair() => Consts.CanRepair(this);

        internal override void GetUpkeep(ref double energyUpk, ref double massUpk)
        {
            base.GetUpkeep(ref energyUpk, ref massUpk);
            energyUpk += Game.Consts.BaseMechUpkeep;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            energyUpk += Game.Consts.BaseMechUpkeep;
        }

        public string Name => $"Mech {PieceNum}";
        public string BlueprintName => Blueprint.ToString();
        public string BlueprintNum => Blueprint.BlueprintNum;
        public override string ToString() => $"{Name} ({BlueprintName})";

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            private int researchLevel;
            private double discount;

            public int ResearchLevel => researchLevel;
            public double Discount => discount;

            public void Init(Game game)
            {
                UpgradeCombineMechs(game, 1);
            }
            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.CombineMechs)
                    UpgradeCombineMechs(game, researchMult);
            }
            private void UpgradeCombineMechs(Game game, double researchMult)
            {
                this.researchLevel = game.Player?.Research.GetTotalLevel() ?? 0;
                this.discount = game.ResearchUpgValues.Calc(UpgType.CombineMechs, researchMult);
            }
        }
    }
}
