using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class PlayerPiece : Piece, IIncome
    {
        private double _vision;
        public double Vision
        {
            get
            {
                return Consts.GetDamagedValue(this, VisionBase, 0);
            }
            protected set
            {
                this._vision = value;
                Game.Map.UpdateVision(this);
            }
        }
        public double VisionBase => _vision;

        internal PlayerPiece(Tile tile, double vision)
            : base(tile.Map.Game.Player, tile)
        {
            this._vision = vision;
        }

        void IIncome.GetIncome(out double energyInc, out double massInc, out double researchInc)
        {
            energyInc = massInc = researchInc = 0;
            GetIncome(ref energyInc, ref massInc, ref researchInc);
        }
        public void GetIncome(ref double energyInc, ref double massInc, ref double researchInc)
        {
            GenerateResources(ref energyInc, ref massInc, ref researchInc);

            double energyUpk = 0, massUpk = 0;
            GetUpkeep(ref energyUpk, ref massUpk);
            energyInc -= energyUpk;
            massInc -= massUpk;
        }
        internal virtual void GenerateResources(ref double energyInc, ref double massInc, ref double researchInc)
        {
        }

        public double GetRepairInc()
        {
            double result = 0;
            if (this.HasBehavior(out Behavior.Combat.IKillable killable))
                killable.GetHitsRepair(out result, out _);
            return result;
        }
        public bool IsRepairing() => HasBehavior(out Behavior.Combat.IKillable killable) && killable.IsRepairing();

        public virtual void Disband()
        {
            Side.AddResources(0, Consts.Income(DisbandMass()));
            Die();
        }
        public double DisbandMass()
        {
            DisbandValue(out double energy, out double mass);
            return energy / Consts.EnergyRepairDiv + mass;
        }
        internal virtual void DisbandValue(out double energy, out double mass)
        {
            double pct = Consts.DisbandValue;
            if (HasBehavior(out IKillable killable))
            {
                double totCur = 0, totMax = 0;
                foreach (var d in killable.Protection)
                {
                    double mult = d.Type == CombatTypes.DefenseType.Hits ? 2 : 1;
                    totCur += Consts.StatValue(d.DefenseCur) * mult;
                    totMax += Consts.StatValue(d.DefenseMax) * mult;
                }
                if (HasBehavior(out IAttacker attacker))
                {
                    double mult = 1 / 3.0;
                    foreach (var a in attacker.Attacks)
                    {
                        totCur += Consts.StatValue(a.AttackCur) * mult;
                        totMax += Consts.StatValue(a.AttackMax) * mult;
                    }
                }
                pct *= totCur / totMax;
            }

            Cost(out int e, out int m);
            energy = e * pct;
            mass = m * pct;
        }
        internal abstract void Cost(out int energy, out int mass);

        internal abstract void OnResearch(Research.Type type);
    }
}
