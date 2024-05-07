using ClassLibrary1.Pieces.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary1
{
    [Serializable]
    public class Research : IResearch
    {
        public const int StartResearch = 20;
        public readonly Game Game;
        Game IResearch.Game => Game;

        private Type _researching;
        private readonly IReadOnlyDictionary<Type, int> _minResearch;
        private readonly Dictionary<Type, int> _lastSeen;
        private readonly Dictionary<Type, int> _researchedTypes;
        private readonly Dictionary<Type, int> _progress;
        private readonly Dictionary<Type, int> _choices;

        public const int MaxChoices = 6;
        private int _numChoices = 3;
        private int _researchLast;
        private double _nextAvg;

        private readonly SortedSet<MechBlueprint> _blueprints;

        public Type Researching
        {
            get { return _researching; }
            set
            {
                if (!Available.Contains(value))
                    throw new Exception();
                _researching = value;
            }
        }
        public IReadOnlyCollection<Type> Available => _choices.Keys;
        public IReadOnlyCollection<Type> Done => _researchedTypes.Keys;
        public int ResearchCur => _researchLast + _progress.Values.Sum();
        public int TurnLastAvailable(Type type)
        {
            _lastSeen.TryGetValue(type, out int result);
            return result;
        }
        public int GetLast(Type type)
        {
            _researchedTypes.TryGetValue(type, out int result);
            return result;
        }
        public int GetProgress(Type type)
        {
            _progress.TryGetValue(type, out int result);
            return result;
        }
        public int GetCost(Type type)
        {
            if (_choices.TryGetValue(type, out int result))
                return result;
            throw new Exception();
        }
        public IReadOnlyList<MechBlueprint> Blueprints => _blueprints.ToList().AsReadOnly();

        public Research(Game Game)
        {
            this.Game = Game;

            this._researching = Type.Mech;
            this._lastSeen = new();
            this._researchedTypes = new();
            this._progress = new() { { _researching, StartResearch } };
            this._choices = new() { { _researching, 50 } };

            this._researchLast = 0;
            this._nextAvg = 26;

            this._blueprints = new();

            this._minResearch = CalcMinResearch();
        }

        internal bool HasScrap(int amt)
        {
            return _progress[_researching] >= amt;
        }
        internal void Scrap(int amt)
        {
            this._progress[_researching] -= amt;
        }

        internal Type? AddResearch(double research)
        {
            foreach (Type type in _choices.Keys)
                _lastSeen[type] = Game.Turn;

            if (_researching != Type.Mech)
                research = Consts.Income(research);
            int add = Game.Rand.Round(research);
            this._progress[_researching] += add;

            Type? result = null;
            if (_progress[_researching] >= _choices[_researching])
            {
                // excess research will be applied to a random available next choice
                int excess = _progress[_researching] - _choices[_researching];
                // upgrading researches you have done previously will cause less of an overall research cost increase
                int previous = GetLast(_researching);
                result = OnResearch();
                GetNextChoices(excess, previous, result.Value);
            }

            return result;
        }

        private Type OnResearch()
        {
            this._researchLast += _choices[_researching];
            this._researchedTypes[_researching] = _researchLast;
            this._progress.Remove(_researching);
            Game.Player.OnResearch(_researching, GetUpgMult(_researching, _researchLast));
            if (_researching == Type.Mech || IsMech(_researching))
                MechBlueprint.OnResearch(this, _blueprints);
            if (_researching == Type.ResearchChoices)
                _numChoices++;
            return _researching;
        }
        private double GetUpgMult(Type type, int research) => GetResearchMult(research > _minResearch[type] ? research - _minResearch[type] : 0);
        public string GetUpgInfo(Type type)
        {
            if (!HasMin(type))
                return string.Empty;// Array.Empty<string>();
            if (type == Type.ResearchChoices)
                return ResearchUpgValues.GetUpgInfo(Type.ResearchChoices, _numChoices, _numChoices + 1, v => v.ToString("0"));
            double prevMult = GetUpgMult(type, GetLast(type));
            double nextMult = GetUpgMult(type, _researchLast + _choices[type]);
            return ResearchUpgValues.GetUpgInfo(type, prevMult, nextMult);
        }
        private static bool HasMin(Type type) => !NoUpgrades.Contains(type) && !IsMech(type);

        public static double GetResearchMult(double research) => (research + Consts.ResearchFactor) / Consts.ResearchFactor;

        private void GetNextChoices(int excess, int previous, Type result)
        {
            GetCostParams(excess, previous, out double nextAvg, out double nextDev, out double nextOE, out double nextMin);

            this._choices.Clear();
            while (!_choices.Any())
            {
                Dictionary<Type, int> types = GetTypeChances(nextAvg);
                while (types.Any() && _choices.Count < _numChoices)
                {
                    Type available = Game.Rand.SelectValue(types);
                    types.Remove(available);
                    //cannot have the same research you just did immediately available again
                    if (available == result)
                        continue;
                    //limit max research choices
                    if (available == Type.ResearchChoices && _numChoices == MaxChoices)
                        continue;
                    //certain types can only be researched once
                    bool noUpg = NoUpgrades.Contains(available);
                    if (HasType(available) && noUpg)
                        continue;
                    //ensure always at least one mech and one non-mech option
                    if (_choices.Count == _numChoices - 1 && (IsMech(available) ? _choices.Keys.All(IsMech) : !_choices.Keys.Any(IsMech)))
                        continue;
                    //enforce minimums
                    if (_researchLast <= _minResearch[available])
                        continue;
                    if (Dependencies[available].All(d => HasType(d)))
                    {
                        this._progress.TryAdd(available, 0);
                        this._choices.Add(available, CalcCost(available, nextAvg, nextDev, nextOE, nextMin));
                    }
                }
            }

            this._researching = Game.Rand.SelectValue(_choices.Keys);
            this._progress[_researching] += excess;
        }
        private Dictionary<Type, int> GetTypeChances(double nextAvg)
        {
            return Enum.GetValues<Type>().ToDictionary(t => t, type =>
            {
                int lastSeen = TurnLastAvailable(type);
                double mult = lastSeen > 0 ? 1 : 1.3;
                mult *= 1.3 - lastSeen / ((double)Game.Turn + 16.9);

                int last = GetLast(type);
                bool hasType = last > 0;
                mult *= (_researchLast + Consts.ResearchFactor) / (last + Consts.ResearchFactor);

                if (!hasType)
                {
                    int min = _minResearch[type];
                    double minMult = (_researchLast - min) * 16.9 / (min + _minResearch.Values.Average() + Consts.ResearchFactor);
                    if (minMult > 0)
                        minMult = Math.Sqrt(minMult);
                    mult *= minMult;
                }

                if (IsUpgradeOnly(type, last) && (hasType || GetAllUnlocks(type).All(t => IsUpgradeOnly(t, GetLast(t)))))
                {
                    double upgMult = (_researchLast - last) / Consts.ResearchFactor;
                    upgMult = Math.Pow(upgMult, upgMult > 1 ? .39 : .78);
                    mult *= upgMult;
                }

                int progress = GetProgress(type);
                mult *= Math.Sqrt((nextAvg + progress) / nextAvg);

                return Game.Rand.Round(byte.MaxValue * mult);
            }).Where(p => p.Value > 0).ToDictionary(p => p.Key, p => p.Value);
        }
        private void GetCostParams(int excess, int previous, out double nextAvg, out double nextDev, out double nextOE, out double nextMin)
        {
            double mult = (1 - Math.Pow(previous / (double)_researchLast, _researchLast / Consts.ResearchFactor));
            if (IsUpgradeOnly(_researching, previous))
                mult *= .65;
            nextAvg = GetNext(_nextAvg) * mult;
            this._nextAvg += nextAvg;

            double devDiv = Math.Pow(_researchLast + nextAvg, .21);
            nextDev = 1.04 / devDiv;
            nextOE = 1.69 / devDiv / devDiv;

            nextMin = 1 + Math.Max(excess, 0);
            nextMin = Game.Rand.GaussianOE(nextMin * 1.3 + 26, .13, .13, nextMin);

            nextAvg = (GetNext(_researchLast) + nextAvg) / 2.0;
            nextAvg = Game.Rand.GaussianOE(nextAvg, nextDev * 2.1, nextOE * 1.3, nextAvg > nextMin ? nextMin : 1);
            nextAvg = (_researchLast + nextAvg + _nextAvg) / 2.0 - _researchLast;
            if (nextAvg < 1)
                nextAvg = 1;
        }
        private static bool IsUpgradeOnly(Type type, int previous)
        {
            return (previous > 0 && !IsMech(type)) || UpgradeOnly.Contains(type);
        }
        private int CalcCost(Type type, double nextAvg, double nextDev, double nextOE, double nextMin)
        {
            int last = GetLast(type);
            double mult = (last + GetNext(last)) / (_researchLast + nextAvg);
            mult = 1 + mult * mult;
            mult *= (double)type / _avgTypeCost;
            int Add() => Game.Rand.OEInt(13 * mult);

            nextAvg = nextAvg * mult + Add();
            double progress = _progress[type];
            int min = Game.Rand.Round(nextMin + progress);
            if (nextAvg > min)
                return Game.Rand.GaussianOEInt(nextAvg, nextDev, nextOE, min);
            else
                return min + Add();
        }
        private static double GetNext(double v) => Math.Pow(v * 5.2 + 1690, .52);

        public bool HasType(Type research)
        {
            return GetLast(research) > 0;
        }

        public int GetLevel()
        {
            return _researchLast;
        }
        Type IResearch.GetType()
        {
            return _researching;
        }
        public int GetMinCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + 2.6 * Consts.ResearchFactor, 0.78));
        }
        public int GetMaxCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + 1.69 * Consts.ResearchFactor, 0.91));
        }
        public bool MakeType(Type type)
        {
            double totalPow, typePow;
            switch (type)
            {
                case Type.MechShields:
                case Type.MechLasers:
                    totalPow = .13;
                    typePow = .91;
                    break;
                case Type.MechRange:
                case Type.MechEnergyWeapons:
                    totalPow = .39;
                    typePow = .65;
                    break;
                case Type.MechArmor:
                case Type.MechExplosives:
                    totalPow = 1.3;
                    typePow = .52;
                    break;
                default: throw new Exception();
            }

            double chance = .65 * Math.Pow(GetLevel() / (GetLevel() + Consts.ResearchFactor), totalPow);
            chance *= Math.Pow(GetLast(type) / (double)GetLevel(), typePow);

            return HasType(type) && Game.Rand.Bool(chance);
        }
        public double GetMult(Type type, double pow)
        {
            return Math.Pow(GetResearchMult(GetLevel()) * GetResearchMult(GetLast(type)), pow / 2.0);
        }

        private static IReadOnlyDictionary<Type, int> CalcMinResearch()
        {
            //help key types to not come too late
            HashSet<Type> keyTechs = new() { Type.Factory, Type.MechMove, Type.TurretRange, Type.ConstructorDefense, Type.ConstructorMove,
                Type.FactoryRepair, Type.FactoryConstructor, Type.BuildingDefense, Type.ExtractorAutoRepair, };

            Dictionary<Type, int> retVal = new();
            HashSet<Type> all = Enum.GetValues<Type>().ToHashSet();

            double research = 0;// StartResearch;
            double dev = Math.E * _avgTypeCost / (double)all.Min();
            while (all.Any())
            {
                Type type = Game.Rand.SelectValue(all.Where(t1 => Dependencies[t1].All(t2 => retVal.ContainsKey(t2))));
                all.Remove(type);
                research += .78 * GetNext(research) * (double)type / _avgTypeCost;
                int min = 0;
                if (GetAllDependencies(type).Count > 2)
                    min = Game.Rand.GaussianCappedInt(research, dev / Math.Sqrt(research));
                else
                    ;
                if (keyTechs.Contains(type))
                    min = Game.Rand.Round(Math.Pow(min, .91));
                retVal.Add(type, min);
            }

            return retVal.AsReadOnly();
        }

        public static IEnumerable<Type> GetUnlocks(Type selected)
        {
            return GetUnlocks(selected, GetDependencies);
        }
        public static IEnumerable<Type> GetAllUnlocks(Type selected)
        {
            return GetUnlocks(selected, GetAllDependencies);
        }
        private static IEnumerable<Type> GetUnlocks(Type selected, Func<Type, HashSet<Type>> GetDependencies)
        {
            return Enum.GetValues<Type>().Where(t => GetDependencies(t).Contains(selected));
        }
        public static HashSet<Type> GetDependencies(Type type)
        {
            HashSet<Type> allDependencies = GetAllDependencies(type);
            allDependencies.RemoveWhere(a => allDependencies.Any(b => GetAllDependencies(b).Contains(a)));
            return allDependencies;
        }
        public static HashSet<Type> GetAllDependencies(Type type)
        {
            HashSet<Type> allDependencies = new(Dependencies[type]);
            foreach (Type a in allDependencies.ToArray())
                foreach (Type b in GetAllDependencies(a))
                    allDependencies.Add(b);
            return allDependencies;
        }

        public static readonly Type[] NoUpgrades = new Type[] { Type.Mech, Type.Constructor, Type.Turret, Type.Factory,
            Type.TurretLasers, Type.TurretExplosives, Type.TurretShields, Type.TurretArmor, Type.TurretAutoRepair,
            Type.FactoryConstructor, Type.FactoryAutoRepair, Type.ExtractorAutoRepair, Type.BurnMass, Type.ScrapResearch, Type.FabricateMass, };
        public static readonly Type[] UpgradeOnly = new Type[] { Type.ConstructorCost, Type.ConstructorMove,
            Type.TurretRange, Type.TurretAttack, Type.TurretDefense, Type.BuildingCost,
            Type.BuildingDefense, Type.ResearchChoices, Type.ExtractorValue, };
        //public static readonly Dictionary<Type, Type[,]> Dependencies = new()
        //{
        //    { Type.Mech, new Type[,] { } },
        //    { Type.CoreShields, new Type[,]          { { Type.Mech, Type.Mech }, { Type.CoreShields, Type.CoreShields }, } },
        //    { Type.Constructor, new Type[,]          { { Type.CoreShields, }, } },
        //    { Type.Turret, new Type[,]               { { Type.Constructor, }, } },
        //    { Type.Factory, new Type[,]              { { Type.Constructor, }, } },
        public static readonly Dictionary<Type, Type[]> Dependencies = new()
        {
            { Type.Mech, Array.Empty<Type>() },
            { Type.CoreShields, new Type[]          { Type.Mech, } },
            { Type.Turret, new Type[]               { Type.CoreShields, } },
            { Type.Constructor, new Type[]          { Type.CoreShields, } },
            { Type.Factory, new Type[]              { Type.Constructor, } },

            { Type.MechVision, new Type[]           { Type.Mech, } },
            { Type.MechShields, new Type[]          { Type.Mech, Type.MechVision, } },
            { Type.MechMove, new Type[]             { Type.Mech, Type.MechShields, } },
            { Type.MechDefense, new Type[]          { Type.Mech, Type.MechShields, } },
            { Type.MechArmor, new Type[]            { Type.Mech, Type.MechDefense, } },
            { Type.MechResilience, new Type[]       { Type.Mech, Type.MechMove, Type.MechArmor } },
            { Type.MechAttack, new Type[]           { Type.Mech, Type.MechVision, } },
            { Type.MechRange, new Type[]            { Type.Mech, Type.MechAttack, } },
            { Type.MechEnergyWeapons, new Type[]    { Type.Mech, Type.MechAttack, } },
            { Type.MechLasers, new Type[]           { Type.Mech, Type.MechRange, Type.MechEnergyWeapons, } },
            { Type.MechExplosives, new Type[]       { Type.Mech, Type.MechRange, Type.MechResilience, } },

            { Type.TurretAttack, new Type[]         { Type.Turret, Type.MechAttack, } }, // delay
            { Type.TurretLasers, new Type[]         { Type.Turret, Type.TurretAttack, Type.MechLasers, } }, // end
            { Type.TurretRange, new Type[]          { Type.Turret, Type.TurretAttack, Type.MechRange, } },
            { Type.TurretExplosives, new Type[]     { Type.Turret, Type.TurretRange, Type.MechExplosives } }, // end
            { Type.TurretDefense, new Type[]        { Type.Turret, } }, // quick
            { Type.TurretArmor, new Type[]          { Type.Turret, Type.TurretDefense, Type.MechArmor, } }, // end
            { Type.TurretShields, new Type[]        { Type.Turret, Type.MechShields, } },
            { Type.TurretAutoRepair, new Type[]     { Type.Turret, Type.TurretShields, Type.FactoryAutoRepair, } }, // end

            { Type.ConstructorCost, new Type[]      { Type.Constructor, } }, // quick
            { Type.ConstructorDefense, new Type[]   { Type.Constructor, Type.MechShields, Type.MechArmor, } },
            { Type.ConstructorMove, new Type[]      { Type.Constructor, Type.ConstructorDefense, Type.MechVision, Type.MechMove, } }, // end
            { Type.ConstructorRepair, new Type[]    { Type.Constructor, Type.FactoryConstructor, Type.FabricateMass, } }, // end

            { Type.FactoryRepair, new Type[]        { Type.Factory, } },
            { Type.FactoryConstructor, new Type[]   { Type.Factory, Type.FactoryRepair, Type.ConstructorCost, } },
            { Type.FactoryAutoRepair, new Type[]    { Type.Factory, Type.FactoryRepair, Type.ExtractorAutoRepair, } },

            { Type.BuildingDefense, new Type[]      { Type.Constructor } }, // quick
            { Type.ExtractorAutoRepair, new Type[]  { Type.BuildingDefense, } },
            { Type.BuildingCost, new Type[]         { Type.BuildingDefense, Type.ConstructorCost, } },
            { Type.ResearchChoices, new Type[]      { Type.Turret, } },
            { Type.ScrapResearch, new Type[]        { Type.ResearchChoices, } },
            { Type.BurnMass, new Type[]             { Type.Factory, } },
            { Type.FabricateMass, new Type[]        { Type.ScrapResearch, Type.BurnMass, Type.FactoryAutoRepair, } },
            { Type.ExtractorValue, new Type[]       { Type.ExtractorAutoRepair, Type.BuildingCost, Type.ScrapResearch, Type.BurnMass, } }, // end
            
            //Type.BuildingResilience - not extractor??
            // Constructor resilience ?
        };

        public static bool IsMech(Type type) => type != Type.Mech && type.ToString().StartsWith("Mech");

        private const double _avgTypeCost = (double)Type.Mech;
        // int value is used as relative cost
        public enum Type
        {
            CoreShields = 100,
            Mech = 169,
            Constructor = 200,
            Turret = 250,
            Factory = 300,

            MechEnergyWeapons = 102,
            MechShields = 105,
            MechResilience = 111,
            MechVision = 114,
            MechAttack = 118,
            MechDefense = 121,
            MechLasers = 129,
            MechMove = 133,
            MechRange = 137,
            MechArmor = 146,
            MechExplosives = 149,

            TurretDefense = 110, // quick
            TurretShields = 120,
            TurretLasers = 130, // end
            TurretRange = 140,
            TurretArmor = 150, // end
            TurretExplosives = 160, // end
            TurretAttack = 170, // delay
            TurretAutoRepair = 245, // end

            ConstructorCost = 180, // quick
            ConstructorDefense = 190, //key
            ConstructorMove = 330, // end
            ConstructorRepair = 350, // end

            FactoryAutoRepair = 205,
            FactoryRepair = 240, //key
            FactoryConstructor = 290, //key

            BuildingDefense = 115, // quick
            FabricateMass = 125,
            ScrapResearch = 145,
            ResearchChoices = 155,
            BurnMass = 165,
            BuildingCost = 175,
            ExtractorAutoRepair = 285, //key
            ExtractorValue = 340, // end   
        }
    }
}
