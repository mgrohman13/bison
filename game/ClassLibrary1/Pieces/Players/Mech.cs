using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static ClassLibrary1.Pieces.Behavior.Combat.CombatTypes;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Mech : PlayerPiece, IKillable.IRepairable
    {
        //private bool _canCombine;

        public MechBlueprint Blueprint { get; private set; }
        public bool CanCombine
        {
            get
            {
                Values values = GetValues(Game);
                return values.CanCombine &&
                    Game.Player.Research.HasType(Research.Type.CombineMechs) && Blueprint.ResearchLevel < values.FromLevel;
            }
        }

        private Mech(Tile tile, MechBlueprint blueprint)
            : base(tile, blueprint.Vision)
        {
            //this._canCombine = tile.Map.Game.Player.Research.HasType(Research.Type.CombineMechs);
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

                double refund = GetRefundPct(upgradeTo.ResearchLevel);

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
        private double GetRefundPct(int upgradeTo, double mult = 1)
        {
            //refund is based on research level difference, centered around UpgRefundValue when difference=ResearchFactor*mult
            double refund = Math.Sqrt((upgradeTo - Blueprint.ResearchLevel) / Game.Consts.ResearchFactor / mult);
            if (refund > 1)
                refund = Game.Consts.UpgRefundValue / refund;
            else
                refund = 1 - (1 - Game.Consts.UpgRefundValue) * Math.Sqrt(refund);
            return refund;
        }

        public bool Upgrade()
        {
            if (CanUpgrade(out MechBlueprint upgradeTo, out int energy, out int mass) && Game.Player.Spend(energy, mass))
            {
                this.Vision = upgradeTo.Vision;
                GetBehavior<IKillable>().Upgrade(upgradeTo.Killable, upgradeTo.Resilience, true);
                GetBehavior<IAttacker>().Upgrade(upgradeTo.Attacker, true);
                GetBehavior<IMovable>().Upgrade(upgradeTo.Movable, true);
                this.Blueprint = upgradeTo;
                return true;
            }
            return false;
        }
        public bool CanCombineNow() => GetCombinations().Any();
        public IEnumerable<Mech> GetCombinations()
        {
            if (CanCombine)
            {
                var choices = Tile.GetAdjacentTiles().Select(t => t.Piece)
                    .Where(p => p?.Side == Side).OfType<Mech>().Where(m => m.CanCombine && m != this);
                if (choices.Any())
                    return choices;
            }
            return [];
        }
        public bool Combine(Mech other)
        {
            //Mech other = GetCombine();
            if (other != null)
            {
                Values values = GetValues(Game);
                int level = values.GenResearchLevel(Math.Max(Blueprint.ResearchLevel, other.Blueprint.ResearchLevel));
                //level =  Game.Rand.RangeInt(Game.Rand.RangeInt(Blueprint.ResearchLevel, other.Blueprint.ResearchLevel), values.ToLevel);

                double thisCost = this.Blueprint.EnergyEquivalent(Game.Consts);
                double otherCost = other.Blueprint.EnergyEquivalent(Game.Consts);
                double refund = (this.GetRefundPct(level, 2) * thisCost + other.GetRefundPct(level, 2) * otherCost) / (thisCost + otherCost);

                this.Blueprint = MechBlueprint.Combine(Game, this.Blueprint, other.Blueprint, level, refund);
                Game.Player.Research.Combined(this.Blueprint);

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

                killable.Upgrade(Blueprint.Killable, Blueprint.Resilience, true, curDef);
                attacker.Upgrade(Blueprint.Attacker, true, curAtt);
                movable.Upgrade(Blueprint.Movable, true, movable.MoveCur + other.GetBehavior<IMovable>().MoveCur);

                //don't refund resources, since they get incorporated into the new mech
                other.Die(out _, out _);

                //_canCombine = false;
                return true;
            }
            return false;
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();
        internal static int CombineLevel(Game game) => GetValues(game).FromLevel;

        internal override void OnResearch(Research.Type type)
        {
            if (type == Research.Type.CombineMechs)
                Game.Player.Research.Obsolete(GetValues(Game).FromLevel);

            //if (type == Research.Type.CombineMechs)
            //    _canCombine = true;
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
            private static int MinLevel => Game.Rand.GaussianOEInt(Game.Rand.RangeInt(52, 65), .13, .169, 1);

            private int toLevel, fromLevel;
            private double deviation;

            private int ToLevel => toLevel;
            public int FromLevel => fromLevel;
            public bool CanCombine => ToLevel > FromLevel;
            public int GenResearchLevel(int min) => Game.Rand.GaussianCappedInt(ToLevel, (deviation += Game.Rand.OE()) / Math.Sqrt(ToLevel), min + 1);

            public void Init(Game game)
            {
                this.toLevel = 0;
                this.fromLevel = MinLevel + Game.Rand.RangeInt(MinLevel, Game.Rand.Round(game.Consts.CombineResearchBuffer));
                this.deviation = Game.Rand.DoubleFull() + Game.Rand.OE();
            }
            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.CombineMechs)
                    UpgradeCombineMechs(game);
            }

            private void UpgradeCombineMechs(Game game)
            {
                this.toLevel = Math.Max(this.toLevel + 1, game.Player?.Research.GetBlueprintLevel() ?? 0);
                int minUpg = game.Player?.PiecesOfType<Mech>().Select(m => m.Blueprint.ResearchLevel).Order().Skip(1).FirstOrDefault() ?? 0;
                this.fromLevel = Game.Rand.RangeInt(Math.Max(minUpg, this.fromLevel) + 1, Math.Max(MinLevel, Game.Rand.Round(this.toLevel - game.Consts.CombineResearchBuffer)));
                this.deviation = Math.Sqrt(deviation + 1) - 1;
            }
        }
    }
}
