using MattUtil;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Core : PlayerPiece, IKillable.IRepairable, IDeserializationCallback
    {
        private double _income = 1, _incomeTrg = 1, _hitsResearchMult = 1;

        public const double START_VISION = Attack.MIN_RANGED;
        public const int CORE_HITS = 10;

        private readonly IKillable killable;

        private Core(Tile tile, Values values)
            : base(tile, values.Vision)
        {
            killable = new Killable(this, new IKillable.Values(DefenseType.Hits, CORE_HITS), values.Resilience);
            SetBehavior(killable, new Repair(this, values.Repair));
            Unlock(tile.Map.Game.Player.Research);

            OnDeserialization(this);
        }
        internal static Core NewCore(Tile tile)
        {
            Core obj = new(tile, GetValues(tile.Map.Game));
            tile.Map.Game.AddPiece(obj);
            return obj;
        }
        public void OnDeserialization(object sender)
        {
            //base.OnDeserialization(sender);
            if (killable != null)
            {
                ((Killable)killable).OnDeserialization(this);
                killable.Event.DamagedEvent += Killable_DamagedEvent;
            }
        }

        private void Killable_DamagedEvent(object sender, Killable.DamagedEventArgs e)
        {
            if (!killable.Dead)
            {
                int defenseCur = killable.Hits.DefenseCur;
                GetBehavior<IKillable>().SetHits(defenseCur, defenseCur);
            }
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock(Game.Player.Research);
            Values values = GetValues(Game);

            this.Vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(values.GetKillable(killable.Hits.DefenseCur, ref _hitsResearchMult), values.Resilience);
            GetBehavior<IRepair>().Upgrade(values.Repair);
            Builder.UpgradeAll(this, values.Repair.Builder);

            //this._hitsResearchMult = values.HitsResearchMult;
        }

        private void Unlock(Research research)
        {
            Values values = GetValues(Game);
            if (!HasBehavior<IBuilder.IBuildMech>() && research.HasType(Research.Type.Mech))
                SetBehavior(new Builder.BuildMech(this, values.Repair.Builder));
            if (!HasBehavior<IBuilder.IBuildConstructor>() && research.HasType(Research.Type.Constructor))
                SetBehavior(new Builder.BuildConstructor(this, values.Repair.Builder));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        internal override void Die()
        {
            Game.End();
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                return Consts.GetRepairCost(this, GetValues(Game).Energy, GetValues(Game).Mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => false;
        public bool CanRepair() => Consts.CanRepair(this);

        internal override void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
            base.GenerateResources(ref energyInc, ref massInc, ref researchInc);
            double mult = _income;
            double lowMult = Math.Sqrt(mult), highMult = mult * mult;
            energyInc += Consts.CoreEnergyLow * lowMult + Consts.CoreEnergyMid * mult + Consts.CoreEnergyHigh * highMult;
            massInc += Consts.CoreMass * highMult;
            researchInc += Consts.CoreResearch * lowMult;
        }
        internal override void EndTurn(ref double energyUpk, ref double massUpk)
        {
            base.EndTurn(ref energyUpk, ref massUpk);
            const double factor = 1 - 1.0 / Consts.CoreExtractTurns;
            this._incomeTrg *= factor;
            this._income = Game.Rand.GaussianCapped(Math.Sqrt(_incomeTrg * _income), 1 - factor, _incomeTrg / 2.0);
        }

        public override string ToString()
        {
            return "Core";
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = 1;
            private readonly double energy, mass;
            private readonly IRepair.Values repair;

            //private double vision;
            //private IKillable.Values hits;
            private IKillable.Values? shield;
            private double hitsResearchMult = 1;
            private double rounding = Game.Rand.NextDouble();

            public Values()
            {
                this.energy = this.mass = 1300;
                this.repair = new(new(8.5), 1);

                //this.vision = -1;
                //this.hits = new(CombatTypes.DefenseType.Hits, -1);
                this.shield = null;//new(CombatTypes.DefenseType.Hits, -1);

                //UpgradeBuildingHits(1);
                UpgradeCoreShields(1);
            }

            public double HitsResearchMult => hitsResearchMult;

            public double Resilience => resilience;
            public double Energy => energy;
            public double Mass => mass;
            public double Vision => START_VISION;
            //public IKillable.Values Hits => hits;
            public IKillable.Values? Shield => shield;
            public IRepair.Values Repair => repair;

            public IKillable.Values[] GetKillable(int curDef, ref double prevMult)
            {
                int def = curDef;
                double diff = hitsResearchMult - prevMult;
                if (diff > 0)
                {
                    double inc = Consts.StatValue(CORE_HITS);
                    def = MTRandom.Round(Consts.StatValueInverse(Consts.StatValue(curDef) + diff * inc), rounding);
                    if (def < curDef)
                        def = curDef;
                    prevMult += (Consts.StatValue(def) - Consts.StatValue(curDef)) / inc;
                }

                IKillable.Values hits = new(DefenseType.Hits, def);
                List<IKillable.Values> defs = new() { hits };
                if (Shield.HasValue)
                    defs.Add(Shield.Value);
                return defs.ToArray();
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                //if (type == Research.Type.BuildingDefense)
                //    UpgradeBuildingHits(researchMult);
                //else
                if (type == Research.Type.CoreDefense)
                    UpgradeCoreShields(researchMult);
            }
            //private void UpgradeBuildingHits(double researchMult)
            //{
            //    //this.vision = START_VISION * Math.Pow(researchMult, Core_Vision);

            //    double defAvg = ResearchUpgValues.Calc(UpgType.CoreDefense, researchMult);
            //    this.hits = new(DefenseType.Hits, Game.Rand.Round(defAvg));
            //}
            private void UpgradeCoreShields(double researchMult)
            {
                this.hitsResearchMult = researchMult;
                this.rounding = Game.Rand.NextDouble();

                double shieldAvg = ResearchUpgValues.Calc(UpgType.CoreShields, researchMult);
                this.shield = new(DefenseType.Shield, Game.Rand.Round(shieldAvg));
            }
        }
    }
}
