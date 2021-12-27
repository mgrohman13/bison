using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;

namespace ClassLibrary1
{
    [Serializable]
    public class Research : IResearch
    {
        public readonly Game Game;
        Game IResearch.Game => Game;

        private Type _researching;
        private readonly Dictionary<Type, int> _lastSeen;
        private readonly Dictionary<Type, int> _researchedTypes;
        private readonly Dictionary<Type, int> _progress;
        private readonly Dictionary<Type, int> _choices;

        //private int _researchCur;
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
        public IReadOnlyCollection<MechBlueprint> Blueprints => _blueprints.ToList().AsReadOnly();

        public Research(Game Game)
        {
            this.Game = Game;

            this._researching = Type.Mech;
            this._lastSeen = new();
            this._researchedTypes = new();
            this._progress = new() { { _researching, 0 } };
            this._choices = new() { { _researching, 25 } };

            //this._researchCur = 0;
            this._researchLast = 0;
            this._nextAvg = 26;

            this._blueprints = new();
        }

        internal bool HasScrap(int amt)
        {
            return _progress[_researching] >= amt;
        }
        internal void Scrap(int amt)
        {
            //this._researchCur -= amt;
            this._progress[_researching] -= amt;
        }

        internal Type? AddResearch(double research)
        {
            //if (_researchLast + _progress.Values.Sum() != _researchCur)
            //    throw new Exception();

            foreach (Type type in _choices.Keys)
                _lastSeen[type] = Game.Turn;

            int add = Consts.Income(research);
            //this._researchCur += add;
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
            Game.Player.OnResearch(_researching, GetResearchMult(_researchLast));
            if (IsMech(_researching))
                MechBlueprint.OnResearch(this, _blueprints);
            if (_researching == Type.ResearchChoices)
                _numChoices++;
            return _researching;
        }
        public static double GetResearchMult(double research)
        {
            return (research + Consts.ResearchFactor) / Consts.ResearchFactor;
        }

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
                    //max of 6 research choices
                    if (available == Type.ResearchChoices && _numChoices == 6)
                        continue;
                    //certain types can only be researched once
                    if (_researchedTypes.ContainsKey(available) && NoUpgrades.Contains(available))
                        continue;
                    // ensure always at least one mech and one non-mech option
                    if (_choices.Count == _numChoices - 1 && (IsMech(available) ? _choices.Keys.All(IsMech) : !_choices.Keys.Any(IsMech)))
                        continue;
                    if (Dependencies[available].All(d => _researchedTypes.ContainsKey(d)))
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
                mult *= (_researchLast + Consts.ResearchFactor) / (last + Consts.ResearchFactor);

                if (IsUpgradeOnly(last))
                {
                    double upgMult = (_researchLast - last) / Consts.ResearchFactor;
                    if (upgMult > 1)
                        upgMult = Math.Sqrt(upgMult);
                    mult *= upgMult;
                }

                int progress = GetProgress(type);
                double progressMult = (nextAvg + progress) / nextAvg;
                mult *= progressMult * progressMult;

