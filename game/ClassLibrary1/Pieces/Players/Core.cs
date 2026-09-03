using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Core : PlayerPiece, IDeserializationCallback, IIncome, IKillable.IRepairable
    {
        public const int CORE_HITS = 10;
        public const double CORE_RANGE = 8.5;

        private double _income = 1, _incomeTrg = 1, _hitsResearchMult = 1;

        bool IKillable.IRepairable.AutoRepair => !Game.GameOver;
        double IKillable.IRepairable.RepairCost => short.MaxValue; //32,767 - should never be used

        private Core(Tile tile)
            : base(tile, 0)
        {
            SetBehavior(
                new Killable(this, new IKillable.Values(DefenseType.Hits, CORE_HITS), 1),
                new Repair(this, new(new(CORE_RANGE), 1)));
            Unlock();

            OnDeserialization(this);
        }
        internal static Core NewCore(Tile tile)
        {
            Core obj = new(tile);
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public void OnDeserialization(object sender)
        {
            //base.OnDeserialization(sender);
            IKillable killable = GetBehavior<IKillable>();
            if (killable != null)
            {
                ((Killable)killable).OnDeserialization(this);
                killable.Event.DamagedEvent += Killable_DamagedEvent;
            }
        }

        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            IKillable killable = GetBehavior<IKillable>();
            if (!killable.Dead)
            {
                int defenseCur = killable.Hits.DefenseCur;
                GetBehavior<IKillable>().SetHits(defenseCur, defenseCur);
            }
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock();
            Upgrade(type);
        }
        private void Upgrade(Research.Type type)
        {
            Values values = Game.Player.GetUpgradeValues<Values>();
            IKillable killable = GetBehavior<IKillable>();
            killable.Upgrade(values.GetKillable(type, Game.Player.Research, killable.Hits.DefenseCur, ref _hitsResearchMult), 1);
        }
        private void Unlock()
        {
            Research research = Game.Player.Research;

            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this, new()));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.Constructor))
                SetBehavior(new Builder.BuildConstructor(this, new()));
            Builder.UpgradeAll(this, new(GetBehavior<IRepair>().RangeBase));
        }

        internal override void Die(out Tile tile, out double treasure)
        {
            tile = this.Tile;
            treasure = 0;
            Game.End();
        }

        internal override void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
            base.GenerateResources(ref energyInc, ref massInc, ref researchInc);
            double mult = _income;
            double lowMult = Math.Sqrt(mult), highMult = mult * mult;
            energyInc += Game.Consts.CoreEnergyLow * lowMult + Game.Consts.CoreEnergyMid * mult + Game.Consts.CoreEnergyHigh * highMult;
            massInc += Game.Consts.CoreMass * highMult;
            researchInc += Game.Consts.CoreResearch * lowMult;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            double factor = 1 - 1.0 / Game.Consts.CoreExtractTurns;
            this._incomeTrg *= factor;
            this._income = Game.Rand.GaussianCapped(Math.Sqrt(_incomeTrg * _income), 1 - factor, _incomeTrg / 2.0);
        }

        public override void Disband() { }
        internal override void DisbandValue(out double energy, out double mass) =>
            energy = mass = 0;
        internal override void Cost(out int energy, out int mass) =>
            throw new Exception();

        public override string ToString() => "Core";
        public bool CanRepair() => Consts.CanRepair(this);

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values : IUpgradeValues
        {
            private double hitsResearchMult = 1;
            private int shields, armor;

            public IKillable.Values[] GetKillable(Research.Type type, Research research, int curDef, ref double prevMult)
            {
                if (type == Research.Type.CoreDefense)
                {
                    double diff = this.hitsResearchMult - prevMult;
                    if (diff > 0)
                    {
                        double inc = Consts.StatValue(CORE_HITS);
                        int def = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(curDef) + diff * inc));
                        if (def < curDef)
                            def = curDef;

                        prevMult += (Consts.StatValue(def) - Consts.StatValue(curDef)) / inc;
                        curDef = def;
                    }
                }

                IKillable.Values hits = new(DefenseType.Hits, curDef);
                List<IKillable.Values> defs = [hits];
                if (research.HasType(Research.Type.CoreDefense))
                    defs.Add(new(DefenseType.Shield, this.shields));
                if (research.HasType(Research.Type.CoreArmor))
                    defs.Add(new(DefenseType.Armor, this.armor));
                return [.. defs];
            }

            public void Init(Game game)
            {
                UpgradeCoreDefense(game, 1);
            }
            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.CoreDefense)
                    UpgradeCoreDefense(game, researchMult);
            }
            private void UpgradeCoreDefense(Game game, double researchMult)
            {
                this.hitsResearchMult = researchMult;

                double shieldAvg = game.ResearchUpgValues.Calc(UpgType.CoreShields, researchMult);
                this.shields = Game.Rand.Round(shieldAvg);

                double armorAvg = game.ResearchUpgValues.Calc(UpgType.CoreArmor, researchMult);
                armorAvg = Consts.StatValue(armorAvg) + Consts.StatValue(shieldAvg) - Consts.StatValue(shields);
                if (armorAvg > 0)
                    armorAvg = Consts.StatValueInverse(armorAvg);
                armorAvg = Math.Max(armorAvg, 1);
                this.armor = Game.Rand.Round(armorAvg);
            }
        }
    }
}
