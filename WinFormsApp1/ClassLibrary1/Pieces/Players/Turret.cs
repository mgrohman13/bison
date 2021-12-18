using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Terrain;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Turret : PlayerPiece, IKillable, IAttacker
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;

        public Piece Piece => this;

        private Turret(Map.Tile tile, double vision, IKillable.Values killable, IEnumerable<IAttacker.Values> attacks)
            : base(tile, vision)
        {
            this.killable = new Killable(this, killable, killable.ShieldMax / 2.1);
            this.attacker = new Attacker(this, attacks);
            SetBehavior(this.killable, this.attacker);
        }
        internal static Turret NewTurret(Foundation foundation)
        {
            Map.Tile tile = foundation.Tile;
            foundation.Game.RemovePiece(foundation);

            double researchMult = Math.Pow(foundation.Game.Player.GetResearchMult(), .9);
            double avgHits = 26;
            double hits = Game.Rand.GaussianCapped(avgHits, .169, 10);
            double shieldMult = researchMult * avgHits / hits;
            hits *= researchMult;
            double armor = 1 - Math.Pow(.6, researchMult);
            double vision = 7 * researchMult;
            double shieldInc = 2.6 * shieldMult;
            double shieldMax = 26 * shieldMult;
            double shieldLimit = 260 * shieldMult;

            List<IAttacker.Values> attacks = new(2);
            for (int a = 0; a < 2; a++)
            {
                double avgDmg = a == 0 ? Math.PI : 5.2;
                double damage = Game.Rand.GaussianOE(avgDmg, .26, .26);
                double armorPierce = a == 0 ? .2 : 0;
                double shieldPierce = a == 0 ? 0 : .2;
                double dev = Game.Rand.Weighted(.13);
                double range = a == 0 ? 21 : 9.1;
                range *= avgDmg / damage;

                damage *= researchMult;
                if (armorPierce > 0)
                    armorPierce = 1 - Math.Pow(1 - armorPierce, researchMult);
                if (shieldPierce > 0)
                    shieldPierce = 1 - Math.Pow(1 - shieldPierce, researchMult);
                range *= researchMult;

                attacks.Add(new(damage, armorPierce, shieldPierce, dev, range));
            }

            Turret obj = new(tile, vision, new(hits, Consts.MechResilience, armor, shieldInc, shieldMax, shieldLimit), attacks);
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass)
        {
            double researchMult = Math.Pow(game.Player.GetResearchMult(), .2);
            energy = 250 / researchMult;
            mass = 250 / researchMult;
        }

        internal override void Die()
        {
            Map.Tile tile = this.Tile;
            base.Die();
            Foundation.NewFoundation(tile);
        }

        double IKillable.RepairCost => GetRepairCost();
        public double GetRepairCost()
        {
            Cost(Game, out double energy, out double mass);
            return Consts.GetRepairCost(energy, mass);
        }

        public override string ToString()
        {
            return "Turret " + PieceNum;
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Resilience => killable.Resilience;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
        public double ShieldIncBase => killable.ShieldIncBase;
        public double ShieldMax => killable.ShieldMax;
        public double ShieldLimit => killable.ShieldLimit;
        public bool Dead => killable.Dead;

        double IKillable.GetInc()
        {
            return killable.GetInc();
        }

        void IKillable.Repair(double hits)
        {
            killable.Repair(hits);
        }
        void IKillable.Damage(double damage, double shieldDmg)
        {
            killable.Damage(damage, shieldDmg);
        }

        #endregion IKillable

        #region IAttacker

        public IReadOnlyCollection<Attacker.Attack> Attacks => attacker.Attacks;
        public bool Fire(IKillable killable)
        {
            return attacker.Fire(killable);
        }
        bool IAttacker.EnemyFire(IKillable killable)
        {
            return false;
        }

        #endregion IAttacker

    }
}
