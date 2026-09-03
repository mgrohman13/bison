using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using ClassLibrary1.Pieces.Terrain;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tile = ClassLibrary1.Map.Map.Tile;
using UpgType = ClassLibrary1.ResearchUpgValues.UpgType;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class Turret : FoundationPiece, IKillable.IRepairable
    {
        //public const int MAX_ATTACKS = 3;
        //public const int MAX_DEFENSES = 3;
        //public static double Resilience => Values.Resilience;

        //private readonly double _shieldMult, _armorMult, _rounding;
        //private readonly double[] _attMult = new double[MAX_ATTACKS];

        internal readonly Blueprint Version;
        //{
        //    get;
        //    private set;
        //}

        private Turret(Tile tile, bool laser)
            : base(tile, 0)
        {
            //this._shieldMult = Game.Rand.GaussianCapped(1, .13, .5);
            //this._armorMult = Game.Rand.GaussianCapped(1 / _shieldMult, .13, .5 / _shieldMult);
            //this._rounding = Game.Rand.NextDouble();
            //double attMults = 1;
            //for (int a = 0; a < MAX_ATTACKS; a++)
            //{
            //    double devMult = (MAX_ATTACKS - a) / (double)MAX_ATTACKS;
            //    double mult = 1 / Game.Rand.GaussianCapped(attMults, .13 * devMult, attMults * (1 - .5 * devMult));
            //    this._attMult[a] = mult;
            //    attMults *= mult;
            //}

            Values values = GetValues(Game);
            this.Version = laser ? values.Laser : values.Turret;
            this.Vision = Version.Vision;

            SetBehavior(
                new Killable(this, Version.Killable, Version.Resilience),
                new Attacker(this, Version.Attacker));
            Unlock();
        }
        internal static Turret NewTurret(Foundation foundation, bool laser)
        {
            Tile tile = foundation.Tile;
            foundation.Die();

            Turret obj = new(tile, laser);
            foundation.Game.AddPiece(obj);
            return obj;
        }
        //public static void Cost(Game game, out int energy, out int mass)
        //{
        //    Values values = GetValues(game);
        //    energy = values.Energy;
        //    mass = values.Mass;
        //}
        //internal override void Cost(out int energy, out int mass) =>
        //    Cost(Game, out energy, out mass);
        internal override void Cost(out int energy, out int mass)
        {
            energy = Version.Energy;
            mass = Version.Mass;
        }

        //TODO:
        public bool CanUpgrade(Game game)
        {
            Values values = GetValues(game);
            Blueprint bp = Version.Laser ? values.Laser : values.Turret;
            return Version.Version < bp.Version;
        }

        public static List<Blueprint> GetBlueprints(Game game)
        {
            List<Blueprint> blueprints = [];
            Values values = GetValues(game);
            if (game.Player.Research.HasType(Research.Type.Turret))
                blueprints.Add(values.Turret);
            if (game.Player.Research.HasType(Research.Type.LaserTurret))
                blueprints.Add(values.Laser);
            return blueprints;
        }

        protected override bool CanReplace<T>(out Tuple<double, double> rounding)
        {
            rounding = new(GetValues(Game).Rounding, 0);
            return (typeof(T) == typeof(Turret) || typeof(T) == typeof(Generator));
        }

        internal override void OnResearch(Research.Type type)
        {
            Unlock();
        }
        //private void Upgrade()
        //{
        //    Values values = GetValues(Game);
        //    GetBehavior<IKillable>().Upgrade(values.GetKillable(Game.Player.Research, _shieldMult, _armorMult, _rounding), Values.Resilience);
        //    GetBehavior<IAttacker>().Upgrade(values.GetAttacks(Game.Player.Research, _attMult, _rounding));           
        //    this.Vision = values.Vision;
        //}
        private void Unlock()
        {
            Research research = Game.Player.Research;
            if (!HasBehavior<IBuilder.IBuildGenerator>() && research.HasType(Research.Type.AmbientGenerator))
                SetBehavior(new Builder.BuildGenerator(this, new()));
            Builder.UpgradeAll(this, new(.5));
        }
        private static Values GetValues(Game game) => game.Player.GetUpgradeValues<Values>();

        double IKillable.IRepairable.RepairCost
        {
            get
            {
                Cost(out int energy, out int mass);
                return Consts.GetRepairCost(this, energy, mass);
            }
        }
        bool IKillable.IRepairable.AutoRepair => Game.Player.Research.HasType(Research.Type.TurretAutoRepair);
        public bool CanRepair() => Consts.CanRepair(this);

        public override string ToString()
        {
            return "Turret " + PieceNum;
        }

        internal const double LASER_RANGE_MULT = 1.5;

        [Serializable]
        [DataContract(IsReference = true)]
        private class Values() : IUpgradeValues
        {
            private readonly double attLaser = Rand(.65), rangeLaser = Rand(LASER_RANGE_MULT, min: 1),
                defLaser = Rand(.78), protLaser = Rand(1.3);
            private double energy, mass, energyLaser, massLaser;
            private double costMult, att, def, protection, range, resilience, vision;

            public double Rounding
            {
                get;
                private set;
            }
            public Blueprint Turret
            {
                get;
                private set;
            }
            public Blueprint Laser
            {
                get;
                private set;
            }

            public void Init(Game game)
            {
                Consts consts = game.Consts;
                //TODO: increase on TurretShields
                SetCost(consts, .52, .91, out this.energy, out this.mass);
                SetCost(consts, 1, consts.EnergyMassRatio, out this.energyLaser, out this.massLaser);
            }
            private void SetCost(Consts consts, double r1, double r2, out double e, out double m)
            {
                double r = Game.Rand.Range(r1, r2);
                double c = consts.EnergyMassRatio;
                double v = consts.TurretCost * costMult;
                //equivalencies:
                //e + m * c = v  
                //e = m * r
                e = v / (1 + c / r);
                e = Game.Rand.GaussianCapped(e, Dev(e, .13), Math.Max(2 * e - v, 0));
                m = (v - e) / c;
                m = Rand(m, Dev(m, .026), 0);
                static double Dev(double ratio, double dev) => Math.Min(dev, dev / ratio);
            }

            private void Generate(Game game, bool laser)
            {
                Research research = game.Player.Research;
                Blueprint Gen()
                {
                    double energyCost = laser ? this.energyLaser : this.energy;
                    double massCost = laser ? this.massLaser : this.mass;
                    byte version = (laser ? Laser : Turret)?.Version ?? 0;
                    double resilience = Game.Rand.GaussianCapped(this.resilience, .13 / this.resilience, Math.Max(2 * this.resilience - 1, 0));
                    return new(game, energyCost, massCost, laser, ++version, research.GetTotalLevel(), vision, resilience,
                    GetKillable(research, laser), GetAttacks(game, laser));
                }

                if (!laser)
                    this.Turret = Gen();
                else if (research.HasType(Research.Type.LaserTurret))
                    this.Laser = Gen();

                this.Rounding = Game.Rand.NextDouble();
            }
            public List<IKillable.Values> GetKillable(Research research, bool laser)
            {
                List<IKillable.Values> results = [];

                double d = this.def * (laser ? this.defLaser : 1);
                int def = RandInt(d);
                results.Add(new(CombatTypes.DefenseType.Hits, def));

                if (laser || research.HasType(Research.Type.TurretShields))
                {
                    CombatTypes.DefenseType type = laser ? CombatTypes.DefenseType.Armor : CombatTypes.DefenseType.Shield;
                    d = this.protection * (laser ? this.protLaser : 1);
                    def = RandInt(d);
                    results.Add(new(type, def));
                }

                return results;
            }
            public List<IAttacker.Values> GetAttacks(Game game, bool laser)
            {
                double a = this.att * (laser ? this.attLaser : 1);
                int att = RandInt(a);
                CombatTypes.AttackType type = laser ? CombatTypes.AttackType.Energy : CombatTypes.AttackType.Kinetic;
                double range = this.range * (laser ? this.rangeLaser : 1);
                range = Rand(range, .104, Attack.MIN_RANGED);
                return [new(game.CombatTypes, type, att, range)];
            }
            private static double Rand(double avg, double dev = .169, double min = .5) =>
                Game.Rand.GaussianOE(avg, dev, dev / Math.E, min);
            private static int RandInt(double a) =>
                Game.Rand.GaussianOEInt(a, .13, .091, 1);

            public void Upgrade(Game game, Research.Type type, double researchMult)
            {
                if (type == Research.Type.BuildingCost)
                    UpgradeBuildingCost(game, researchMult);
                else if (type == Research.Type.TurretDefense)
                    UpgradeTurretDefense(game, researchMult);
                else if (type == Research.Type.TurretRange)
                    UpgradeTurretRange(game, researchMult);
                else if (type == Research.Type.TurretAttack)
                    UpgradeTurretAttack(game, researchMult);
                else if (type == Research.Type.TurretShields)
                    UpgradeTurretShields(game);//, researchMult);

                switch (type)
                {
                    case Research.Type.TurretDefense:
                    case Research.Type.TurretRange:
                    case Research.Type.TurretAttack:
                        Generate(game, false);
                        Generate(game, true);
                        break;
                    case Research.Type.Turret:
                    case Research.Type.TurretShields:
                        Generate(game, false);
                        break;
                    case Research.Type.LaserTurret:
                        Generate(game, true);
                        break;
                }
            }

            private void UpgradeBuildingCost(Game game, double researchMult)
            {
                this.costMult = game.ResearchUpgValues.Calc(UpgType.TurretCost, researchMult);
            }
            private void UpgradeTurretDefense(Game game, double researchMult)
            {
                this.def = game.ResearchUpgValues.Calc(UpgType.TurretDefense, researchMult);
                this.protection = game.ResearchUpgValues.Calc(UpgType.TurretProtection, researchMult);
                double resilience = game.ResearchUpgValues.Calc(UpgType.TurretResilience, researchMult);
                this.resilience = ResearchUpgValues.TurretResilience + (1 - ResearchUpgValues.TurretResilience) * (1 - resilience);
            }
            private void UpgradeTurretRange(Game game, double researchMult)
            {
                this.range = game.ResearchUpgValues.Calc(UpgType.TurretRange, researchMult);
                this.vision = game.ResearchUpgValues.Calc(UpgType.TurretVision, researchMult);
            }
            private void UpgradeTurretAttack(Game game, double researchMult)
            {
                this.att = game.ResearchUpgValues.Calc(UpgType.TurretAttack, researchMult);
            }
            private void UpgradeTurretShields(Game game)//, double researchMult)
            {
                SetCost(game.Consts, this.energy / this.mass, 1.3, out this.energy, out this.mass);
            }
        }

        [Serializable]
        [DataContract(IsReference = true)]
        public class Blueprint(bool laser, byte version, int researchLevel, double vision, double resilience,
            IReadOnlyList<IKillable.Values> killable, IReadOnlyList<IAttacker.Values> attacker) : IBlueprint
        {
            public static string GetName(bool laser) => (laser ? "Laser " : "") + "Turret";
            public string Name => GetName(Laser);

            public readonly bool Laser = laser;
            public readonly byte Version = version;
            public int ResearchLevel => researchLevel;

            public readonly int Energy;
            public readonly int Mass;

            public double Vision => vision;
            public readonly double Resilience = resilience;
            public IReadOnlyList<IKillable.Values> Killable => killable;
            public IReadOnlyList<IAttacker.Values> Attacker => attacker;

            public Blueprint(Game game, double energyCost, double massCost, bool laser, byte version, int researchLevel, double vision, double resilience,
                IReadOnlyList<IKillable.Values> killable, IReadOnlyList<IAttacker.Values> attacker)
                    : this(laser, version, researchLevel, vision, resilience, killable, attacker)
            {
                MechBlueprint.CalcCost(game, Research.GetResearchMult(game.Consts, researchLevel), vision,
                    killable, resilience, attacker, null, out double energy, out double mass);
                double total = energy + mass * game.Consts.EnergyMassRatio;
                energy = total * energyCost;
                mass = total * massCost;
                MechBlueprint.RoundCosts(game, energy, mass, out this.Energy, out this.Mass);
            }
        }
    }
}
