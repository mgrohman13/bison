using ClassLibrary1.Pieces.Behavior;
using ClassLibrary1.Pieces.Behavior.Combat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static ClassLibrary1.ResearchUpgValues;
using AttackType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.AttackType;
using DefenseType = ClassLibrary1.Pieces.Behavior.Combat.CombatTypes.DefenseType;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1.Pieces.Players
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class MechBlueprint : IComparable<MechBlueprint>, IBlueprint
    {
        public readonly MechBlueprint UpgradeFrom;
        public MechBlueprint UpgradeTo { get; private set; }

        private readonly Dictionary<MechBlueprint, MechBlueprint> _combinations = [];
        private List<MechBlueprint> _comboFrom = [];
        //private IEnumerable<MechBlueprint> _comboFrom = new List<MechBlueprint>();

        public readonly string BlueprintNum;
        public readonly int Energy;
        public readonly int Mass;
        public int ResearchLevel { get; }
        public double Vision { get; }
        public readonly double Resilience;
        public IReadOnlyList<IKillable.Values> Killable { get; }
        public IReadOnlyList<IAttacker.Values> Attacker { get; }
        public readonly IMovable.Values Movable;

        public IKillable.Values Hits => Killable.Single(d => d.Type == CombatTypes.DefenseType.Hits);
        public MechBlueprint CombinesWith(MechBlueprint other) => _combinations.GetValueOrDefault(other);
        public IEnumerable<MechBlueprint> ComboFrom => _comboFrom;

        private MechBlueprint(Game game, int blueprintNum, MechBlueprint upgrade, int research, double vision,
            IEnumerable<IKillable.Values> killable, double resilience, IEnumerable<IAttacker.Values> attacker, IMovable.Values movable)
        {
            this.BlueprintNum = "";
            if (blueprintNum > 0)
            {
                blueprintNum--;
                int num = 1 + blueprintNum / 26;
                BlueprintNum = (char)(blueprintNum % 26 + 65) + (num > 1 ? num.ToString() : "");
            }

            this.UpgradeFrom = upgrade;
            this.ResearchLevel = research;
            this.Vision = vision;
            this.Resilience = resilience;
            this.Killable = CombatTypes.OrderDef(killable);
            this.Attacker = CombatTypes.OrderAtt(attacker);
            this.Movable = movable;

            CalcCost(game, out double energy, out double mass);
            RoundCosts(game, energy, mass, out this.Energy, out this.Mass);
        }

        internal static void RoundCosts(Game game, double e, double m, out int energy, out int mass)
        {
            energy = Game.Rand.Round(e / 10.0) * 10;
            mass = Game.Rand.Round((m + (e - energy) / game.Consts.EnergyMassRatio) / 5.0) * 5;
        }

        private void CalcCost(Game game, out double energy, out double mass)
        {
            double researchMult = Research.GetResearchMult(game.Consts, ResearchLevel);
            CalcCost(game, researchMult, Vision, Killable, Resilience, Attacker, Movable, out energy, out mass);
        }
        public static void CalcCost(Game game, double researchMult, double vision, IEnumerable<IKillable.Values> killable, double resilience,
            IEnumerable<IAttacker.Values> attacker, IMovable.Values? movable, out double energy, out double mass)
        {
            Consts consts = game.Consts;
            CombatTypes combatTypes = game.CombatTypes;

            researchMult = Math.Pow(researchMult, 1 / consts.MechCostPow);
            double baseMove = consts.MoveValue(movable);
            double r = Math.Pow(Math.Pow(resilience, Math.Log(3) / Math.Log(2)) * 1.5 + 0.5, consts.CostResiliencePow);

            double AttCost(IAttacker.Values a)
            {
                double rangeMult = 1;
                if (a.Range > Attack.MELEE_RANGE)
                    rangeMult = (a.Range + Attack.MELEE_RANGE) / (Math.PI * Attack.MIN_RANGED);
                else
                    rangeMult = Math.Sqrt((baseMove + consts.CostMoveAdd) / (2 * consts.CostMoveAdd * consts.CostMoveMult));
                rangeMult = Math.Pow(rangeMult, consts.CostRangePow);
                return BaseAttCost(combatTypes, a)
                    * Math.Pow((a.Reload + .5) / (CombatTypes.ReloadAvg(a.Attack) + .5), consts.CostReloadPow)
                    * rangeMult;
            }

            double DefCost(IKillable.Values d) => Consts.StatValue(d.Defense) * combatTypes.Cost(d.Type)
                * (d.Type == DefenseType.Hits ? Math.Pow(r, consts.CostResilienceHitsPow) : 1);

            double attPow = Math.Pow(1 + (consts.CostMoveMult + baseMove) / consts.CostAttMoveDiv / consts.CostMoveMult, consts.CostAttMovePow);
            double att = MultAttCost(consts, Math.Pow(attacker.Sum(AttCost), attPow), researchMult);
            double def = killable.Sum(DefCost) / researchMult * consts.CostStatsMult;

            researchMult = Math.Sqrt(researchMult);
            double move = (baseMove + consts.CostMoveAdd) * consts.CostMoveMult / researchMult;
            double v = vision;
            v = (v + consts.CostVisionAdd) * consts.CostVisionMult / researchMult;

            double total = CalcCost(consts, att, def, move, v, r);

            //Debug.WriteLine($"total: {total}");

            double energyPct = Math.Sqrt(att / (att + def + v));
            energyPct *= move / (move + def + v);
            energyPct = Math.Sqrt(energyPct);

            double attEnergy = attacker.Sum(a => AttCost(a) * combatTypes.EnergyCostRatio(a.Type));
            double defEnergy = killable.Sum(d => DefCost(d) * combatTypes.EnergyCostRatio(d.Type));
            double attMass = attacker.Sum(a => AttCost(a) * (1 - combatTypes.EnergyCostRatio(a.Type)));
            double defMass = killable.Sum(d => DefCost(d) * (1 - combatTypes.EnergyCostRatio(d.Type)));

            energyPct *= (attEnergy + defEnergy) / (attEnergy + defEnergy + attMass + defMass);
            energyPct = Math.Sqrt(energyPct);

            energy = total * energyPct;
            mass = (total - energy) / consts.EnergyMassRatio;
        }

        private static double BaseAttCost(CombatTypes combatTypes, IAttacker.Values a) =>
            Consts.StatValue(a.Attack) * combatTypes.Cost(a.Type);
        private static double MultAttCost(Consts consts, double cost, double researchMult) =>
            cost / researchMult * consts.CostStatsMult;
        internal static double MissileCost(Game game, IAttacker.Values missile, double researchMult) =>
            CalcCost(game.Consts, MultAttCost(game.Consts, BaseAttCost(game.CombatTypes, missile), researchMult),
                game.Consts.CostStatsMult, game.Consts.CostMoveAdd * game.Consts.CostMoveMult, 0, 1);
        private static double CalcCost(Consts consts, double att, double def, double move, double vision, double resilience)
        {
            resilience = Math.Sqrt(resilience);
            return Math.Pow((att + vision) * (def * resilience + move) * resilience, consts.MechCostPow) * consts.MechCostMult;
        }

        public int TotalCost()
        {
            return Energy + Mass;
        }
        public double EnergyEquivalent(Consts consts)
        {
            return Energy + Mass * consts.EnergyMassRatio;
        }

        internal static MechBlueprint MechOneOff(IResearch research, int researchLevel)
        {
            return GenBlueprint(research.Game, null, research, research.Game.GetPieceNum(typeof(MechBlueprint)), researchLevel, false);
        }
        internal static MechBlueprint Alien(IResearch research)
        {
            return GenBlueprint(research.Game, null, research, 0, research.GetBlueprintLevel(), true);
        }
        internal static MechBlueprint OnResearch(IResearch research, SortedSet<MechBlueprint> blueprints)
        {
            Game game = research.Game;
            Consts consts = game.Consts;
            int researchLevel = research.GetBlueprintLevel();

            IEnumerable<object> select = blueprints;
            if (Game.Rand.Bool())
            {
                var existing = game.Player.PiecesOfType<Mech>().Select(m => m.Blueprint);
                if (Game.Rand.Bool())
                    select = select.Concat(existing).Append("");
                else
                    select = existing;
            }

            select = select.Select(o =>
            {
                if (o is MechBlueprint b)
                {
                    while (b.UpgradeTo != null)
                        b = b.UpgradeTo;
                    return b;
                }
                return o;
            });

            var doubles = select.Append("").ToLookup(b => b, b =>
            {
                double chance;
                if (b is MechBlueprint blueprint)
                {
                    if (blueprint.UpgradeTo != null)
                        throw new Exception();
                    chance = researchLevel - blueprint.ResearchLevel;
                    if (chance < 0)
                        chance = 0;
                }
                else
                {
                    chance = consts.ResearchFactor;
                }
                return (chance * chance);
            });

            double mult = 1;
            double sum = doubles.Sum(p => p.Sum());
            double max = int.MaxValue - 13 * doubles.Count;
            if (sum > max)
                mult = max / sum;
            var ints = doubles.ToDictionary(p => p.Key, p => Game.Rand.Round(p.Sum() * mult));

            if (research.GetType() == Type.MechUpgrades)
                ints[""] = 0;
            MechBlueprint upgrade = Game.Rand.SelectValue(ints) as MechBlueprint;

            MechBlueprint newBlueprint = GenBlueprint(game, upgrade, research, game.GetPieceNum(typeof(MechBlueprint)), researchLevel, false);
            blueprints.Add(newBlueprint);
            if (upgrade != null)
            {
                upgrade.UpgradeTo = newBlueprint;
                blueprints.Remove(upgrade);
            }
            return newBlueprint;
        }
        private static MechBlueprint GenBlueprint(Game game, MechBlueprint upgrade, IResearch research, int blueprintNum, int researchLevel, bool alien)
        {
            Consts consts = game.Consts;
            MechBlueprint blueprint;

            bool valid = false;
            do
            {
                if (upgrade == null)
                    blueprint = CheckCost(game, NewBlueprint(game, research, blueprintNum, researchLevel, alien),
                        upgrade, research, blueprintNum, researchLevel, alien);
                else do
                    blueprint = CheckCost(game, UpgradeBlueprint(game, upgrade, research, blueprintNum, researchLevel),
                        upgrade, research, blueprintNum, researchLevel, alien);
                while (!UpgradeValid(consts, blueprint, upgrade, research));

                valid = research.GetType() switch
                {
                    Type.Mech => blueprint.Attacker.Single().Attack < blueprint.Killable.Single().Defense,
                    Type.MechEnergyWeapons => blueprint.Attacker.Any(a => a.Type == AttackType.Energy && a.Range == Attack.MELEE_RANGE),
                    Type.MechShields => blueprint.Killable.Any(k => k.Type == DefenseType.Shield),
                    //Type.MechResilience =>,
                    //Type.MechVision =>,
                    Type.MechAttack => blueprint.Attacker.Sum(a => Consts.StatValue(a.Attack)) * 2.1 >= blueprint.Killable.Max(k => Consts.StatValue(k.Defense)),
                    Type.MechDefense => blueprint.Attacker.Sum(a => Consts.StatValue(a.Attack)) * 2.1 <= blueprint.Killable.Max(k => Consts.StatValue(k.Defense)),
                    Type.MechLasers => blueprint.Attacker.Any(a => a.Type == AttackType.Energy && a.Range > Attack.MELEE_RANGE),
                    //Type.MechMove =>,
                    Type.MechRange => blueprint.Attacker.Any(a => a.Range > Attack.MELEE_RANGE),
                    Type.MechArmor => blueprint.Killable.Any(k => k.Type == DefenseType.Armor),
                    Type.MechExplosives => blueprint.Attacker.Any(a => a.Type == AttackType.Explosive),
                    _ => true,
                };
                valid &= blueprint.Hits.Defense > 1;
            }
            while (!valid);

            return blueprint;
        }

        private static MechBlueprint NewBlueprint(Game game, IResearch research, int blueprintNum, int researchLevel, bool alien)
        {
            double vision = alien ? 0 : GenVision(research);
            double resilience = GenResilience(research);
            IReadOnlyList<IKillable.Values> killable = GenKillable(research);
            IReadOnlyList<IAttacker.Values> attacker = GenAttacker(research);
            IMovable.Values movable = GenMovable(game.Consts, research, killable, attacker);
            return new(game, blueprintNum, null, researchLevel, vision, killable, resilience, attacker, movable);
        }

        internal static MechBlueprint Combine(Game game, MechBlueprint bp1, MechBlueprint bp2, int researchLevel, double costMult)
        {
            MechBlueprint existing = bp1.CombinesWith(bp2);
            if (existing != null)
                return existing;

            Consts consts = game.Consts;
            int blueprintNum = game.GetPieceNum(typeof(MechBlueprint));

            double Min(double v1, double v2) => (v1 + v2 + Math.Min(v1, v2)) / 3;
            double Max(double v1, double v2) => Math.Sqrt(v1 * v1 + v2 * v2);

            static double GetStr(IAttacker.Values values) => Consts.StatValue(values.Attack);
            static double GetRange(IAttacker.Values values) => values.Range;
            static double GetReload(IAttacker.Values values) => values.Reload;
            static double GetAtt(MechBlueprint bp, AttackType type, bool ranged, Func<IAttacker.Values, double> GetStat, double defaultValue = 0) =>
                bp.Attacker.Where(k => k.Type == type && ranged == k.Range > Attack.MELEE_RANGE)
                    .Sum(a => (double?)GetStat(a)) ?? defaultValue;
            static double GetDef(MechBlueprint bp, DefenseType type) =>
                bp.Killable.Where(k => k.Type == type).Select(k => Consts.StatValue(k.Defense)).SingleOrDefault(0);
            Tuple<double, double> GetStatRange(Func<MechBlueprint, double> GetStat)
            {
                double stat1 = GetStat(bp1);
                double stat2 = GetStat(bp2);
                return Tuple.Create(Min(stat1, stat2), Max(stat1, stat2));
            }

            const int countStr = 9, countReload = 6, countRange = 3;
            Tuple<double, double> temp;
            double[] minStr = new double[countStr], maxStr = new double[countStr];
            double[] minReload = new double[countReload], maxReload = new double[countReload];
            double[] minRange = new double[countRange], maxRange = new double[countRange];

            temp = GetStatRange(bp => GetDef(bp, DefenseType.Hits));
            minStr[0] = temp.Item1;
            maxStr[0] = temp.Item2;
            temp = GetStatRange(bp => GetDef(bp, DefenseType.Shield));
            minStr[1] = temp.Item1;
            maxStr[1] = temp.Item2;
            temp = GetStatRange(bp => GetDef(bp, DefenseType.Armor));
            minStr[2] = temp.Item1;
            maxStr[2] = temp.Item2;

            for (int a = 0; a < countReload; a++)
            {
                AttackType type = a switch
                {
                    >= 0 and <= 1 => AttackType.Kinetic,
                    >= 2 and <= 3 => AttackType.Energy,
                    >= 4 and <= 5 => AttackType.Explosive,
                    _ => throw new Exception(),
                };
                bool range = a % 2 == 1;

                temp = GetStatRange(bp => GetAtt(bp, type, range, GetStr));
                minStr[3 + a] = temp.Item1;
                maxStr[3 + a] = temp.Item2;

                if (maxStr[3 + a] > 0)
                {
                    temp = GetStatRange(bp => GetAtt(bp, type, range, GetReload));
                    minReload[a] = temp.Item1;
                    maxReload[a] = temp.Item2;

                    if (range)
                    {
                        temp = GetStatRange(bp => GetAtt(bp, type, range, GetRange, Attack.MIN_RANGED));
                        minRange[a / 2] = temp.Item1;
                        maxRange[a / 2] = temp.Item2;
                    }
                }
                else
                    ;
            }

            for (int a = 0; a < countStr; a++)
                maxStr[a] *= Math.Sqrt(costMult);

            double minVision = Min(bp1.Vision, bp2.Vision);
            double maxVision = Math.Max(bp1.Vision, bp2.Vision);
            double minResilience = Math.Min(bp1.Resilience, bp2.Resilience);
            double maxResilience = Math.Max(bp1.Resilience, bp2.Resilience);

            double minMoveInc = Min(bp1.Movable.MoveInc, bp2.Movable.MoveInc);
            double maxMoveInc = Math.Max(bp1.Movable.MoveInc, bp2.Movable.MoveInc);
            double minMoveMax = Min(bp1.Movable.MoveMax, bp2.Movable.MoveMax);
            double maxMoveMax = Math.Max(bp1.Movable.MoveMax, bp2.Movable.MoveMax);
            double minMoveLimit = Min(bp1.Movable.MoveLimit, bp2.Movable.MoveLimit);
            double maxMoveLimit = Math.Max(bp1.Movable.MoveLimit, bp2.Movable.MoveLimit);

            double target = (bp1.EnergyEquivalent(consts) + bp2.EnergyEquivalent(consts)) * costMult;
            double tolerance = 0;

            while (true)
            {
                int[] valStr = new int[countStr];
                for (int a = 0; a < countStr; a++)
                    valStr[a] = Game.Rand.Round(Consts.StatValueInverse(Game.Rand.Range(minStr[a], maxStr[a])));
                int[] valReload = new int[countReload];
                for (int a = 0; a < countReload; a++)
                    valReload[a] = Math.Min(Game.Rand.Round(Game.Rand.Range(minReload[a], maxReload[a])), valStr[a + 3]);
                int[] valRange = new int[countRange];
                for (int a = 0; a < countRange; a++)
                    valRange[a] = Game.Rand.Round(Game.Rand.Range(minRange[a], maxRange[a]));

                double vision = Game.Rand.Range(minVision, maxVision);
                double resilience = Game.Rand.Range(minResilience, maxResilience);
                double moveInc = Game.Rand.Range(minMoveInc, maxMoveInc);
                int moveMax = Math.Max(Game.Rand.Round(Game.Rand.Range(minMoveMax, maxMoveMax)), (int)Math.Ceiling(moveInc) + 1);
                int moveLimit = Math.Max(Game.Rand.Round(Game.Rand.Range(minMoveLimit, maxMoveLimit)), moveMax + 1);

                List<IKillable.Values> killable = [new(DefenseType.Hits, valStr[0])];
                if (valStr[1] > 0)
                    killable.Add(new(DefenseType.Shield, valStr[1]));
                if (valStr[2] > 0)
                    killable.Add(new(DefenseType.Armor, valStr[2]));

                List<IAttacker.Values> attacker = [];
                if (valStr[3] > 0)
                    attacker.Add(new(AttackType.Kinetic, valStr[3], Attack.MELEE_RANGE, Math.Max(1, valReload[0])));
                if (valStr[4] > 0)
                    attacker.Add(new(AttackType.Kinetic, valStr[4], valRange[0], Math.Max(1, valReload[1])));
                if (valStr[5] > 0)
                    attacker.Add(new(AttackType.Energy, valStr[5], Attack.MELEE_RANGE, Math.Max(1, valReload[2])));
                if (valStr[6] > 0)
                    attacker.Add(new(AttackType.Energy, valStr[6], valRange[1], Math.Max(1, valReload[3])));
                if (valStr[7] > 0)
                    attacker.Add(new(AttackType.Explosive, valStr[7], Attack.MELEE_RANGE, Math.Max(1, valReload[4])));
                if (valStr[8] > 0)
                    attacker.Add(new(AttackType.Explosive, valStr[8], valRange[2], Math.Max(1, valReload[5])));

                IMovable.Values movable = new(moveInc, moveMax, moveLimit);
                MechBlueprint result = new(game, blueprintNum, null, researchLevel, vision,
                    Game.Rand.Iterate(killable), resilience, Game.Rand.Iterate(attacker), movable);
                double cost = result.EnergyEquivalent(consts);

                tolerance += Game.Rand.NextDouble();
                if (Math.Abs(cost - target) < tolerance)
                {
                    bp1._combinations.Add(bp2, result);
                    if (bp1 != bp2)
                        bp2._combinations.Add(bp1, result);
                    result._comboFrom = [.. new MechBlueprint[] { bp1, bp2 }.Order()];
                    return result;
                }

                for (int a = 0; a < countStr; a++)
                    valStr[a] = Game.Rand.Round(Consts.StatValue(valStr[a]));

                if (cost < target)
                {
                    for (int a = 0; a < countStr; a++)
                    {
                        minStr[a] = Game.Rand.Range(minStr[a], valStr[a]);
                        if (maxStr[a] > 0)
                            maxStr[a] += Game.Rand.Range(1, Consts.StatValueInverse(valStr[a]));
                    }
                    for (int a = 0; a < countReload; a++)
                    {
                        minReload[a] = Game.Rand.Range(minReload[a], valReload[a]);
                        if (maxReload[a] > 0)
                            maxReload[a] += Game.Rand.DoubleHalf(1 / (1 + valReload[a]));
                    }
                    for (int a = 0; a < countRange; a++)
                    {
                        minRange[a] = Game.Rand.Range(minRange[a], valRange[a]);
                        if (maxRange[a] > 0)
                            maxRange[a] += Game.Rand.DoubleFull();
                    }

                    minVision = Game.Rand.Range(minVision, vision);
                    minResilience = Game.Rand.Range(minResilience, resilience);
                    minMoveInc = Game.Rand.Range(minMoveInc, moveInc);
                    minMoveMax = Game.Rand.Range(minMoveMax, moveMax);
                    minMoveLimit = Game.Rand.Range(minMoveLimit, moveLimit);

                    if (Game.Rand.Bool())
                    {
                        maxVision += Game.Rand.DoubleFull();
                        maxResilience = Math.Pow(maxResilience, Game.Rand.Range(.91, 1));
                        if (Game.Rand.Bool())
                        {
                            maxMoveInc += Game.Rand.DoubleHalf();
                            maxMoveMax++;
                            maxMoveLimit += Game.Rand.RangeInt(1, 3);
                        }
                    }
                }
                else
                {
                    for (int a = 0; a < countStr; a++)
                        maxStr[a] = Game.Rand.Range(maxStr[a], valStr[a]);
                    for (int a = 0; a < countReload; a++)
                        maxReload[a] = Game.Rand.Range(maxReload[a], valReload[a]);
                    for (int a = 0; a < countRange; a++)
                        maxRange[a] = Game.Rand.Range(maxRange[a], valRange[a]);

                    maxVision = Game.Rand.Range(maxVision, vision);
                    maxResilience = Game.Rand.Range(maxResilience, resilience);
                    maxMoveInc = Game.Rand.Range(maxMoveInc, moveInc);
                    maxMoveMax = Game.Rand.Range(maxMoveMax, moveMax);
                    maxMoveLimit = Game.Rand.Range(maxMoveLimit, moveLimit);
                }
            }
        }

        private static MechBlueprint UpgradeBlueprint(Game game, MechBlueprint upgrade, IResearch research, int blueprintNum, int researchLevel)
        {
            Consts consts = game.Consts;
            CombatTypes combatTypes = game.CombatTypes;

            double resilience = upgrade.Resilience;
            double vision = upgrade.Vision;
            List<IKillable.Values> killable = [.. upgrade.Killable];
            List<IAttacker.Values> attacker = [.. upgrade.Attacker];
            IMovable.Values movable = upgrade.Movable;

            Type upgType = research.GetType();
            HashSet<Type> done = [];
            int times = 1 + Game.Rand.OEInt();
            for (int a = 0; a < times; a++)
            {
                switch (upgType)
                {
                    case Type.MechAttack:
                        attacker = [.. Game.Rand.Iterate(attacker).Select(attack =>
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
                            return UpgAttack(combatTypes, attacker, attack, type, att, range);
                        })];
                        break;
                    case Type.MechRange:
                        attacker = [.. Game.Rand.Iterate(attacker).Select(attack =>
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
                            return UpgAttack(combatTypes,attacker, attack, type, att, range);
                        })];
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
                            killable = [.. Game.Rand.Iterate(killable).Select(defense =>
                            {
                                IKillable.Values newKillable = GenKillable(research).Where(k => k.Type == DefenseType.Hits).Single();
                                int def = defense.Defense;
                                if (defense.Type == DefenseType.Hits)
                                    def = newKillable.Defense;
                                return new IKillable.Values(defense.Type, def);
                            })];
                        break;
                    case Type.MechDefense:
                        killable = [.. Game.Rand.Iterate(killable).Select(defense =>
                        {
                            int def = defense.Defense;
                            IKillable.Values newKillable = Game.Rand.SelectValue(GenKillable(research).Where(k => k.Type == DefenseType.Hits || k.Type == defense.Type));
                            if (Game.Rand.Next(newKillable.Defense + 1) >= Game.Rand.Next(def + 1))
                                def = newKillable.Defense;
                            return new IKillable.Values(defense.Type, def);
                        })];
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
                        IMovable.Values newMovable = GenMovable(consts, research, killable, attacker);
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
                            newMovable = GenMovable(consts, research, killable, attacker);
                            movable = new IMovable.Values(newMovable);
                        }
                        break;
                    case Type.CombineMechs:
                        times += Game.Rand.Next(6);
                        break;
                    default:
                        throw new Exception();
                }
                if (!done.Add(upgType))
                    ;
                upgType = Game.Rand.SelectValue(Enum.GetValues<Type>().Where(Research.IsMech)
                    .Concat([Type.MechMove]) //more likely to pick
                    .Where(t => !done.Contains(t) || Game.Rand.Next(13) == 0) //small chance of picking the same type again
                    .Concat([Type.MechResilience, Type.MechVision])); //can pick multiple times
            }
            return new(game, blueprintNum, upgrade, researchLevel, vision, killable, resilience, attacker, movable);

            void UpgAttackType(AttackType upgAtt)
            {
                attacker = [.. Game.Rand.Iterate(attacker).Select(attack =>
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
                    return UpgAttack(combatTypes, attacker, attack, type, att, range);
                })];

                double trgAtts = NumAtts(research);
                int numAttacks = attacker.Count;
                if (CheckNumAtts())
                    AddAttack(combatTypes, attacker, GenAtt());
                numAttacks = attacker.Count;
                attacker = [.. Game.Rand.Iterate(attacker).Where(a =>
                {
                    bool keep = a.Type == upgAtt || CheckNumAtts();
                    if (!keep)
                        numAttacks--;
                    return keep;
                })];
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
                killable = [.. Game.Rand.Iterate(killable).Select(defense =>
                {
                    int def = defense.Defense;
                    IKillable.Values newKillable = GenDef();
                    if (defense.Type == newKillable.Type || Game.Rand.Bool())
                        def = newKillable.Defense;
                    defense = new IKillable.Values(defense.Type, def);
                    return defense;
                })];

                IKillable.Values addKillable = GenDef();
                if (!killable.Any(k => k.Type == addKillable.Type))
                    killable = [.. killable, .. new[] { addKillable }];

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

        private static bool UpgradeValid(Consts consts, MechBlueprint blueprint, MechBlueprint upgrade, IResearch research)
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
                Type.MechMove => b => Consts.StatValue(consts.MoveValue(b.Movable)),
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

        private static MechBlueprint CheckCost(Game game, MechBlueprint blueprint, MechBlueprint upgrade, IResearch research, int blueprintNum, int researchLevel, bool alien)
        {
            Type researching = research.GetType();
            int minTotal, maxTotal;
            {
                minTotal = research.GetMinCost();
                maxTotal = research.GetMaxCost();
                if (upgrade != null)
                {
                    //avg??
                    minTotal = Math.Max(minTotal, Game.Rand.Round(upgrade.TotalCost() * 0.65));
                    maxTotal = Math.Min(maxTotal, Game.Rand.Round(upgrade.TotalCost() * 1.69));
                }
                if (researching != Type.Mech)
                {
                    const double dev = .26, oe = .091;
                    minTotal = Game.Rand.GaussianOEInt(minTotal, dev, oe, 520);
                    if (maxTotal > minTotal)
                        maxTotal = Game.Rand.GaussianOEInt(maxTotal, dev, oe, minTotal);
                    else
                        maxTotal = Game.Rand.Round(minTotal * Game.Rand.Range(1, 1 + Game.Rand.Weighted(.13)));
                }
            }

            int oldCost = blueprint.TotalCost();
            bool canKeep = true;
            while (blueprint.TotalCost() < minTotal && (canKeep &= ModStat(true))) ;
            while (blueprint.TotalCost() > maxTotal && (canKeep &= ModStat(false))) ;

            int newCost = blueprint.TotalCost();
            if (oldCost != newCost)
                Debug.WriteLine($"blueprint ({(blueprint.BlueprintNum == "" ? "Alien" : blueprint.BlueprintNum)}) {oldCost} -> {newCost}");

            if ((!canKeep || research.GetType() == Type.Mech || Game.Rand.Bool()) && (newCost < minTotal || newCost > maxTotal))
                blueprint = GenBlueprint(game, upgrade, research, blueprintNum, researchLevel, alien);

            return blueprint;

            bool ModStat(bool increase)
            {
                Debug.WriteLine($"ModStat: {blueprint.TotalCost()} ({minTotal}-{maxTotal})");

                int inc = 1;

                //variables for relative chances
                double vision = blueprint.Vision;
                double resilience = blueprint.Resilience;
                double moveInc = blueprint.Movable.MoveInc;
                double moveMax = blueprint.Movable.MoveMax;
                double moveLimit = blueprint.Movable.MoveLimit;
                double[] def = [.. blueprint.Killable.Select(k => (double)k.Defense)];
                double[] att = [.. blueprint.Attacker.Select(a => (double)a.Attack)];
                double[] reload = [.. blueprint.Attacker.Select(a => (double)a.Reload)];
                double[] range = [.. blueprint.Attacker.Select(a => (double)a.Range)];

                if (increase)
                {
                    //boundary conditions
                    if (moveInc + inc >= moveMax)
                        moveInc = 0;
                    if (moveMax + inc >= moveLimit)
                        moveMax = 0;
                    for (int a = 0; a < att.Length; a++)
                    {
                        if (reload[a] + inc >= att[a])
                            reload[a] = 0;
                        if (range[a] == Attack.MELEE_RANGE)
                            range[a] = 0;
                    }
                }
                else
                {
                    //boundary conditions
                    if (vision - inc <= 1)
                        vision = 0;
                    if (moveInc - inc <= 1)
                        moveInc = 0;
                    if (moveMax - inc <= moveInc)
                        moveMax = 0;
                    if (moveLimit - inc <= moveMax)
                        moveLimit = 0;
                    for (int b = 0; b < def.Length; b++)
                        if (def[b] - inc <= 1)
                            def[b] = 0;
                    for (int c = 0; c < att.Length; c++)
                    {
                        if (att[c] - inc < reload[c]) //allow dropping to 1
                            att[c] = 0;
                        if (reload[c] - inc < 1) //allow dropping to 1
                            reload[c] = 0;
                        if (range[c] - inc <= Attack.MIN_RANGED * 2) //buffer to allow inc by up to MIN_RANGED
                            range[c] = 0;
                    }

                    //offsets
                    vision -= 1;
                    moveInc -= 1;
                    moveMax -= 2;
                    moveLimit -= 3;
                    def = [.. def.Select(d => d - 2)];
                    att = [.. att.Select(a => a - 1)];
                    reload = [.. reload.Select(r => r - 1)];
                    range = [.. range.Select(r => r - Attack.MIN_RANGED)];
                }

                //weight multipliers 
                const double resilienceMult = 16.9, rangeMult = .39;
                resilience *= resilienceMult; //resilienceMult used in inc
                reload = [.. reload.Select(r => r * (increase ? 3.9 : 6.5))]; //inc is unaffected
                range = [.. range.Select(r => r * rangeMult)]; //inc is also higher 
                moveMax /= 2; //inc is unaffected
                moveLimit /= 3; //inc is unaffected

                int GetChance(double value)
                {
                    if (value < 0)
                        value = 0;
                    else if (!increase)
                        value *= value; //if decreasing, favor extreme values
                    return Game.Rand.Round(value);
                }

                double newVision = blueprint.Vision, newResilience = blueprint.Resilience;
                var newKillable = blueprint.Killable.ToArray();
                var newAttacker = blueprint.Attacker.ToArray();
                var newMovable = blueprint.Movable;

                if (!increase)
                    inc *= -1;

                void IncVision() => newVision += inc;
                void IncResilience()
                {
                    if (increase)
                        newResilience = 1 - newResilience;
                    newResilience -= newResilience / resilienceMult;
                    if (increase)
                        newResilience = 1 - newResilience;
                }
                void IncMoveInc() => IncMovable(inc, 0, 0);
                void IncMoveMax() => IncMovable(0, inc, 0);
                void IncMoveLimit() => IncMovable(0, 0, inc);
                void IncMovable(int moveInc, int moveMax, int moveLimit) =>
                    newMovable = new(blueprint.Movable.MoveInc + moveInc, blueprint.Movable.MoveMax + moveMax, blueprint.Movable.MoveLimit + moveLimit);

                Dictionary<Action, int> chances = new() {
                    { IncVision, GetChance(vision) },
                    { IncResilience, GetChance(resilience) },
                    { IncMoveInc, GetChance(moveInc) },
                    { IncMoveMax, GetChance(moveMax) },
                    { IncMoveLimit, GetChance(moveLimit) },
                };

                for (int d = 0; d < blueprint.Killable.Count; d++)
                {
                    int e = d; //capture loop variable
                    chances.Add(() =>
                        newKillable[e] = new(newKillable[e].Type, newKillable[e].Defense + inc),
                        GetChance(def[e]));
                }
                for (int f = 0; f < blueprint.Attacker.Count; f++)
                {
                    int g = f; //capture loop variable
                    chances.Add(() =>
                        IncAttacker(inc, 0, 0),
                        GetChance(att[g]));
                    chances.Add(() =>
                        IncAttacker(0, inc * Game.Rand.Range(1, Attack.MIN_RANGED), 0), //equivalent to 0.350116223 rangeMult
                        GetChance(range[g]));
                    chances.Add(() =>
                        IncAttacker(0, 0, inc),
                        GetChance(reload[g]));
                    void IncAttacker(int incAtt, double incRange, int incReload) =>
                        newAttacker[g] = new(newAttacker[g].Type, newAttacker[g].Attack + incAtt, newAttacker[g].Range + incRange, newAttacker[g].Reload + incReload);
                }

                Action Inc = Game.Rand.SelectValue(chances);
                Inc();

                blueprint = new(game, blueprintNum, blueprint.UpgradeFrom, blueprint.ResearchLevel, newVision, newKillable, newResilience, newAttacker, newMovable);
                return true;
            }
        }
        private static IAttacker.Values UpgAttack(CombatTypes combatTypes, List<IAttacker.Values> attacker, IAttacker.Values oldAttack,
            AttackType type, int att, double range)
        {
            bool rangeChange = (oldAttack.Range > Attack.MELEE_RANGE) != (range > Attack.MELEE_RANGE);
            if (oldAttack.Type != type || rangeChange)
            {
                int matches = attacker.Append(oldAttack).Count(a => a.Type == type && ((a.Range > Attack.MELEE_RANGE) == (range > Attack.MELEE_RANGE)));
                if (matches == 1)
                {
                    type = oldAttack.Type;
                    if (rangeChange)
                        range = oldAttack.Range;
                }
            }
            if (oldAttack.Attack == att)
                return new(type, att, range, oldAttack.Reload);
            return new(combatTypes, type, att, range);
        }

        private static double GenVision(IResearch research)
        {
            const double avgVision = 5.25;
            double avg = avgVision, dev = .39, oe = .169;
            bool isVision = research.GetType() == Type.MechVision;
            ModValues(isVision, 1.7, ref avg, ref dev, ref oe);
            avg *= research.GetMult(Type.MechVision, Blueprint_Vision_Pow);
            if (isVision)
                avg += 1.3;
            return Game.Rand.GaussianOE(avg, dev, oe, isVision ? Game.Rand.Round(Math.Sqrt(avg) + avgVision) : 1);
        }
        private static double GenResilience(IResearch research)
        {
            bool isResilience = research.GetType() == Type.MechResilience;
            return GenResilience(isResilience ? .5 : .39, isResilience ? .13 : .26,
                Math.Pow(research.GetMult(Type.MechResilience, 1) + (isResilience ? .52 : 0),
                    isResilience ? .5 : .2));
        }
        public static double GenResilience(double avg, double dev, double pow)
        {
            double weightPct = dev / Math.PI;
            double max = 1 - 2 * avg;
            double weight = avg * weightPct / max;
            avg *= 1 - weightPct;
            double w = 0;
            if (max > 0)
                Game.Rand.Weighted(max, weight);
            return Consts.GetPct(Game.Rand.GaussianCapped(avg, dev) + w, pow);
        }
        private static ReadOnlyCollection<IKillable.Values> GenKillable(IResearch research)
        {
            bool shields = research.GetType() == Type.MechShields || research.MakeType(Type.MechShields);
            bool armor = research.GetType() == Type.MechArmor || research.MakeType(Type.MechArmor);
            if (shields && armor && Game.Rand.Bool())
                if (Game.Rand.Bool())
                    shields = false;
                else
                    armor = false;

            double hitsMult = 1.04;
            if (shields && Game.Rand.Bool())
                hitsMult *= .91;
            if (armor && Game.Rand.Bool())
                hitsMult *= .65;
            if (!shields && !armor
                    && ((research.HasType(Type.MechShields) && Game.Rand.Bool())
                    || (research.HasType(Type.MechArmor) && Game.Rand.Bool())))
                hitsMult *= 1.3;

            IKillable.Values hits = GenType(DefenseType.Hits, null, hitsMult);
            List<IKillable.Values> defenses = [hits];
            if (shields)
                defenses.Add(GenType(DefenseType.Shield, Type.MechShields, armor && Game.Rand.Bool() ? .52 : .78));
            if (armor)
                defenses.Add(GenType(DefenseType.Armor, Type.MechArmor, 1.17));
            return defenses.AsReadOnly();

            IKillable.Values GenType(DefenseType type, Type? additionalResearch, double mult)
            {
                double avg = 5.2, dev = .26, oe = .078;
                ModValues(research.GetType() == Type.MechDefense || research.GetType() == additionalResearch, 1.4, ref avg, ref dev, ref oe);

                double researchMult = research.GetMult(Type.MechDefense, Blueprint_Defense_Pow);
                if (additionalResearch.HasValue)
                    researchMult = Math.Sqrt(researchMult) * research.GetMult(additionalResearch.Value, Blueprint_Defense_Pow / 2.0);

                int defense = Game.Rand.GaussianOEInt(2.6 + avg * mult * researchMult, dev, oe, 1);
                return new(type, defense);
            }
        }
        private static double NumAtts(IResearch research) => 1.13 * research.GetMult(Type.MechAttack, Blueprint_Attacks_Count_Pow);

        private static ReadOnlyCollection<IAttacker.Values> GenAttacker(IResearch research)
        {
            CombatTypes combatTypes = research.Game.CombatTypes;

            int numAttacks = research.GetType() == Type.Mech ? 1 : Game.Rand.GaussianOEInt(NumAtts(research), .26, .13, 1);
            if (numAttacks < 1)
                numAttacks = 1;
            List<IAttacker.Values> attacks = new(numAttacks);

            HashSet<AttackType> used = [];
            bool usedRange = false;

            for (int a = 0; a < numAttacks; a++)
            {
                Type researchType = a > 0 ? Type.Mech : research.GetType();

                AttackType type = GetAttackType(researchType, out bool isLaser);
                bool ranged = IsRanged(researchType, isLaser, ref type);
                double range = GetRange(researchType, ranged, out double rangeAvg);

                HashSet<Type> apply = GetResearchTypes(type, ranged);

                //modify for current research type
                double attAvg = 3.9, dev = .21, oe = .052;
                foreach (var item in apply)//Game.Rand.Iterate(apply))
                    ModValues(researchType == item, 1.3, ref attAvg, ref dev, ref oe);

                //modify for research totals
                double researchMult = 1;
                foreach (var item in apply)
                    researchMult *= research.GetMult(item, Blueprint_Attack_Pow);
                researchMult = Math.Pow(researchMult, 1.0 / apply.Count);
                attAvg *= researchMult;

                //modify for attack type
                attAvg *= combatTypes.GetDamageMult(type);

                //modify for multiple attacks and range
                attAvg = 1 + (attAvg - 1) * Math.Sqrt(rangeAvg / range / numAttacks);

                int cap = 1;
                if (researchType == Type.MechAttack || researchType == Type.MechEnergyWeapons || researchType == Type.MechExplosives
                    || researchType == Type.MechLasers || researchType == Type.MechRange)
                {
                    int rangedAtt = Game.Rand.RangeInt(Game.Rand.RangeInt(0, 2), Game.Rand.RangeInt(4, 6));
                    attAvg += rangedAtt;
                    cap += rangedAtt;
                }

                if (research is EnemyResearch)
                {
                    int enemyAtt = Game.Rand.RangeInt(Game.Rand.RangeInt(0, 2), Game.Rand.RangeInt(2, 4));
                    attAvg += enemyAtt;
                    cap += enemyAtt;
                }

                int attack = cap;
                if (attAvg > cap)
                    attack = Game.Rand.GaussianOEInt(attAvg, dev, oe, cap);

                if (range > Attack.MELEE_RANGE)
                {
                    double mult = Game.Rand.GaussianOEInt(1.69, .078, .021, 1);
                    while (attack > Game.Rand.GaussianCappedInt(mult + range / mult, .26, 1))
                    {
                        attack--;
                        range += Game.Rand.DoubleFull(mult);
                    }
                }

                AddAttack(combatTypes, attacks, new(combatTypes, type, attack, range));
            }
            return attacks.AsReadOnly();

            AttackType GetAttackType(Type researchType, out bool isLaser)
            {
                bool explosive = research.MakeType(Type.MechExplosives);
                bool MakeType(Type t) => research.MakeType(t) && !explosive;

                isLaser = researchType == Type.MechLasers || MakeType(Type.MechLasers);

                AttackType type = AttackType.Kinetic;
                if (researchType == Type.MechExplosives)
                    type = AttackType.Explosive;
                else if (isLaser || researchType == Type.MechEnergyWeapons || MakeType(Type.MechEnergyWeapons))
                    type = AttackType.Energy;
                else if (explosive)
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
                if (ranged && usedRange && researchType != Type.MechLasers && type != AttackType.Explosive && Game.Rand.Bool())
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
                    rangeAvg += 5.2;
                    double dev = .39, oe = .39;
                    ModValues(researchType == Type.MechRange, 1.6, ref rangeAvg, ref dev, ref oe);
                    rangeAvg *= research.GetMult(Type.MechRange, Blueprint_Range_Pow);
                    rangeAvg += 6.5;
                    oe /= Math.Sqrt(rangeAvg);
                    range = Game.Rand.GaussianOE(rangeAvg, dev, oe, Attack.MIN_RANGED);
                }
                return range;
            }
            static HashSet<Type> GetResearchTypes(AttackType type, bool ranged)
            {
                HashSet<Type> apply = [Type.MechAttack];
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
        private static void AddAttack(CombatTypes combatTypes, List<IAttacker.Values> attacker, IAttacker.Values values)
        {
            AttackType type = values.Type;
            double range = values.Range;
            int existing = attacker.FindIndex(a => a.Type == type && ((a.Range > Attack.MELEE_RANGE) == (range > Attack.MELEE_RANGE)));
            if (existing >= 0)
            {
                var other = attacker[existing];
                int attack = Game.Rand.Round(Consts.StatValueInverse(Consts.StatValue(values.Attack) + Consts.StatValue(other.Attack)));
                range = (range + other.Range) / 2;
                attacker[existing] = new(combatTypes, type, attack, range);
            }
            else
            {
                attacker.Add(values);
            }
        }

        private static IMovable.Values GenMovable(Consts consts, IResearch research, IEnumerable<IKillable.Values> killable, IEnumerable<IAttacker.Values> attacker)
        {
            double avg = 6.00, dev = .169, oe = .13;

            double researchMult = research.GetMult(Type.MechMove, 1);
            const double lowPenalty = 1.40;
            if (researchMult < lowPenalty)
                avg *= researchMult / lowPenalty;

            ModValues(research.GetType() == Type.MechMove, 1.3, ref avg, ref dev, ref oe);

            avg *= research.GetMult(Type.MechMove, Blueprint_Move_Pow);
            avg += 0.91;

            oe /= Math.Sqrt(avg);
            avg++;
            double cap = Game.Rand.Range(Game.Rand.Range(1, Math.Sqrt(2) + 1), Math.Sqrt(2) * 2 + Game.Rand.DoubleHalf());
            while (avg < cap)
                cap -= Game.Rand.NextDouble();
            double move = Game.Rand.GaussianOE(avg, dev, oe, cap);
            int MinMax() => (int)Math.Ceiling(move) + 1;
            int max = Game.Rand.GaussianOEInt(1 + move * 2, dev * 2.6, oe * 1.3, MinMax());
            int limit = Game.Rand.GaussianOEInt(1 + move + max, dev * 2.6, oe * 2.6, max + (int)move);

            IMovable.Values movable = new(move, max, limit);

            double att = attacker.Sum(a => Consts.StatValue(a.Attack));
            double def = killable.Sum(k => Consts.StatValue(k.Defense));
            while (Game.Rand.DoubleFull(consts.MoveValue(movable)) > Game.Rand.DoubleHalf(att) + Game.Rand.DoubleHalf(def))
            {
                move -= Game.Rand.DoubleHalf();
                if (move < cap)
                {
                    move = cap;
                }
                else
                {
                    max -= Game.Rand.RangeInt(0, 1);
                    int m = MinMax();
                    if (max < m)
                        max = m;
                    else if (limit > max + (int)move)
                        limit -= Game.Rand.RangeInt(0, 1);
                }
                movable = new(move, max, limit);
            }

            return movable;
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
            int sign = this.ResearchLevel - other.ResearchLevel;
            if (sign == 0)
                sign = this.BlueprintNum.CompareTo(other.BlueprintNum);
            return sign;
        }

        public override string ToString()
        {
            return "Type " + BlueprintNum;
        }
    }
}
