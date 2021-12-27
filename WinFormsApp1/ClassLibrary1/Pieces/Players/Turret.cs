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
    public class Turret : PlayerPiece, IKillable.IRepairable
    {
        private readonly double _hitsMult, _rounding;
        private readonly double[] _rangeMult = new double[2], _dev = new double[2];

        public Piece Piece => this;

        private Turret(Map.Tile tile, Values values)
            : base(tile, values.Vision)
        {
            this._hitsMult = Game.Rand.GaussianCapped(1, .169, .39);
            this._rounding = Game.Rand.NextDouble();
            for (int a = 0; a < 2; a++)
            {
                this._rangeMult[a] = Game.Rand.GaussianOE(values.AttackRange[a], .26, .26, 1) / values.AttackRange[a];
                this._dev[a] = Game.Rand.Weighted(.13);
            }

            IKillable.Values killable = values.GetKillable(_hitsMult, _rounding);
            SetBehavior(
                new Killable(this, killable, killable.HitsMax, killable.ShieldMax / 2.1),
                new Attacker(this, values.GetAttacks(Game.Player.Research, _rangeMult, _rounding, _dev)));
        }
        internal static Turret NewTurret(Foundation foundation)
        {
            Map.Tile tile = foundation.Tile;
            foundation.Game.RemovePiece(foundation);

            Turret obj = new(tile, GetValues(foundation.Game));
            foundation.Game.AddPiece(obj);
            return obj;
        }
        public static void Cost(Game game, out int energy, out int mass)
        {
            Values values = GetValues(game);
            energy = values.Energy;
            mass = values.Mass;
        }

        internal override void OnResearch(Research.Type type)
        {
            Values values = GetValues(Game);

            this._vision = values.Vision;
            GetBehavior<IKillable>().Upgrade(values.GetKillable(_hitsMult, _rounding));
            GetBehavior<IAttacker>().Upgrade(values.GetAttacks(Game.Player.Research, _rangeMult, _rounding, _dev));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }

        internal override void Die()
        {
            Map.Tile tile = this.Tile;
            base.Die();
            Foundation.NewFoundation(tile);
        }

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(Game, out int energy, out int mass);
                return Consts.GetRepairCost(energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.TurretAutoRepair);

        public override string ToString()
        {
            return "Turret " + PieceNum;
        }

        [Serializable]
        private class Values : IUpgradeValues
        {
            private const double resilience = .65;

            private int energy, mass;
            private double vision;
            private IKillable.Values killable;
            private readonly IAttacker.Values[] attacks;
            public Values()
            {
                this.killable = new(-1, -1);
                this.attacks = new IAttacker.Values[] { new(-1, -1, -1, -1, -1), new(-1, -1, -1, -1, -1) };
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                UpgradeTurretDefense(1);
                UpgradeTurretAttack(1);
                UpgradeTurretRange(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double[] AttackRange => attacks.Select(v => v.Range).ToArray();
            public IKillable.Values GetKillable(double hitsMult, double rounding)
            {
                int hits = MTRandom.Round(killable.HitsMax * hitsMult, rounding);
                double shieldMult = Math.Pow(killable.HitsMax / (double)hits, 1.04);
                double shieldInc = killable.ShieldInc * shieldMult;
                int shieldMax = 1 + MTRandom.Round((killable.ShieldMax - 1) * shieldMult, rounding);
                int shieldLimit = 1 + MTRandom.Round((killable.ShieldLimit - 1) * shieldMult, rounding);
                return new(hits, killable.Resilience, killable.Armor, shieldInc, shieldMax, shieldLimit);
            }
            public IAttacker.Values[] GetAttacks(Research research, double[] rangeMult, double rounding, double[] dev)
            {
                IAttacker.Values[] result = new IAttacker.Values[2];
                for (int a = 0; a < 2; a++)
                {
                    IAttacker.Values attack = attacks[a];
                    int damage = MTRandom.Round(attack.Damage / rangeMult[a], rounding);
                    if (damage < 1)
                        damage = 1;
                    double range = int.MaxValue;
                    do
                    {
                        if (range < 1 && damage > 1)
                            --damage;
                        range = attack.Range * Math.Pow(attack.Damage / (double)damage, .78);
                    } while (range < 1 && damage > 1);
                    if (range < 1)
                        range = 1;
                    double ap = research.HasType(Research.Type.TurretAttack) ? attack.ArmorPierce : 0;
                    double sp = research.HasType(Research.Type.TurretAttack) ? attack.ShieldPierce : 0;
                    result[a] = new(damage, ap, sp, dev[a], range);
                }
                return result;
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingHits)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.TurretDefense)
                    UpgradeTurretDefense(researchMult);
                else if (type == Research.Type.TurretRange)
                    UpgradeTurretRange(researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .4);
                this.energy = Game.Rand.Round(400 / researchMult);
                this.mass = Game.Rand.Round(800 / researchMult);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .6);
                int hits = Game.Rand.Round(78 * researchMult);
                this.vision = 7 * researchMult;
                this.killable = new(hits, resilience, killable.Armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit);
            }
            private void UpgradeTurretDefense(double researchMult)
            {
                double armor = Consts.GetPct(.2, Math.Pow(researchMult, .5));
                double shieldInc = Math.PI * Math.Pow(researchMult, .9);
                int shieldMax = Game.Rand.Round(39 * Math.Pow(researchMult, .7));
                int shieldLimit = Game.Rand.Round(85 * Math.Pow(researchMult, .8));
                this.killable = new(killable.HitsMax, resilience, armor, shieldInc, shieldMax, shieldLimit);
            }
            private void UpgradeTurretRange(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .8);
                for (int a = 0; a < 2; a++)
                {
                    double range = a == 0 ? 21 : 9.1;
                    range *= researchMult;
                    IAttacker.Values attack = attacks[a];
                    attacks[a] = new(attack.Damage, attack.ArmorPierce, attack.ShieldPierce, -1, range);
                }
            }
            private void UpgradeTurretAttack(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .9);
                for (int a = 0; a < 2; a++)
                {
                    double damage = a == 0 ? 7.8 : 13;
                    double armorPierce = a == 0 ? .13 : 0;
                    double shieldPierce = a == 0 ? 0 : .13;

                    damage *= researchMult;
                    if (armorPierce > 0)
                        armorPierce = Consts.GetPct(armorPierce, researchMult);
                    if (shieldPierce > 0)
                        shieldPierce = Consts.GetPct(shieldPierce, researchMult);

                    attacks[a] = new(Game.Rand.Round(damage), armorPierce, shieldPierce, -1, attacks[a].Range);
                }
            }
        }
    }
}
