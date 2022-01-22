using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
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
        public readonly IKillable.Values Killable;
        public readonly IReadOnlyCollection<IAttacker.Values> Attacks;
        public readonly IMovable.Values Movable;
        private MechBlueprint(int blueprintNum, MechBlueprint upgrade, int research, double vision, IKillable.Values killable, IEnumerable<IAttacker.Values> attacks, IMovable.Values movable)
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
            this.Killable = killable;
            this.Attacks = attacks.ToList().AsReadOnly();
            this.Movable = movable;

            CalcCost(out double energy, out double mass);
            this.Energy = Game.Rand.Round(energy);
            this.Mass = Game.Rand.Round(mass);
        }
        private void CalcCost(out double energy, out double mass)
        {
            double researchMult = Research.GetResearchMult(ResearchLevel);

            double resilience = Math.Pow(Math.Pow(Killable.Resilience, Math.Log(3) / Math.Log(2)) * 1.5 + 0.5, .39);
            double armor = 1 / (1 - Killable.Armor * (1 - .21));
            double hp = Killable.HitsMax * armor * resilience;
            double shield = Killable.ShieldInc * 13 + Killable.ShieldMax * 1 + Killable.ShieldLimit / 2.6;
            shield /= 3.0;

            double dmg = 0, rng = 0, ap = 0, sp = 0, cnt = 0;
            foreach (IAttacker.Values attack in Attacks)
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

            double vision = (Vision + 5.2) / 9.1;
            double move = 26 * Movable.MoveInc / 1.0 + 2 * Movable.MoveMax / 2.1 + 1 * Movable.MoveLimit / 3.9;
            move /= 26 + 2 + 1;

            hp /= Math.Pow(researchMult, .9);
            shield /= Math.Pow(researchMult, .7);
            rng /= Math.Pow(researchMult, .6);
            dmg /= Math.Pow(researchMult, .8);
            vision /= Math.Pow(researchMult, .5);
            move /= Math.Pow(researchMult, .4);
            rng *= 9.1;
            move += 2.6;
            double total = Math.Sqrt((hp + shield + rng) * (dmg + vision) * move) * Consts.MechCostMult;

            shield *= 3.9;
            shield = (1 + shield) / (1.0 + hp + shield);
            ap = (((1 + ap) / (2 + 1.5 * ap + sp) - 1 / 3.0) * 21 / 5.0 + .052) / 1.052;
            rng /= (78 + rng);
            move /= (9.1 + move);
            double energyPct = Math.Pow(shield * ap * rng * move, 1 / 4.5);

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
            IKillable.Values killable = GenKillable(research);
            IReadOnlyCollection<IAttacker.Values> attacker = GenAttacker(research);
            IMovable.Values movable = GenMovable(research);
            return new(blueprintNum, null, research.GetLevel(), vision, killable, attacker, movable);
        }
        private static MechBlueprint UpgradeBlueprint(MechBlueprint upgrade, IResearch research, int blueprintNum)
        {
            double vision = upgrade.Vision;
            IKillable.Values killable = upgrade.Killable;
            IEnumerable<IAttacker.Values> attacker = upgrade.Attacks;
            IMovable.Values movable = upgrade.Movable;

            IKillable.Values newKillable;
            IAttacker.Values newAttacker;
            IMovable.Values newMovable;
            int hits, max, limit, damage;
            double armor, inc, range, dev;

            Type researching = research.GetType();
            int times = 1 + Game.Rand.OEInt(.39);
            for (int a = 0; a < times; a++)
            {

                switch (researching)
                {
                    case Type.MechDamage:
                        attacker = attacker.Select(a =>
                        {
                            newAttacker = GenAttacker(research).First();
                            range = a.Range;
                            if (newAttacker.Damage > a.Damage && Game.Rand.Bool())
                                range = newAttacker.Range;
                            dev = Game.Rand.Bool() ? a.Dev : newAttacker.Dev;
                            return new IAttacker.Values(newAttacker.Damage, a.ArmorPierce, a.ShieldPierce, dev, range);
                        });
                        break;
                    case Type.MechRange:
                        attacker = attacker.Select(a =>
                        {
                            newAttacker = GenAttacker(research).First();
                            damage = a.Damage;
                            if (newAttacker.Range > a.Range && Game.Rand.Bool())
                                damage = newAttacker.Damage;
                            dev = Game.Rand.Bool() ? a.Dev : newAttacker.Dev;
                            return new IAttacker.Values(damage, a.ArmorPierce, a.ShieldPierce, dev, newAttacker.Range);
                        });
                        break;
                    case Type.MechAP:
                        attacker = attacker.Select(a =>
                        {
                            newAttacker = GenAttacker(research).First();
                            damage = a.Damage;
                            if (newAttacker.ArmorPierce > a.ArmorPierce && Game.Rand.Bool())
                                damage = newAttacker.Damage;
                            dev = Game.Rand.Bool() ? a.Dev : newAttacker.Dev;
                            return new IAttacker.Values(damage, newAttacker.ArmorPierce, a.ShieldPierce, dev, a.Range);
                        });
                        break;
                    case Type.MechSP:
                        attacker = attacker.Select(a =>
                        {
                            newAttacker = GenAttacker(research).First();
                            damage = a.Damage;
                            if (newAttacker.ShieldPierce > a.ShieldPierce && Game.Rand.Bool())
                                damage = newAttacker.Damage;
                            dev = Game.Rand.Bool() ? a.Dev : newAttacker.Dev;
                            return new IAttacker.Values(damage, a.ArmorPierce, newAttacker.ShieldPierce, dev, a.Range);
                        });
                        break;
                    case Type.MechArmor:
                        newKillable = GenKillable(research);
                        hits = killable.HitsMax;
                        if (newKillable.Armor > killable.Armor && Game.Rand.Bool())
                            hits = newKillable.HitsMax;
                        killable = new IKillable.Values(hits, killable.Resilience, newKillable.Armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit);
                        break;
                    case Type.MechResilience:
                        newKillable = GenKillable(research);
                        hits = killable.HitsMax;
                        if (newKillable.Resilience > killable.Resilience && Game.Rand.Bool())
                            hits = newKillable.HitsMax;
                        armor = killable.Armor;
                        if (newKillable.Resilience > killable.Resilience && Game.Rand.Bool())
                            armor = newKillable.Armor;
                        killable = new IKillable.Values(hits, newKillable.Resilience, armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit);
                        break;
                    case Type.MechHits:
                        newKillable = GenKillable(research);
                        armor = killable.Armor;
                        if (newKillable.HitsMax > killable.HitsMax && Game.Rand.Bool())
                            armor = newKillable.Armor;
                        killable = new IKillable.Values(newKillable.HitsMax, killable.Resilience, armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit);
                        break;
                    case Type.MechShields:
                        newKillable = GenKillable(research);
                        inc = killable.ShieldInc;
                        if (newKillable.ShieldInc > inc || Game.Rand.Bool())
                            inc = newKillable.ShieldInc;
                        max = killable.ShieldMax;
                        if (newKillable.ShieldMax > max || Game.Rand.Bool())
                            max = newKillable.ShieldMax;
                        limit = killable.ShieldLimit;
                        if (newKillable.ShieldLimit > limit || Game.Rand.Bool())
                            limit = newKillable.ShieldLimit;
                        killable = new IKillable.Values(killable.HitsMax, killable.Resilience, killable.Armor, inc, max, limit);
                        break;
                    case Type.MechMove:
                        newMovable = GenMovable(research);
                        inc = movable.MoveInc;
                        if (newMovable.MoveInc > inc || Game.Rand.Bool())
                            inc = newMovable.MoveInc;
                        max = movable.MoveMax;
                        if (newMovable.MoveMax > max || Game.Rand.Bool())
                            max = newMovable.MoveMax;
                        limit = movable.MoveLimit;
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

                researching = Game.Rand.SelectValue(Enum.GetValues<Research.Type>().Where(Research.IsMech).Where(t => t != researching));
            }

            return new(blueprintNum, upgrade, research.GetLevel(), vision, killable, attacker, movable);
        }
        private static MechBlueprint CheckCost(MechBlueprint blueprint, MechBlueprint upgrade, IResearch research, int blueprintNum)
        {
            IKillable.Values killable = blueprint.Killable;
            IMovable.Values movable = blueprint.Movable;
            void Rebuild() => blueprint = new(blueprintNum, blueprint.UpgradeFrom, blueprint.ResearchLevel, blueprint.Vision, killable, blueprint.Attacks, movable);

            static double ModVal(bool increase, double amt) => (increase ? 1 : -1) * Game.Rand.DoubleFull(amt);
            bool ModMass(bool increase)
            {
                int hits = killable.HitsMax + (increase ? 1 : -1);
                if (hits < 1)
                    return false;
                killable = new IKillable.Values(hits, killable.Resilience, killable.Armor, killable.ShieldInc, killable.ShieldMax, killable.ShieldLimit);
                Rebuild();
                return true;
            };
            bool ModEnergy(bool increase)
            {
                bool ModVals(bool increase, ref double inc, ref int max, ref int limit)
                {
                    double i = inc;
                    int m = max, l = limit;
                    void ModInc()
                    {
                        i += ModVal(increase, 1 / 13.0);
                        while (increase && i >= m)
                            ModMax();
                    }
                    void ModMax()
                    {
                        m += (increase ? 1 : -1);
                        while (!increase && i >= m)
                            ModInc();
                        while (increase && m >= l)
                            ModLimit();
                    };
                    void ModLimit()
                    {
                        l += (increase ? 1 : -1);
                        while (!increase && m >= l)
                            ModMax();
                    }
                    switch (Game.Rand.Next(3))
                    {
                        case 0:
                            ModInc();
                            break;
                        case 1:
                            ModMax();
                            break;
                        case 2:
                            ModLimit();
                            break;
                    }
                    inc = i; max = m; limit = l;
                    return i >= .39 && m >= 1 && l >= 1;
                };
                if (killable.ShieldInc > 0)
                {
                    double shieldInc = killable.ShieldInc;
                    int shieldMax = killable.ShieldMax;
                    int shieldLimit = killable.ShieldLimit;
                    if (!ModVals(increase, ref shieldInc, ref shieldMax, ref shieldLimit))
                        return false;
                    killable = new IKillable.Values(killable.HitsMax, killable.Resilience, killable.Armor, shieldInc, shieldMax, shieldLimit);
                }
                else
                {
                    throw new Exception();
                    //return false;
                }
                //else
                //{
                //    double moveInc = movable.MoveInc;
                //    double moveMax = movable.MoveMax;
                //    double moveLimit = movable.MoveLimit;
                //    if (!ModVals(increase, ref moveInc, ref moveMax, ref moveLimit))
                //        return false;
                //    movable = new IMovable.Values(moveInc, moveMax, moveLimit);
                //}
                Rebuild();
                return true;
            };
            bool Mod(bool increase)
            {
                bool mass;
                if (!increase && (research.GetType() == Type.MechHits || research.GetType() == Type.Mech))
                    if (killable.ShieldInc > 0)
                        mass = false;
                    else
                        return false;
                else if (!increase && research.GetType() == Type.MechShields)
                    mass = true;
                else if (killable.ShieldInc > 0)
                    mass = Game.Rand.Bool();
                else
                    mass = true;
                return mass ? ModMass(increase) : ModEnergy(increase);
            };

            Type researching = research.GetType();
            //double minEnergy, maxEnergy, minMass, maxMass,
            int minTotal, maxTotal;
            //if (researching == Type.Mech)
            //{
            //    //minEnergy = 130;
            //    //maxEnergy = 390;
            //    //minMass = 390;
            //    //maxMass = 910;
            //    minTotal = 520;
            //    maxTotal = 1300;
            //}
            //else
            {
                minTotal = research.GetMinCost();
                maxTotal = research.GetMaxCost();
                //minEnergy = minMass = minTotal / 2.0;
                //maxEnergy = maxMass = maxTotal - minTotal;
                //if (researching == Type.MechShields || researching == Type.MechAP || researching == Type.MechRange || researching == Type.MechMove)
                //{
                //    minEnergy = minTotal;
                //    maxEnergy = 2 * maxTotal;
                //}
                //else
                //{
                //    minMass = minTotal;
                //    maxMass = 2 * maxTotal;
                //}
                if (researching != Type.Mech)
                {
                    const double minDev = .13;
                    //minEnergy = Game.Rand.GaussianCapped(minEnergy, minDev);
                    //minMass = Game.Rand.GaussianCapped(minMass, minDev);
                    minTotal = Game.Rand.GaussianCappedInt(minTotal, minDev);
                    const double maxDev = .21, maxOE = .169;
                    //maxEnergy = Game.Rand.GaussianOE(maxEnergy, maxDev, maxOE, minEnergy);
                    //maxMass = Game.Rand.GaussianOE(maxMass, maxDev, maxOE, minMass);
                    maxTotal = Game.Rand.GaussianOEInt(maxTotal, maxDev, maxOE, minTotal);
                }
            }

            //while (energy < minEnergy && ModEnergy(true)) ;
            //while (mass < minMass && ModMass(true)) ;
            //while (energy > maxEnergy && ModEnergy(false)) ;
            //while (mass > maxMass && ModMass(false)) ;
            bool canKeep = true;
            while (blueprint.TotalCost() < minTotal && (canKeep &= Mod(true))) ;
            while (blueprint.TotalCost() > maxTotal && (canKeep &= Mod(false))) ;

            if ((!canKeep || research.GetType() == Type.Mech || Game.Rand.Bool()) && (blueprint.TotalCost() < minTotal || blueprint.TotalCost() > maxTotal))
                blueprint = GenBlueprint(upgrade, research, blueprintNum);

            return blueprint;
        }
        private static double GenVision(IResearch research)
        {
            double avg = 7.8, dev = .39, oe = .091;
            ModValues(research.GetType() == Type.MechVision, 2.6, ref avg, ref dev, ref oe);
            double vision = Game.Rand.GaussianOE(avg, dev, oe, 1);
            vision *= research.GetMult(Type.MechVision, .3);
            return vision;
        }
        private static IKillable.Values GenKillable(IResearch research)
        {
            double avg = 26, dev = .26, oe = .078;
            ModValues(research.GetType() == Type.MechHits, 1.69, ref avg, ref dev, ref oe);
            int hitsMax = Game.Rand.GaussianOEInt(avg * research.GetMult(Type.MechHits, .9), dev, oe, 1);

            bool isResilience = research.GetType() == Type.MechResilience;
            double resilience = Consts.GetPct(Game.Rand.GaussianCapped(.39, .091, .169),
                Math.Pow(research.GetMult(Type.MechResilience, 1) + (isResilience ? .52 : 0), isResilience ? .5 : .2));

            double armor = 0;

            double shieldInc = 0;
            int shieldMax = 0;
            int shieldLimit = 0;

            bool isShields = research.GetType() == Type.MechShields;
            if (isShields || research.MakeType(Type.MechShields))
            {
                avg = 1.3;
                dev = .26;
                oe = .21;
                ModValues(isShields, 1.3, ref avg, ref dev, ref oe);
                shieldInc = Game.Rand.GaussianOE(avg * research.GetMult(Type.MechShields, .9), dev, oe, .39);
                shieldMax = Game.Rand.GaussianOEInt(shieldInc * 13 + 1, dev * 1.3, oe, (int)shieldInc + 1);
                shieldLimit = Game.Rand.GaussianOEInt(shieldInc * 13 + shieldMax + 1, dev * 1.3, oe * 1.3, shieldMax + 1);
            }

            bool isArmor = research.GetType() == Type.MechArmor;
            if (isArmor || research.MakeType(Type.MechArmor))
                armor = Game.Rand.Weighted(isArmor ? .95 : .75, 1 - Math.Pow(isArmor ? .52 : .65, research.GetMult(Type.MechArmor, .9)));

            return new(hitsMax, resilience, armor, shieldInc, shieldMax, shieldLimit);
        }
        private static IReadOnlyCollection<IAttacker.Values> GenAttacker(IResearch research)
        {
            int num = Game.Rand.GaussianOEInt(1.69, .26, .13, 1);
            List<IAttacker.Values> attacks = new(num);
            for (int a = 0; a < num; a++)
            {
                double avg = 5.2, dev = .26, oe = .169;
                ModValues(research.GetType() == Type.MechDamage, 2.1, ref avg, ref dev, ref oe);
                int damage = Game.Rand.GaussianOEInt(avg * research.GetMult(Type.MechDamage, 1), dev, oe, 1);
                double armorPierce = 0;
                double shieldPierce = 0;
                double randomness = Game.Rand.Weighted(.169);
                avg = 3.9;
                dev = .39;
                oe = .104;
                ModValues(research.GetType() == Type.MechRange, 2.1, ref avg, ref dev, ref oe);
                double range = Game.Rand.GaussianOE(avg, dev, oe, 1);

                bool isAP = research.GetType() == Type.MechAP;
                if (isAP || research.MakeType(Type.MechAP))
                    armorPierce = Game.Rand.Weighted(isAP ? .95 : .75, 1 - Math.Pow(isAP ? .52 : .65, research.GetMult(Type.MechAP, .6)));
                bool isSP = research.GetType() == Type.MechSP;
                if (isSP || research.MakeType(Type.MechSP))
                    shieldPierce = Game.Rand.Weighted(isSP ? .95 : .75, 1 - Math.Pow(isSP ? .52 : .65, research.GetMult(Type.MechSP, .6)));
                range *= research.GetMult(Type.MechRange, .6);

                attacks.Add(new(damage, armorPierce, shieldPierce, randomness, range));
            }
            return attacks;
        }
        private static IMovable.Values GenMovable(IResearch research)
        {
            double avg = 2.6, dev = .13, oe = .21;
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
