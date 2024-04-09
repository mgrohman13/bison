using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AttackType = ClassLibrary1.Pieces.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.CombatTypes.DefenseType;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    public class MechBlueprint : IComparable<MechBlueprint>
    {
        public readonly MechBlueprint UpgradeFrom;
        public MechBlueprint UpgradeTo { get; private set; }
        public readonly string BlueprintNum;
        public readonly int Energy;
        public readonly int Mass;
        public readonly int ResearchLevel;
        public readonly double Vision;
        public readonly double Resilience;
        public readonly IReadOnlyCollection<IKillable.Values> Killable;
        public readonly IReadOnlyCollection<IAttacker.Values> Attacker;
        public readonly IMovable.Values Movable;

        private MechBlueprint(int blueprintNum, MechBlueprint upgrade, int research, double vision,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacker, IMovable.Values movable)
        {
            this.BlueprintNum = "";
            while (blueprintNum > 0)
            {
                BlueprintNum += (char)(--blueprintNum % 26 + 65);
                blueprintNum /= 26;
            }

            this.UpgradeFrom = upgrade;
            this.ResearchLevel = research;
            this.Vision = vision;
            this.Resilience = resilience;
            this.Killable = CombatTypes.OrderDef(killable);
            this.Attacker = CombatTypes.OrderAtt(attacker);
            this.Movable = movable;

            CalcCost(out double energy, out double mass);
            this.Energy = Game.Rand.Round(energy / 10.0) * 10;
            this.Mass = Game.Rand.Round((mass + (energy - this.Energy) / Consts.MechMassDiv) / 5.0) * 5;
        }
        private void CalcCost(out double energy, out double mass)
        {
            double researchMult = Research.GetResearchMult(ResearchLevel);
            CalcCost(researchMult, Vision, Killable, Resilience, Attacker, Movable, out energy, out mass);
        }
        public static void CalcCost(double researchMult, double vision, IEnumerable<IKillable.Values> killable, double resilience,
            IEnumerable<IAttacker.Values> attacker, IMovable.Values? movable, out double energy, out double mass)
        {
            const double attPow = 1.13;
            double AttCost(IAttacker.Values a) => Consts.StatValue(a.Attack) * Math.Sqrt(a.Reload / CombatTypes.ReloadAvg(a.Attack))
                * CombatTypes.Cost(a.Type) * Math.Sqrt((a.Range + 2.1) / (Attack.MELEE_RANGE + 2.1));
            double DefCost(IKillable.Values d) => Consts.StatValue(d.Defense) * CombatTypes.Cost(d.Type);

            double r = Math.Pow(Math.Pow(resilience, Math.Log(3) / Math.Log(2)) * 1.5 + 0.5, .39);
            double att = Math.Pow(attacker.Sum(AttCost), attPow) / researchMult;
            double def = killable.Sum(DefCost) * r / researchMult;

            double v = vision;
            double move = Consts.MoveValue(movable);
            double mult = Math.Sqrt(researchMult);
            move = (move + 2.6) * 26 / mult;
            v = (v + 6.5) * 3.9 / mult;

            double total = (att + v) * (def + move) * Consts.MechCostMult;

            //Debug.WriteLine($"total: {total}");

            double energyPct = Math.Sqrt(att / (att + def));
            energyPct *= Math.Sqrt(move / (move + v));

            double attEnergy = attacker.Sum(a => AttCost(a) * CombatTypes.EnergyCostRatio(a.Type));
            double defEnergy = killable.Sum(d => DefCost(d) * CombatTypes.EnergyCostRatio(d.Type));
            double attMass = attacker.Sum(a => AttCost(a) * (1 - CombatTypes.EnergyCostRatio(a.Type)));
            double defMass = killable.Sum(d => DefCost(d) * (1 - CombatTypes.EnergyCostRatio(d.Type)));

            energyPct = Math.Sqrt(energyPct * (attEnergy + defEnergy) / (attEnergy + defEnergy + attMass + defMass));

            energy = total * energyPct;
            mass = (total - energy) / Consts.MechMassDiv;
        }

        public int TotalCost()
        {
            return Energy + Mass;
        }
        public double AlienCost()
        {
            return Energy + Mass * Consts.MechMassDiv;
        }

        internal static MechBlueprint Alien(IResearch research)
        {
            return GenBlueprint(null, research, 0);
        }
        internal static MechBlueprint OnResearch(IResearch research, SortedSet<MechBlueprint> blueprints)
        {
            MechBlueprint upgrade = Game.Rand.SelectValue(new object[] { "" }.Concat(blueprints), b =>
            {
                double chance;
                if (b is MechBlueprint blueprint)
                {
                    chance = research.GetLevel() - blueprint.ResearchLevel;
                    if (chance > 0)
                        chance *= chance;
                    else
                        chance = 0;
                }
                else
                {
                    chance = Consts.ResearchFactor * Consts.ResearchFactor;
                }
                return Game.Rand.Round(chance);
            }) as MechBlueprint;

            MechBlueprint newBlueprint = GenBlueprint(upgrade, research, research.Game.GetPieceNum(typeof(MechBlueprint)));
            blueprints.Add(newBlueprint);
            if (upgrade != null)
            {
                upgrade.UpgradeTo = newBlueprint;
                blueprints.Remove(upgrade);
            }
            return newBlueprint;
        }
        private static MechBlueprint GenBlueprint(MechBlueprint upgrade, IResearch research, int blueprintNum)
        {
            MechBlueprint blueprint;
            if (upgrade == null)
                blueprint = CheckCost(NewBlueprint(research, blueprintNum), upgrade, research, blueprintNum);
            else do
                    blueprint = CheckCost(UpgradeBlueprint(upgrade, research, blueprintNum), upgrade, research, blueprintNum);
                while (!UpgradeValid(blueprint, upgrade, research));
            return blueprint;
        }

        private static MechBlueprint NewBlueprint(IResearch research, int blueprintNum)
        {
            double vision = GenVision(research);
            double resilience = GenResilience(research);
            IReadOnlyCollection<IKillable.Values> killable = GenKillable(research);
            IReadOnlyCollection<IAttacker.Values> attacker = GenAttacker(research);
            IMovable.Values movable = GenMovable(research);
            return new(blueprintNum, null, research.GetLevel(), vision, killable, resilience, attacker, movable);
        }

        private static MechBlueprint UpgradeBlueprint(MechBlueprint upgrade, IResearch research, int blueprintNum)
        {
            double resilience = upgrade.Resilience;
            double vision = upgrade.Vision;
            List<IKillable.Values> killable = upgrade.Killable.ToList();
            List<IAttacker.Values> attacker = upgrade.Attacker.ToList();
            IMovable.Values movable = upgrade.Movable;

            Type upgType = research.GetType();
            HashSet<Type> done = new() { };
            int times = 1 + Game.Rand.OEInt(.5);
            for (int a = 0; a < times; a++)
            {
                switch (upgType)
                {
                    case Type.MechAttack:
                        attacker = Game.Rand.Iterate(attacker).Select(attack =>
                        {
                            AttackType type = attack.Type;
                            int att = attack.Attack;
                            double range = attack.Range;
                            IAttacker.Values newAttacker = Game.Rand.SelectValue(GenAttacker(research));
                            if (Game.Rand.Next(newAttacker.Attack + 1) >= Game.Rand.Next(att + 1))
                            {
                                att = newAttacker.Attack;
                                if (Game.Rand.Bool())
                                {
                                    range = newAttacker.Range;
                                    if (Game.Rand.Bool() || CheckTypeRange(attack, newAttacker))
                                        type = newAttacker.Type;
                                }
                            }
                            return new IAttacker.Values(type, att, range);
                        }).ToList();
                        break;
                    case Type.MechRange:
                        attacker = Game.Rand.Iterate(attacker).Select(attack =>
                        {
                            AttackType type = attack.Type;
                            int att = attack.Attack;
                            double range = attack.Range;
                            IAttacker.Values newAttacker = Game.Rand.SelectValue(GenAttacker(research));
                            if (Game.Rand.DoubleFull(newAttacker.Range) >= Game.Rand.DoubleFull(range))
                            {
                                range = newAttacker.Range;
                                if (Game.Rand.Bool())
                                    att = newAttacker.Attack;
                                if (Game.Rand.Bool() || CheckTypeRange(attack, newAttacker))
                                    type = newAttacker.Type;
                            }
                            return new IAttacker.Values(type, att, range);
                        }).ToList();
                        break;
                    case Type.MechExplosives:
                        UpgAttackType(AttackType.Explosive);
                        break;
                    case Type.MechLasers:
                    case Type.MechEnergyWeapons:
                        UpgAttackType(AttackType.Energy);
                        break;
                    case Type.MechResilience:
                        resilience = GenResilience(research);
                        if (Game.Rand.Bool())
                            killable = Game.Rand.Iterate(killable).Select(defense =>
                            {
                                IKillable.Values newKillable = GenKillable(research).Where(k => k.Type == DefenseType.Hits).Single();
                                int def = defense.Defense;
                                if (defense.Type == DefenseType.Hits)
                                    def = newKillable.Defense;
                                return new IKillable.Values(defense.Type, def);
                            }).ToList();
                        break;
                    case Type.MechDefense:
                        killable = Game.Rand.Iterate(killable).Select(defense =>
                        {
                            int def = defense.Defense;
                            IKillable.Values newKillable = Game.Rand.SelectValue(GenKillable(research).Where(k => k.Type == DefenseType.Hits || k.Type == defense.Type));
                            if (Game.Rand.Next(newKillable.Defense + 1) >= Game.Rand.Next(def + 1))
                                def = newKillable.Defense;
                            return new IKillable.Values(defense.Type, def);
                        }).ToList();
                        if (Game.Rand.Bool())
                            resilience = GenResilience(research);
                        break;
                    case Type.MechArmor:
                        UpgDefenseType(DefenseType.Armor);
                        break;
                    case Type.MechShields:
                        UpgDefenseType(DefenseType.Shield);
                        break;
                    case Type.MechMove:
                        IMovable.Values newMovable = GenMovable(research);
                        double inc = newMovable.MoveInc;
                        int max = movable.MoveMax;
                        if (max <= inc || (newMovable.MoveMax > inc && Game.Rand.Bool()))
                            max = newMovable.MoveMax;
                        int limit = movable.MoveLimit;
                        if (limit <= max || (newMovable.MoveLimit > max && Game.Rand.Bool()))
                            limit = newMovable.MoveLimit;
                        movable = new IMovable.Values(inc, max, limit);
                        if (Game.Rand.Bool())
                            vision = GenVision(research);
                        break;
                    case Type.MechVision:
                        vision = GenVision(research);
                        if (Game.Rand.Bool())
                        {
                            newMovable = GenMovable(research);
                            movable = new IMovable.Values(newMovable.MoveInc, newMovable.MoveMax, newMovable.MoveLimit);
                        }
                        break;
                    default:
                        throw new Exception();
                }
                if (!done.Add(upgType))
                    ;
                upgType = Game.Rand.SelectValue(Enum.GetValues<Research.Type>().Where(Research.IsMech)
                    .Concat(new[] { Type.MechMove }) //more likely to pick
                    .Where(t => !done.Contains(t) || Game.Rand.Next(13) == 0) //small chance of picking the same type again
                    .Concat(new[] { Type.MechResilience, Type.MechVision })); //can pick multiple times
            }
            return new(blueprintNum, upgrade, research.GetLevel(), vision, killable, resilience, attacker, movable);

            void UpgAttackType(AttackType upgAtt)
            {
                attacker = Game.Rand.Iterate(attacker).Select(attack =>
                {
                    AttackType type = attack.Type;
                    int att = attack.Attack;
                    double range = attack.Range;
                    IAttacker.Values newAttacker = GenAtt();
                    if (type != newAttacker.Type && Game.Rand.Bool())
                    {
                        type = newAttacker.Type;
                        if (Game.Rand.Bool())
                            att = newAttacker.Attack;
                        if (Game.Rand.Bool() || CheckTypeRange(attack, newAttacker))
                            range = newAttacker.Range;
                    }
                    return new IAttacker.Values(type, att, range);
                }).ToList();

                double trgAtts = NumAtts(research);
                int numAttacks = attacker.Count;
                if (CheckNumAtts())
                    attacker.Add(GenAtt());
                numAttacks = attacker.Count;
                HashSet<AttackType> seen = new();
                attacker = Game.Rand.Iterate(attacker).Where(a =>
                {
                    bool keep = seen.Add(a.Type) || CheckNumAtts();
                    if (!keep)
                        numAttacks--;
                    return keep;
                }).ToList();
                if (numAttacks != attacker.Count)
                    throw new Exception();

                IAttacker.Values GenAtt()
                {
                    IEnumerable<IAttacker.Values> genAtt = GenAttacker(research);
                    bool IsUpg(IAttacker.Values a) => a.Type == upgAtt;
                    if (genAtt.Any(IsUpg) && Game.Rand.Bool())
                        genAtt = genAtt.Where(IsUpg);
                    return Game.Rand.Bool() ? Game.Rand.SelectValue(genAtt) : genAtt.First();
                }
                bool CheckNumAtts() => Game.Rand.DoubleHalf(numAttacks) <= Game.Rand.DoubleHalf(trgAtts);
            }
            static bool CheckTypeRange(IAttacker.Values attack, IAttacker.Values newAttacker) =>
                attack.Type != newAttacker.Type && (attack.Range > Attack.MELEE_RANGE) != (newAttacker.Range > Attack.MELEE_RANGE);
            void UpgDefenseType(DefenseType upgDef)
            {
                killable = Game.Rand.Iterate(killable).Select(defense =>
                {
                    int def = defense.Defense;
                    IKillable.Values newKillable = GenDef();
                    if (defense.Type == newKillable.Type || Game.Rand.Bool())
                        def = newKillable.Defense;
                    defense = new IKillable.Values(defense.Type, def);
                    return defense;
                }).ToList();

                IKillable.Values addKillable = GenDef();
                if (!killable.Any(k => k.Type == addKillable.Type))
                    killable = killable.Concat(new[] { addKillable }).ToList();

                killable.RemoveAll(k => k.Type != DefenseType.Hits && k.Type != upgDef && Game.Rand.Bool());

                IKillable.Values GenDef()
                {
                    IEnumerable<IKillable.Values> genDef = GenKillable(research);
                    bool IsUpg(IKillable.Values k) => k.Type == upgDef;
                    bool NotHits(IKillable.Values k) => k.Type != DefenseType.Hits;
                    if (genDef.Any(IsUpg) && Game.Rand.Bool(.91))
                        genDef = genDef.Where(IsUpg);
                    else if (genDef.Any(NotHits) && Game.Rand.Bool())
                        genDef = genDef.Where(NotHits);
                    return Game.Rand.SelectValue(genDef);
                }
            }
        }
        private static bool UpgradeValid(MechBlueprint blueprint, MechBlueprint upgrade, IResearch research)
        {
            Func<MechBlueprint, double?> GetRaw = research.GetType() switch
            {
                Type.MechAttack => b => b.Attacker.Sum(a => (double?)Consts.StatValue(a.Attack)),
                Type.MechRange => b => b.Attacker.Sum(a => (double?)a.Range * Consts.StatValue(a.Attack)) / b.Attacker.Sum(a => (double?)Consts.StatValue(a.Attack)),
                Type.MechExplosives => b => b.Attacker.Where(a => a.Type == AttackType.Explosive).Sum(a => (double?)Consts.StatValue(a.Attack)),
                Type.MechLasers => b => b.Attacker.Where(a => a.Type == AttackType.Energy && a.Range > Attack.MELEE_RANGE).Sum(a => (double?)Consts.StatValue(a.Attack)),
                Type.MechEnergyWeapons => b => b.Attacker.Where(a => a.Type == AttackType.Energy).Sum(a => (double?)Consts.StatValue(a.Attack)),
                Type.MechResilience => b => Consts.StatValue(b.Resilience * 13),
                Type.MechDefense => b => b.Killable.Sum(k => (double?)Consts.StatValue(k.Defense)),
                Type.MechArmor => b => b.Killable.Where(k => k.Type == DefenseType.Armor).Sum(k => (double?)Consts.StatValue(k.Defense)),
                Type.MechShields => b => b.Killable.Where(k => k.Type == DefenseType.Shield).Sum(k => (double?)Consts.StatValue(k.Defense)),
                Type.MechMove => b => Consts.StatValue(Consts.MoveValue(b.Movable) * 1.3),
                Type.MechVision => b => Consts.StatValue(b.Vision),
                _ => throw new Exception(),
            };
            double offset = Game.Rand.NextDouble();
            double GetValue(MechBlueprint b) => Consts.StatValueInverse(GetRaw(b) ?? 0) + offset;
            double oldVal = GetValue(upgrade), newVal = GetValue(blueprint);
            bool valid = Game.Rand.Round(oldVal + Game.Rand.OE(.13)) < Game.Rand.Round(newVal);
            if (!valid)
                Debug.WriteLine($"{research.GetType()} upgrade invalid ({(float)oldVal} -> {(float)newVal})");
            return valid;
        }

        private static MechBlueprint CheckCost(MechBlueprint blueprint, MechBlueprint upgrade, IResearch research, int blueprintNum)
        {
            Type researching = research.GetType();
            int minTotal, maxTotal;
            {
                minTotal = research.GetMinCost();
                maxTotal = research.GetMaxCost();
                if (upgrade != null)
                {
                    minTotal = Math.Max(minTotal, Game.Rand.Round(upgrade.TotalCost() * 0.65));
                    maxTotal = Math.Min(maxTotal, Game.Rand.Round(upgrade.TotalCost() * 1.69));
                }
                if (researching != Type.Mech)
                {
                    const double minDev = .13;
                    minTotal = Game.Rand.GaussianCappedInt(minTotal, minDev);
                    const double maxDev = .21, maxOE = .169;
                    maxTotal = Game.Rand.GaussianOEInt(maxTotal, maxDev, maxOE, minTotal);
                }
            }

            bool canKeep = true;
            while (blueprint.TotalCost() < minTotal && (canKeep &= ModStat(true))) ;
            while (blueprint.TotalCost() > maxTotal && (canKeep &= ModStat(false))) ;

            if ((!canKeep || research.GetType() == Type.Mech || Game.Rand.Bool()) && (blueprint.TotalCost() < minTotal || blueprint.TotalCost() > maxTotal))
                blueprint = GenBlueprint(upgrade, research, blueprintNum);

            return blueprint;

            bool ModStat(bool increase)
            {
                Debug.WriteLine($"ModStat: {blueprint.TotalCost()} ({minTotal}-{maxTotal})");

                int sum = blueprint.Killable.Where(CanModDef).Select(k => (int?)k.Defense)
                    .Concat(blueprint.Attacker.Where(CanModAtt).Select(a => (int?)a.Attack))
                    .Sum() ?? 0;
                if (sum == 0)
                    return false;

                List<IKillable.Values> killable = new();
                List<IAttacker.Values> attacker = new();
                int mod = increase ? 1 : -1;
                int select = Game.Rand.Next(sum);
                foreach (var k in Game.Rand.Iterate(blueprint.Killable))
                {
                    select -= CanModDef(k) ? k.Defense : 0;
                    if (select < 0)
                        killable.Add(new(k.Type, Math.Max(1, k.Defense + mod)));
                    else
                        killable.Add(k);
                }
                foreach (var a in Game.Rand.Iterate(blueprint.Attacker))
                {
                    select -= CanModAtt(a) ? a.Attack : 0;
                    if (select < 0)
                        attacker.Add(new(a.Type, Math.Max(1, a.Attack + mod), a.Range));
                    else
                        attacker.Add(a);
                }

                blueprint = new(blueprintNum, blueprint.UpgradeFrom, blueprint.ResearchLevel, blueprint.Vision, killable, blueprint.Resilience, attacker, blueprint.Movable);
                return true;

                bool CanModDef(IKillable.Values k) => increase || k.Defense > 1;
                bool CanModAtt(IAttacker.Values a) => increase || a.Attack > 1;
            }
        }

        private static double GenVision(IResearch research)
        {
            double avg = 5.2, dev = .39, oe = .091;
            ModValues(research.GetType() == Type.MechVision, 2.1, ref avg, ref dev, ref oe);
            double vision = Game.Rand.GaussianOE(avg, dev, oe, 1);
            vision *= research.GetMult(Type.MechVision, .5);
            return vision;
        }
        private static double GenResilience(IResearch research)
        {
            bool isResilience = research.GetType() == Type.MechResilience;
            return Consts.GetPct(Game.Rand.GaussianCapped(.39, .091, .169),
                Math.Pow(research.GetMult(Type.MechResilience, 1) + (isResilience ? .52 : 0), isResilience ? .5 : .2));
        }
        private static IReadOnlyCollection<IKillable.Values> GenKillable(IResearch research)
        {
            IKillable.Values hits = GenType(DefenseType.Hits, null, 1.04);
            List<IKillable.Values> defenses = new() { hits };
            if (research.GetType() == Type.MechShields || research.MakeType(Type.MechShields))
                defenses.Add(GenType(DefenseType.Shield, Type.MechShields, 0.65));
            if (research.GetType() == Type.MechArmor || research.MakeType(Type.MechArmor))
                defenses.Add(GenType(DefenseType.Armor, Type.MechArmor, 0.91));
            return defenses.AsReadOnly();

            IKillable.Values GenType(DefenseType type, Type? additionalResearch, double mult)
            {
                double avg = 5.2, dev = .26, oe = .078;
                ModValues(research.GetType() == Type.MechDefense, 1.69, ref avg, ref dev, ref oe);
                if (additionalResearch.HasValue)
                    ModValues(research.GetType() == additionalResearch, 1.69, ref avg, ref dev, ref oe);

                const double defResearchPow = .7;
                double researchMult = research.GetMult(Type.MechDefense, defResearchPow);
                if (additionalResearch.HasValue)
                    researchMult = Math.Sqrt(researchMult) * research.GetMult(additionalResearch.Value, defResearchPow / 2.0);

                int defense = Game.Rand.GaussianOEInt(2.6 + avg * mult * researchMult, dev, oe, 1);
                return new(type, defense);
            }
        }
        private static double NumAtts(IResearch research) => 1.13 * research.GetMult(Type.MechAttack, .3);

        private static IReadOnlyCollection<IAttacker.Values> GenAttacker(IResearch research)
        {
            int numAttacks = research.GetType() == Type.Mech ? 1 : Game.Rand.GaussianOEInt(NumAtts(research), .26, .13, 1);
            if (numAttacks < 1)
                numAttacks = 1;
            List<IAttacker.Values> attacks = new(numAttacks);

            HashSet<AttackType> used = new();
            bool usedRange = false;

            for (int a = 0; a < numAttacks; a++)
            {
                Type researchType = a > 0 ? Type.Mech : research.GetType();

                AttackType type = GetAttackType(researchType, out bool isLaser);
                bool ranged = IsRanged(researchType, isLaser, ref type);
                double range = GetRange(researchType, ranged, out double rangeAvg);

                HashSet<Type> apply = GetResearchTypes(type, ranged);

                //modify for current research type
                double attAvg = 3.9, dev = .26, oe = .13;
                foreach (var item in apply)//Game.Rand.Iterate(apply))
                    ModValues(researchType == item, 2.1, ref attAvg, ref dev, ref oe);

                //modify for research totals
                double researchMult = 1;
                foreach (var item in apply)
                    researchMult *= research.GetMult(item, .8);
                researchMult = Math.Pow(researchMult, 1.0 / apply.Count);
                attAvg *= researchMult;

                //modify for attack type
                attAvg *= CombatTypes.GetDamageMult(type);

                //modify for multiple attacks and range
                attAvg = 1 + (attAvg - 1) * Math.Sqrt(rangeAvg / range / numAttacks);

                int attack = Game.Rand.GaussianOEInt(attAvg, dev, oe, 1);
                attacks.Add(new(type, attack, range));
            }
            return attacks.AsReadOnly();

            AttackType GetAttackType(Type researchType, out bool isLaser)
            {
                isLaser = researchType == Type.MechLasers || research.MakeType(Type.MechLasers);
                AttackType type = AttackType.Kinetic;
                if (researchType == Type.MechExplosives)
                    type = AttackType.Explosive;
                else if (isLaser || researchType == Type.MechEnergyWeapons || research.MakeType(Type.MechEnergyWeapons))
                    type = AttackType.Energy;
                else if (research.MakeType(Type.MechExplosives))
                    type = AttackType.Explosive;
                if (used.Contains(type) && Game.Rand.Bool())
                    type = AttackType.Kinetic;
                used.Add(type);
                return type;
            }
            bool IsRanged(Type researchType, bool isLaser, ref AttackType type)
            {
                bool ranged = isLaser || type == AttackType.Explosive ||
                    researchType == Type.MechRange || research.MakeType(Type.MechRange);
                if (ranged && type == AttackType.Energy && !research.HasType(Type.MechLasers))
                    if (researchType == Type.MechRange)
                        type = AttackType.Kinetic;
                    else
                        ranged = false;
                if (ranged && usedRange && Game.Rand.Bool())
                    ranged = false;
                usedRange |= ranged;
                return ranged;
            }
            double GetRange(Type researchType, bool ranged, out double rangeAvg)
            {
                double range = Attack.MELEE_RANGE;
                rangeAvg = range;
                if (ranged)
                {
                    rangeAvg = 6.5;
                    double dev = .39, oe = .104;
                    ModValues(researchType == Type.MechRange, 2.1, ref rangeAvg, ref dev, ref oe);
                    rangeAvg *= research.GetMult(Type.MechRange, .6);
                    range = Game.Rand.GaussianOE(rangeAvg, dev, oe, Attack.MIN_RANGED);
                }
                return range;
            }
            static HashSet<Type> GetResearchTypes(AttackType type, bool ranged)
            {
                HashSet<Type> apply = new() { Type.MechAttack };
                switch (type)
                {
                    case AttackType.Energy:
                        apply.Add(Type.MechEnergyWeapons);
                        if (ranged)
                            apply.Add(Type.MechLasers);
                        break;
                    case AttackType.Explosive:
                        apply.Add(Type.MechExplosives);
                        break;
                }
                return apply;
            }
        }

        private static IMovable.Values GenMovable(IResearch research)
        {
            double avg = 6.5, dev = .13, oe = .21;

            double researchMult = research.GetMult(Type.MechMove, 1);
            const double lowPenalty = 3;
            if (researchMult < lowPenalty)
                avg *= researchMult / lowPenalty;

            ModValues(research.GetType() == Type.MechMove, 1.69, ref avg, ref dev, ref oe);

            double move = Game.Rand.GaussianOE(1 + avg * research.GetMult(Type.MechMove, .4), dev, oe, 1);
            int max = Game.Rand.GaussianOEInt(1 + move * 2, dev * 2.6, oe * 1.3, (int)move + 1);
            int limit = Game.Rand.GaussianOEInt(1 + move + max, dev * 2.6, oe * 2.6, max + 1);

            return new(move, max, limit);
        }
        private static void ModValues(bool match, double mult, ref double avg, ref double dev, ref double oe)
        {
            if (match)
            {
                avg *= mult;
                mult = Math.Sqrt(mult);
                dev /= mult;
                oe *= mult;
            }
        }

        public int CompareTo(MechBlueprint other)
        {
            return Math.Sign(this.ResearchLevel - other.ResearchLevel);
        }

        public override string ToString()
        {
            return "Type " + BlueprintNum;
        }
    }
}
