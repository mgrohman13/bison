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
    public class Mech : PlayerPiece, IKillable, IAttacker, IMovable
    {
        private readonly IKillable killable;
        private readonly IAttacker attacker;
        private readonly IMovable movable;

        public Piece Piece => this;

        private Mech(Map.Tile tile, MechBlueprint blueprint)
            : base(tile, blueprint.Vision)
        {
            this.killable = new Killable(this, blueprint.Killable);
            this.attacker = new Attacker(this, blueprint.Attacks);
            this.movable = new Movable(this, blueprint.Movable);
            SetBehavior(this.killable, this.attacker, this.movable);
        }
        internal static Mech NewMech(Map.Tile tile, MechBlueprint blueprint)
        {
            Mech obj = new(tile, blueprint);
            tile.Map.Game.AddPiece(obj);
            //temp
            tile.Map.Game.GenBlueprints();
            return obj;
        }
        public static void Cost(Game game, out double energy, out double mass, MechBlueprint blueprint)
        {
            Cost(out energy, out mass, blueprint, game.Player.GetResearchMult());
        }
        internal static void Cost(out double energy, out double mass, MechBlueprint blueprint, double researchMult)
        {
            var killable = blueprint.Killable;
            var attacks = blueprint.Attacks;
            var movable = blueprint.Movable;

            double hp = killable.HitsMax / (1 - Math.Pow(killable.Armor, 1.13));
            double shield = killable.ShieldInc * 13 + killable.ShieldMax * 1 + killable.ShieldLimit / 2.6;
            shield /= 3.0;

            double dmg = 0, rng = 0, ap = 0, sp = 0, cnt = 0;
            foreach (IAttacker.Values attack in attacks)
            {
                double a = 1 + attack.ArmorPierce;
                double s = 1 + attack.ShieldPierce;
                double r = 2.6 + attack.Range;
                dmg += a * s * attack.Damage;
                double w = attack.Damage;
                rng += r * w;
                ap += a * w;
                sp += s * w;
                cnt += w;
            }
            rng /= cnt;
            ap /= cnt;
            sp /= cnt;
            ap -= 1;
            sp -= 1;

            double vision = (blueprint.Vision + 5.2) / 9.1;
            double move = 26 * movable.MoveInc / 1.0 + 2 * movable.MoveMax / 2.1 + 1 * movable.MoveLimit / 3.9;
            move /= 26 + 2 + 1;

            hp /= Math.Pow(researchMult, .9);
            shield /= Math.Pow(researchMult, .7);
            rng /= Math.Pow(researchMult, .6);
            dmg /= Math.Pow(researchMult, .8);
            vision /= Math.Pow(researchMult, .5);
            move /= Math.Pow(researchMult, .4);
            move += 3.9;
            double total = Math.Sqrt((hp + shield + (rng * 3.9)) * (dmg + vision) * move) * Consts.MechCostMult;

            shield *= 2.6;
            shield = (1 + shield) / (hp + shield);
            ap = 1 + (1 + ap) / (1 + ap + sp);
            rng /= (13 + rng);
            move /= (39 + move);
            double energyPct = Math.Pow(shield / 2.0 * ap / 2.0 * rng * move, 1 / 4.5);

            energy = total * energyPct;
            mass = (total - energy) / Consts.MechMassDiv;
        }

        double IKillable.RepairCost => GetRepairCost();
        public double GetRepairCost()
        {
            MechBlueprint blueprint = new(Vision, new(HitsMax, Consts.MechResilience, Armor, ShieldInc, ShieldMax, ShieldLimit),
                Attacks.Select(a => new IAttacker.Values(a.Damage, a.ArmorPierce, a.ShieldPierce, a.Dev, a.Range)).ToList(),
                new(MoveInc, MoveMax, MoveLimit));
            Cost(out double energy, out double mass, blueprint, Game.Player.GetResearchMult());
            return Consts.GetRepairCost(energy, mass);
        }

        public override void GenerateResources(ref double energyInc, ref double energyUpk, ref double massInc, ref double massUpk, ref double researchInc, ref double researchUpk)
        {
            base.GenerateResources(ref energyInc, ref energyUpk, ref massInc, ref massUpk, ref researchInc, ref researchUpk);
            energyUpk += Consts.BaseMechUpkeep;
        }

        public override string ToString()
        {
            return "Mech " + PieceNum;
        }

        #region IKillable

        public double HitsCur => killable.HitsCur;
        public double HitsMax => killable.HitsMax;
        public double Resilience => killable.Resilience;
        public double Armor => killable.Armor;
        public double ShieldCur => killable.ShieldCur;
        public double ShieldInc => killable.ShieldInc;
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

        #region IMovable

        public double MoveCur => movable.MoveCur;
        public double MoveInc => movable.MoveInc;
        public double MoveMax => movable.MoveMax;
        public double MoveLimit => movable.MoveLimit;

        double IMovable.GetInc()
        {
            return movable.GetInc();
        }

        public bool Move(Map.Tile to)
        {
            return movable.Move(to);
        }
        bool IMovable.EnemyMove(Map.Tile to)
        {
            return false;
        }

        #endregion IMovable
    }
}
