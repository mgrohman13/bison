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
            rng *= 3.9;
            move += 3.9;
            double total = Math.Sqrt((hp + shield + rng) * (dmg + vision) * move) * Consts.MechCostMult;

            shield *= 3.9;
            shield = (1 + shield) / (1 + hp + shield);
            ap = (((1 + ap) / (2 + 1.5 * ap + sp) - 1 / 3.0) * 21 / 5.0 + .052) / 1.052;
            rng /= (39 + rng);
            move /= (13 + move);
            double energyPct = Math.Pow(shield * ap * rng * move, 1 / 4.5);

            energy = total * energyPct;
            mass = (total - energy) / Consts.MechMassDiv;
        }

        double IKillable.IRepairable.RepairCost => GetRepairCost();
        public double GetRepairCost()
        {
            IKillable killable = GetBehavior<IKillable>();
            IAttacker attacker = GetBehavior<IAttacker>();
            IMovable movable = GetBehavior<IMovable>();
            MechBlueprint blueprint = new(Vision, new(killable.HitsMax, Consts.MechResilience, killable.Armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit),
                attacker.Attacks.Select(a => new IAttacker.Values(a.Damage, a.ArmorPierce, a.ShieldPierce, a.Dev, a.Range)).ToList(),
                new(movable.MoveInc, movable.MoveMax, movable.MoveLimit));
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
    }
}
