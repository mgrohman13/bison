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
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
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
            this.Killable = killable.ToList().AsReadOnly();
            this.Attacker = attacks.ToList().AsReadOnly();
            this.Movable = movable;

            CalcCost(out double energy, out double mass);
            this.Energy = Game.Rand.Round(energy);
            this.Mass = Game.Rand.Round(mass);
        }
        private void CalcCost(out double energy, out double mass)
        {
            double researchMult = Research.GetResearchMult(ResearchLevel);

            const double attPow = 1.13;
            double AttCost(IAttacker.Values a) => Consts.StatValue(a.Attack) * CombatTypes.Cost(a.Type)
                * Math.Sqrt((a.Range + 2.1) / (Attack.MELEE_RANGE + 2.1));
            double DefCost(IKillable.Values d) => Consts.StatValue(d.Defense) * CombatTypes.Cost(d.Type);

            double resilience = Math.Pow(Math.Pow(Resilience, Math.Log(3) / Math.Log(2)) * 1.5 + 0.5, .39);
            double att = Math.Pow(Attacker.Sum(AttCost), attPow) / researchMult;
            double def = Killable.Sum(DefCost) * resilience / researchMult;

            double vision = Vision;
            double move = 8 * Movable.MoveInc / 1.0 + 2 * Movable.MoveMax / 2.1 + 1 * Movable.MoveLimit / 3.9;
            move /= 8 + 2 + 1;
            double mult = Math.Sqrt(researchMult);
            move = (move + 2.6) * 26 / mult;
            vision = (vision + 6.5) * 3.9 / mult;

            double total = (att + vision) * (def + move) * Consts.MechCostMult;

            //Debug.WriteLine($"total: {total}");

            double energyPct = Math.Sqrt(att / (att + def));
            energyPct *= Math.Sqrt(move / (move + vision));

            double attEnergy = Attacker.Sum(a => AttCost(a) * CombatTypes.EnergyCostRatio(a.Type));
            double defEnergy = Killable.Sum(d => DefCost(d) * CombatTypes.EnergyCostRatio(d.Type));
            double attMass = Attacker.Sum(a => AttCost(a) * (1 - CombatTypes.EnergyCostRatio(a.Type)));
            double defMass = Killable.Sum(d => DefCost(d) * (1 - CombatTypes.EnergyCostRatio(d.Type)));

            energyPct = Math.Sqrt(energyPct * (attEnergy + defEnergy) / (attEnergy + defEnergy + attMass + defMass));

            energy = total * energyPct;
            mass = (total - energy) / Consts.MechMassDiv;
        }
        private int TotalCost()
        {
            return Energy + Mass;
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
                blueprint = NewBlueprint(research, blueprintNum);
            else do
                    blueprint = UpgradeBlueprint(upgrade, research, blueprintNum);
                while (blueprint.TotalCost() + Game.Rand.GaussianOEInt(169, .39, .26) < upgrade.TotalCost() + Game.Rand.GaussianOEInt(390, .39, .26));
            return CheckCost(blueprint, upgrade, research, blueprintNum);
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
            IEnumerable<IKillable.Values> killable = upgrade.Killable;
            IEnumerable<IAttacker.Values> attacker = upgrade.Attacker;
            IMovable.Values movable = upgrade.Movable;

            Type researching = research.GetType();
            int times = 1 + Game.Rand.OEInt(.39);
            for (int a = 0; a < times; a++)
            {
                switch (researching)
                {
                    case Type.MechAttack:
                        attacker = attacker.Select(attacker =>
                        {
                            IAttacker.Values newAttacker = Game.Rand.SelectValue(GenAttacker(research));
                            double range = attacker.Range;
                            if (newAttacker.Attack > attacker.Attack && Game.Rand.Bool())
                                range = newAttacker.Range;
                            AttackType type = attacker.Type;
                            if (range > attacker.Range && Game.Rand.Bool())
                                type = newAttacker.Type;
                            return new IAttacker.Values(attacker.Type, newAttacker.Attack, range);
                        });
                        break;
                    case Type.MechRange:
                        attacker = attacker.Select(attacker =>
                        {
                            IAttacker.Values newAttacker = Game.Rand.SelectValue(GenAttacker(research));
                            int att = attacker.Attack;
                            if (newAttacker.Range > attacker.Range && Game.Rand.Bool())
                                att = newAttacker.Attack;
                            AttackType type = attacker.Type;
                            if (att > attacker.Attack && Game.Rand.Bool())
                                type = newAttacker.Type;
                            return new IAttacker.Values(type, att, newAttacker.Range);
                        });
                        break;
                    case Type.MechExplosives:
                    case Type.MechLasers:
                    case Type.MechEnergyWeapons:
                        UpgAttackType();
                        break;
                    case Type.MechResilience:
                        resilience = GenResilience(research);
                        if (resilience > upgrade.Resilience)
                            killable = killable.Select(killable =>
                            {
                                IKillable.Values newKillable = GenKillable(research).Where(k => k.Type == CombatTypes.DefenseType.Hits).Single();
                                int def = killable.Defense;
                                if (Game.Rand.Bool())
                                    def = newKillable.Defense;
                                return new IKillable.Values(killable.Type, def);
                            });
                        break;
                    case Type.MechDefense:
                        bool defInc = false;
                        killable = killable.Select(killable =>
                        {
                            IKillable.Values newKillable = Game.Rand.SelectValue(GenKillable(research).Where(k => k.Type == CombatTypes.DefenseType.Hits || k.Type == killable.Type));
                            defInc |= newKillable.Defense > killable.Defense;
                            return new IKillable.Values(killable.Type, newKillable.Defense);
                        });
                        if (defInc)
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
                        double inc = movable.MoveInc;
                        if (newMovable.MoveInc > inc || Game.Rand.Bool())
                            inc = newMovable.MoveInc;
                        int max = movable.MoveMax;
                        if (newMovable.MoveMax > max || Game.Rand.Bool())
                            max = newMovable.MoveMax;
                        int limit = movable.MoveLimit;
                        if (newMovable.MoveLimit > limit || Game.Rand.Bool())
                            limit = newMovable.MoveLimit;
                        movable = new IMovable.Values(inc, max, limit);
                        break;
                    case Type.MechVision:
                        vision = GenVision(research);
                        if (vision > upgrade.Vision && Game.Rand.Bool())
                        {
                            newMovable = GenMovable(research);
                            movable = new IMovable.Values(newMovable.MoveInc, newMovable.MoveMax, newMovable.MoveLimit);
                        }
                        break;
                    default:
                        throw new Exception();
                }
                void UpgAttackType()
                {
                    int numAttacks = attacker.Count();
                    if (Game.Rand.Next(numAttacks + 1) > 0)
                    {
                        int mod = Game.Rand.Next(numAttacks);
                        int idx = 0;
                        attacker = attacker.Select(a =>
                        {
                            IAttacker.Values newAttacker = GenAttacker(research).First();
                            AttackType newType = idx++ == mod || Game.Rand.Bool() ? newAttacker.Type : a.Type;
                            int att = a.Attack;
                            if (newType != a.Type && Game.Rand.Bool())
                                att = newAttacker.Attack;
                            double range = a.Range;
                            if (newType != a.Type && ((a.Range == Attack.MELEE_RANGE) != (a.Range == Attack.MELEE_RANGE) || Game.Rand.Bool()))
                                range = newAttacker.Range;
                            return new IAttacker.Values(newType, att, range);
                        });
                    }
                    else
                    {
                        attacker = attacker.Concat(new[] { GenAttacker(research).First() });
                    }
                }
                void UpgDefenseType(DefenseType type)
                {
                    bool defInc = false;
                    killable = killable.Select(killable =>
                    {
                        IKillable.Values newKillable = Game.Rand.SelectValue(GenKillable(research).Where(k => k.Type == type || k.Type == killable.Type));
                        if (killable.Type == type || Game.Rand.Bool())
                        {
                            int def = killable.Defense;
                            defInc |= newKillable.Defense > killable.Defense;
                            killable = new IKillable.Values(killable.Type, killable.Defense);
                        }
                        return killable;
                    });
                    if (defInc)
                        resilience = GenResilience(research);
                }

                researching = Game.Rand.SelectValue(Enum.GetValues<Research.Type>().Where(Research.IsMech).Where(t => t != researching));
            }

            return new(blueprintNum, upgrade, research.GetLevel(), vision, killable, resilience, attacker, movable);
        }
        private static MechBlueprint CheckCost(MechBlueprint blueprint, MechBlueprint upgrade, IResearch research, int blueprintNum)
        {
            Type researching = research.GetType();
            int minTotal, maxTotal;
            {
                minTotal = research.GetMinCost();
                maxTotal = research.GetMaxCost();
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

                int sum = blueprint.Killable.Select(k => k.Defense).Concat(blueprint.Attacker.Select(a => a.Attack)).Sum();
                int select = Game.Rand.Next(sum);

                List<IKillable.Values> killable = new();
                List<IAttacker.Values> attacker = new();
                int mod = increase ? 1 : -1;
                bool changed = false;
                foreach (var k in Game.Rand.Iterate(blueprint.Killable))
                {
                    select -= k.Defense;
                    if (select < 0)
                    {
                        select = sum;
                        changed = increase || k.Defense > 1;
                        killable.Add(new(k.Type, Math.Max(1, k.Defense + mod)));
                    }
                    else
                    {
                        killable.Add(k);
                    }
                }
                foreach (var a in Game.Rand.Iterate(blueprint.Attacker))
                {
                    select -= a.Attack;
                    if (select < 0)
                    {
                        select = sum;
                        changed = increase || a.Attack > 1;
                        attacker.Add(new(a.Type, Math.Max(1, a.Attack + mod)));
                    }
                    else
                    {
                        attacker.Add(a);
                    }
                }

                if (changed)
                    blueprint = new(blueprintNum, blueprint.UpgradeFrom, blueprint.ResearchLevel, blueprint.Vision, killable, blueprint.Resilience, attacker, blueprint.Movable);
                return changed;
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
            IKillable.Values hits = GenType(DefenseType.Hits, null, 0.91);
            List<IKillable.Values> defenses = new() { hits };
            if (research.GetType() == Type.MechShields || research.MakeType(Type.MechShields))
                defenses.Add(GenType(DefenseType.Shield, Type.MechShields, 0.65));
            if (research.GetType() == Type.MechArmor || research.MakeType(Type.MechArmor))
                defenses.Add(GenType(DefenseType.Armor, Type.MechArmor, 1.04));
            return defenses.AsReadOnly();

            IKillable.Values GenType(DefenseType type, Type? additionalResearch, double mult)
            {
                double avg = 3.9, dev = .26, oe = .078;
                ModValues(research.GetType() == Type.MechDefense, 1.69, ref avg, ref dev, ref oe);
                if (additionalResearch.HasValue)
                    ModValues(research.GetType() == additionalResearch, 1.69, ref avg, ref dev, ref oe);

                const double defResearchPow = .6;
                double researchMult = research.GetMult(Type.MechDefense, defResearchPow);
                if (additionalResearch.HasValue)
                    researchMult = Math.Sqrt(researchMult) * research.GetMult(additionalResearch.Value, defResearchPow / 2.0);

                int defense = Game.Rand.GaussianOEInt(2.6 + avg * mult * researchMult, dev, oe, 1);
                return new(type, defense);
            }
        }
        private static IReadOnlyCollection<IAttacker.Values> GenAttacker(IResearch research)
        {
            int numAttacks = research.GetType() == Type.Mech ? 1 : Game.Rand.GaussianOEInt(1.3 * research.GetMult(Type.MechAttack, .3), .26, .13, 1);
            List<IAttacker.Values> attacks = new(numAttacks);

            HashSet<AttackType> used = new();
            bool usedRange = false;

            for (int a = 0; a < numAttacks; a++)
            {
                Type researchType = a > 0 ? Type.Mech : research.GetType();

                AttackType type = GetAttackType(researchType, out bool isLaser);
                bool ranged = IsRanged(researchType, isLaser, type);
                double range = GetRange(researchType, ranged, out double rangeAvg);

                HashSet<Type> apply = GetResearchTypes(type, ranged);

                //modify for current research type
                double attAvg = 2.6, dev = .26, oe = .13;
                foreach (var item in apply) // Game.Rand.Iterate(apply))
                    ModValues(researchType == item, 2.1, ref attAvg, ref dev, ref oe);

                //modify for research totals
                double researchMult = 1;
                foreach (var item in apply) //Game.Rand.Iterate(apply))
                    researchMult *= research.GetMult(item, .7);
                researchMult = Math.Pow(researchMult, 1.0 / apply.Count);
                attAvg *= researchMult;

                //modify for attack type
                attAvg *= type switch
                {
                    AttackType.Kinetic => 1.3,
                    AttackType.Energy => 1.04,
                    AttackType.Explosive => 0.91,
                    _ => throw new Exception()
                };

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
            bool IsRanged(Type researchType, bool isLaser, AttackType type)
            {
                bool ranged = isLaser || type == AttackType.Explosive ||
                    researchType == Type.MechRange || research.MakeType(Type.MechRange);
                if ((ranged && type == AttackType.Energy && !research.HasType(Type.MechLasers))
                        || (usedRange && Game.Rand.Bool()))
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
                    rangeAvg = 5.2;
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
            double avg = 13, dev = .13, oe = .21;

            double researchMult = research.GetMult(Type.MechMove, 1);
            const double lowPenalty = 5;
            if (researchMult < lowPenalty)
                avg *= researchMult / lowPenalty;

            ModValues(research.GetType() == Type.MechMove, 1.69, ref avg, ref dev, ref oe);

            double move = Game.Rand.GaussianOE(avg * research.GetMult(Type.MechMove, .4), dev, oe, 1);
            int max = Game.Rand.GaussianOEInt(move * 2, dev * 2.6, oe * 1.3, (int)move + 1);
            int limit = Game.Rand.GaussianOEInt(move + max, dev * 2.6, oe * 2.6, max + 1);

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