                return Game.Rand.Round(byte.MaxValue * mult);
            }).Where(p => p.Value > 0).ToDictionary(p => p.Key, p => p.Value);
        }
        private void GetCostParams(int excess, int previous, out double nextAvg, out double nextDev, out double nextOE, out double nextMin)
        {
            double mult = (1 - Math.Pow(previous / (double)_researchLast, _researchLast / Consts.ResearchFactor));
            if (IsUpgradeOnly(previous))
                mult *= .65;
            nextAvg = GetNext(_nextAvg) * mult;
            this._nextAvg += nextAvg;

            double devDiv = Math.Pow(_researchLast + nextAvg, .21);
            nextDev = 1.04 / devDiv;
            nextOE = 1.69 / devDiv / devDiv;

            nextMin = 1 + Math.Max(excess, 0);
            nextMin = Game.Rand.GaussianOE(nextMin * 1.3 + 26, .13, .13, nextMin);

            nextAvg = (GetNext(_researchLast) + nextAvg) / 2.0;
            nextAvg = Game.Rand.GaussianOE(nextAvg, nextDev * 2.1, nextOE * 1.3, nextAvg > nextMin ? nextMin : 0);
            nextAvg = (_researchLast + nextAvg + _nextAvg) / 2.0 - _researchLast;
        }
        private bool IsUpgradeOnly(int previous)
        {
            return (previous > 0 && !IsMech(_researching)) || UpgradeOnly.Contains(_researching);
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
        private static double GetNext(double v) => Math.Pow(v * 5.2 + 520, .52);

        internal bool HasType(Type research)
        {
            return _researchedTypes.ContainsKey(research);
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
            return Game.Rand.Round(Math.Pow(GetLevel() + 1.17 * Consts.ResearchFactor, 0.78) - 21);
        }
        public int GetMaxCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + 0.78 * Consts.ResearchFactor, 0.91) - 39);
        }
        public bool MakeType(Type type)
        {
            double chance = .5;
            switch (type)
            {
                case Type.MechShields:
                    chance *= 1;
                    break;
                case Type.MechSP:
                case Type.MechAP:
                    chance *= Math.Pow(GetLevel() / (GetLevel() + Consts.ResearchFactor), 1.69);
                    goto case Type.MechArmor;
                case Type.MechArmor:
                    chance *= Math.Pow(GetLast(type) / (double)GetLevel(), .65);
                    break;
                default: throw new Exception();
            }
            return HasType(type) && Game.Rand.Bool(chance);
        }
        public double GetMult(Type type, double pow)
        {
            return Math.Pow(GetResearchMult(GetLevel()) * GetResearchMult(GetLast(type)), pow / 2.0);
        }

        private static readonly Type[] NoUpgrades = new Type[] { Type.Mech, Type.Constructor, Type.Turret,  Type.Factory, Type.FactoryConstructor,
            Type.ExtractorAutoRepair, Type.FactoryAutoRepair, Type.TurretAutoRepair, Type.BurnMass, Type.FabricateMass, Type.ScrapResearch };
        private static readonly Type[] UpgradeOnly = new Type[] { Type.ConstructorCost, Type.ConstructorMove, Type.TurretRange, Type.TurretDefense,
            Type.BuildingCost, Type.BuildingHits, Type.ExtractorValue, Type.ResearchChoices };
        private static readonly Dictionary<Type, Type[]> Dependencies = new()
        {
            { Type.Mech, Array.Empty<Type>() },
            { Type.CoreShields, new Type[] { Type.Mech } },
            { Type.Constructor, new Type[] { Type.CoreShields } },
            { Type.Turret, new Type[] { Type.Constructor } },
            { Type.Factory, new Type[] { Type.Constructor } },

            { Type.MechShields, new Type[] { Type.Mech } },
            { Type.MechVision, new Type[] { Type.Mech, Type.MechShields } },
            { Type.MechMove, new Type[] { Type.Mech, Type.MechVision } },
            { Type.MechHits, new Type[] { Type.Mech, Type.MechShields } },
            { Type.MechArmor, new Type[] { Type.Mech, Type.MechHits } },
            { Type.MechDamage, new Type[] { Type.Mech, Type.MechShields } },
            { Type.MechSP, new Type[] { Type.Mech, Type.MechDamage, Type.MechShields } },
            { Type.MechRange, new Type[] { Type.Mech, Type.MechSP } },
            { Type.MechAP, new Type[] { Type.Mech, Type.MechRange, Type.MechArmor } },
            { Type.MechResilience, new Type[] { Type.Mech, Type.MechMove, Type.MechAP } },

            { Type.ConstructorCost, new Type[] { Type.Constructor } }, // early
            { Type.ConstructorDefense, new Type[] { Type.Constructor, Type.MechShields, Type.MechArmor } }, // early
            { Type.ConstructorMove, new Type[] { Type.Constructor, Type.ConstructorCost, Type.Factory, Type.MechVision, Type.MechMove } },
            { Type.ConstructorRepair, new Type[] { Type.Constructor, Type.ConstructorDefense, Type.ConstructorMove, Type.FactoryConstructor } }, // end

            { Type.TurretRange, new Type[] { Type.Turret, Type.MechRange } },
            { Type.TurretAttack, new Type[] { Type.Turret, Type.MechDamage, Type.MechSP, Type.MechAP } },
            { Type.TurretDefense, new Type[] { Type.Turret, Type.CoreShields, Type.MechArmor } },

            { Type.FactoryRepair, new Type[] { Type.Factory, Type.BuildingCost } }, // early
            { Type.FactoryConstructor, new Type[] { Type.Factory, Type.FactoryRepair, Type.ConstructorDefense } },

            { Type.BuildingCost, new Type[] { Type.CoreShields } },
            { Type.BuildingHits, new Type[] { Type.BuildingCost, Type.Turret, Type.MechHits } },
            { Type.ExtractorValue, new Type[] { Type.ExtractorAutoRepair, Type.MechResilience } }, // end

            { Type.ExtractorAutoRepair, new Type[] { Type.BuildingCost } }, // early
            { Type.FactoryAutoRepair, new Type[] { Type.Factory, Type.BuildingHits, Type.FactoryRepair } },
            { Type.TurretAutoRepair, new Type[] { Type.Turret, Type.BuildingHits, Type.TurretDefense } },

            { Type.ScrapResearch, new Type[] { Type.BuildingHits } },
            { Type.FabricateMass, new Type[] { Type.BuildingHits } },
            { Type.BurnMass, new Type[] { Type.BuildingHits } },
            { Type.ResearchChoices, new Type[] { Type.ScrapResearch, Type.FabricateMass, Type.BurnMass, } }, // end
        };

        public static bool IsMech(Type type) => type.ToString().StartsWith("Mech");
        private const double _avgTypeCost = (double)Type.Mech;
        // int value is used as relative cost
        public enum Type
        {
            Mech = 169,
            Constructor = 200,
            Turret = 250,
            Factory = 300,

            MechShields = 102,
            MechVision = 105,
            MechMove = 137,
            MechHits = 113,
            MechArmor = 124,
            MechDamage = 116,
            MechSP = 120,
            MechRange = 129,
            MechAP = 138,
            MechResilience = 141,

            ConstructorCost = 190, // early 
            ConstructorDefense = 220, // early
            ConstructorMove = 270,
            ConstructorRepair = 290, // end

            TurretRange = 150,
            TurretAttack = 140,
            TurretDefense = 110,

            FactoryRepair = 160, // early
            FactoryConstructor = 320,

            CoreShields = 100,
            BuildingCost = 155,
            BuildingHits = 115,
            ExtractorValue = 350, // end

            ExtractorAutoRepair = 245, // early
            FactoryAutoRepair = 205,
            TurretAutoRepair = 285,

            ScrapResearch = 125,
            FabricateMass = 145,
            BurnMass = 165,
            ResearchChoices = 170,

            // min 100  avg 169  max 350
            // avoid:  104 117 130 143 156  130 195 260 325
        }
    }
}
