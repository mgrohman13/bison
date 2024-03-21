using ClassLibrary1.Pieces.Terrain;
using MattUtil;
using System;
using System.Linq;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class Turret : FoundationPiece, IKillable.IRepairable
    {
        const int NUM_ATTACKS = 1;

        private readonly double _hitsMult, _pierceMult, _rounding;
        private readonly double[] _rangeMult = new double[NUM_ATTACKS], _dev = new double[NUM_ATTACKS];

        public Piece Piece => this;

        private Turret(Tile tile, Values values)
            : base(tile, values.Vision)
        {
            this._hitsMult = Game.Rand.GaussianCapped(1, .169, .39);
            this._pierceMult = Game.Rand.GaussianCapped(1, .13, .13);
            this._rounding = Game.Rand.NextDouble();
            for (int a = 0; a < NUM_ATTACKS; a++)
            {
                this._rangeMult[a] = Game.Rand.GaussianOE(values.AttackRange[a], .26, .26, 1) / values.AttackRange[a];
                this._dev[a] = Game.Rand.Weighted(.21);
            }

            IKillable.Values killable = values.GetKillable(Game.Player.Research, _hitsMult, _rounding);
            SetBehavior(
                new Killable(this, killable, killable.Defense),//, killable.ShieldMax / 2.1),
                new Attacker(this, values.GetAttacks(Game.Player.Research, _pierceMult, _rangeMult, _rounding, _dev)));
        }
        internal static Turret NewTurret(Foundation foundation)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

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
            GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _hitsMult, _rounding));
            GetBehavior<IAttacker>().Upgrade(values.GetAttacks(Game.Player.Research, _pierceMult, _rangeMult, _rounding, _dev));
        }
        private static Values GetValues(Game game)
        {
            return game.Player.GetUpgradeValues<Values>();
        }
        internal static double GetRounding(Game game)
        {
            return GetValues(game).Rounding;
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
            private double vision, rounding;
            private IKillable.Values killable;
            private readonly IAttacker.Values[] attacks;
            public Values()
            {
                this.killable = new(-1, -1);
                this.attacks = new IAttacker.Values[] { new(-1) };//, -1, -1, -1, -1), new(-1, -1, -1, -1, -1) };
                UpgradeBuildingCost(1);
                UpgradeBuildingHits(1);
                //UpgradeTurretDefense(1);
                UpgradeTurretAttack(1);
                UpgradeTurretRange(1);
            }

            public int Energy => energy;
            public int Mass => mass;
            public double Vision => vision;
            public double[] AttackRange => attacks.Select(v => v.Range).ToArray();
            public double Rounding => rounding;
            public IKillable.Values GetKillable(Research research, double hitsMult, double rounding)
            {
                int defense = killable.Defense;
                //int hits = MTRandom.Round(killable.HitsMax * hitsMult, rounding);
                //double armor, shieldInc;
                //int shieldMax, shieldLimit;
                //armor = shieldInc = shieldMax = shieldLimit = 0;
                //if (research.HasType(Research.Type.TurretDefense))
                //{
                //    armor = killable.Armor;
                //    double shieldMult = Math.Pow(killable.HitsMax / (double)hits, 1.3);
                //    shieldInc = killable.ShieldInc * shieldMult;
                //    shieldMax = 1 + MTRandom.Round((killable.ShieldMax - 1) * shieldMult, rounding);
                //    shieldLimit = 1 + MTRandom.Round((killable.ShieldLimit - 1) * shieldMult, rounding);
                //}
                return new(defense, killable.Resilience);//, armor, shieldInc, shieldMax, shieldLimit);
            }
            public IAttacker.Values[] GetAttacks(Research research, double _pierceMult, double[] rangeMult, double rounding, double[] dev)
            {
                IAttacker.Values[] result = new IAttacker.Values[NUM_ATTACKS];
                for (int a = 0; a < NUM_ATTACKS; a++)
                {
                    IAttacker.Values attack = attacks[a];
                    int att = attack.Attack;
                    //int damage = MTRandom.Round(attack.Damage / rangeMult[a], rounding);
                    //if (damage < 1)
                    //    damage = 1;
                    //double range = int.MaxValue;
                    //do
                    //{
                    //    if (range < 1 && damage > 1)
                    //        --damage;
                    //    range = attack.Range * Math.Pow(attack.Damage / (double)damage, .78);
                    //} while (range < 1 && damage > 1);
                    //if (range < 1)
                    //    range = 1;
                    //double ap = research.HasType(Research.Type.TurretAttack) ? Consts.GetPct(attack.ArmorPierce, _pierceMult) : 0;
                    //double sp = research.HasType(Research.Type.TurretAttack) ? Consts.GetPct(attack.ShieldPierce, 1 / _pierceMult) : 0;
                    result[a] = new(att);//, ap, sp, dev[a], range);
                }
                return result;
            }

            public void Upgrade(Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(researchMult);
                else if (type == Research.Type.BuildingDefense)
                    UpgradeBuildingHits(researchMult);
                else if (type == Research.Type.TurretDefenses)
                    UpgradeTurretDefense(researchMult);
                else if (type == Research.Type.TurretRange)
                    UpgradeTurretRange(researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(researchMult);
            }
            private void UpgradeBuildingCost(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .4);
                rounding = Game.Rand.NextDouble();
                this.energy = MTRandom.Round(1000 / researchMult, rounding);
                this.mass = MTRandom.Round(1750 / researchMult, rounding);
            }
            private void UpgradeBuildingHits(double researchMult)
            {
                //researchMult = Math.Pow(researchMult, .6);
                int defense = Game.Rand.Round(30 * Math.Pow(researchMult, .5));
                this.vision = 15 * Math.Pow(researchMult, .5);
                this.killable = new(defense, resilience);//, killable.Armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit);
            }
            private void UpgradeTurretDefense(double researchMult)
            {
                //double armor = Consts.GetPct(.2, Math.Pow(researchMult, .5));
                //double max = 39 * Math.Pow(researchMult, .7);
                //double limit = 85 * Math.Pow(researchMult, .8);
                //int shieldMax = Game.Rand.Round(max);
                //int shieldLimit = Game.Rand.Round(limit);
                //double mult = Math.Pow((max * 2 + limit) / (shieldMax * 2 + shieldLimit), 1 / 6.5);
                //double shieldInc = Math.PI * mult * Math.Pow(researchMult, .9);
                this.killable = new(killable.Defense, resilience);//, armor, shieldInc, shieldMax, shieldLimit);
            }
            private void UpgradeTurretRange(double researchMult)
            {
                //researchMult = Math.Pow(researchMult, .8);
                for (int a = 0; a < NUM_ATTACKS; a++)
                {
                    //double range = a == 0 ? 21 : 9.1;
                    //range *= researchMult;
                    IAttacker.Values attack = attacks[a];
                    attacks[a] = new(attack.Attack);//, attack.ArmorPierce, attack.ShieldPierce, -1, range);
                }
            }
            private void UpgradeTurretAttack(double researchMult)
            {
                researchMult = Math.Pow(researchMult, .6);
                for (int a = 0; a < NUM_ATTACKS; a++)
                {
                    double attAvg = 20 * researchMult;
                    const double lowPenalty = 2.5;
                    if (researchMult < lowPenalty)
                        attAvg *= researchMult / lowPenalty;
                    int attack = Game.Rand.Round(attAvg);

                    //double damage = a == 0 ? 7.8 : 13;
                    //double armorPierce = a == 0 ? .13 : 0;
                    //double shieldPierce = a == 0 ? 0 : .13;

                    //damage *= researchMult;
                    //if (armorPierce > 0)
                    //    armorPierce = Consts.GetPct(armorPierce, researchMult);
                    //if (shieldPierce > 0)
                    //    shieldPierce = Consts.GetPct(shieldPierce, researchMult);

                    attacks[a] = new(attack);// Game.Rand.Round(damage), armorPierce, shieldPierce, -1, attacks[a].Range);
                }
            }
        }
    }
}
