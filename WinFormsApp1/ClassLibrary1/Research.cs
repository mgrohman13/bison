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
        private readonly Dictionary<Type, double> _researchedTypes;
        private readonly Dictionary<Type, double> _progress;
        private readonly Dictionary<Type, double> _options;

        private double _researchCur;
        private double _researchLast;
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
        public IReadOnlyCollection<Type> Available => _options.Keys;
        public IReadOnlyCollection<Type> Done => _researchedTypes.Keys;
        public double ResearchCur => _researchCur;
        public double GetLast(Type type)
        {
            _researchedTypes.TryGetValue(type, out double result);
            return result;
        }
        public double GetProgress(Type type)
        {
            _progress.TryGetValue(type, out double result);
            return result;
        }
        public double GetCost(Type type)
        {
            if (_options.TryGetValue(type, out double result))
                return result;
            throw new Exception();
        }
        public IReadOnlyCollection<MechBlueprint> Blueprints => _blueprints.ToList().AsReadOnly();

        public Research(Game Game)
        {
            this.Game = Game;

            this._researching = Type.Mech;
            this._researchedTypes = new();
            this._progress = new() { { _researching, 0 } };
            this._options = new() { { _researching, 25 } };

            this._researchCur = 0;
            this._researchLast = 0;
            this._nextAvg = 26;

            this._blueprints = new();
        }

        internal Type? AddResearch(double research)
        {
            this._researchCur += research;
            this._progress[_researching] += research;

            Type? result = null;
            if (_progress[_researching] >= _options[_researching])
            {
                // excess research will be applied to a random available next choice
                double excess = _progress[_researching] - _options[_researching];
                // upgrading researches you have done previously will cause less of an overall research cost increase
                this._researchedTypes.TryGetValue(_researching, out double previous);
                result = OnResearch();
                GetNextChoices(excess, previous);
            }
            return result;
        }
        private Type OnResearch()
        {
            this._researchLast += _options[_researching];
            this._researchedTypes[_researching] = _researchLast;
            this._progress.Remove(_researching);
            Game.Player.OnResearch(_researching, GetResearchMult(_researchLast));
            if (IsMech(_researching))
                MechBlueprint.OnResearch(this, _blueprints);
            return _researching;
        }
        public static double GetResearchMult(double research)
        {
            return (research + Consts.ResearchFactor) / Consts.ResearchFactor;
        }

        private void GetNextChoices(double excess, double previous)
        {
            const int choices = 3;

            GetCostParams(excess, previous, out double nextAvg, out double nextDev, out double nextOE, out double nextMin);

            this._options.Clear();
            foreach (Type available in Game.Rand.Iterate(Enum.GetValues<Type>()))
            {
                if (_researchedTypes.ContainsKey(available) && BaseTypes.Contains(available))
                    continue;
                // ensure always at least one mech and one non-mech option
                if (_options.Count == choices - 1 && (IsMech(available) ? _options.Keys.All(IsMech) : !_options.Keys.Any(IsMech)))
                    continue;
                if (Dependencies[available].All(d => _researchedTypes.ContainsKey(d)))
                {
                    if (_progress.ContainsKey(available) && this._progress[available] > 0)
                        ;
                    this._progress.TryAdd(available, 0);
                    this._options.Add(available, CalcCost(available, nextAvg, nextDev, nextOE, nextMin));
                    if (_options.Count == 1)
                    {
                        if (this._progress[available] > 0 && excess > 0)
                            ;
                        this._progress[available] += excess;
                        this._researching = available;
                    }
                    if (_options.Count == choices)
                        break;
                }
            }
        }
        private void GetCostParams(double excess, double previous, out double nextAvg, out double nextDev, out double nextOE, out double nextMin)
        {
            nextAvg = GetNext(_nextAvg) * (1 - Math.Pow(previous / _researchLast, _researchLast / Consts.ResearchFactor));
            this._nextAvg += nextAvg;

            double devDiv = Math.Pow(_researchCur + nextAvg, .21);
            nextDev = 1.04 / devDiv;
            nextOE = 1.69 / devDiv / devDiv;

            nextMin = 1 + Math.Max(excess, 0);
            nextMin = Game.Rand.GaussianOE(nextMin * 1.3 + 26, .13, .13, nextMin);

            nextAvg = (GetNext(_researchCur) + nextAvg) / 2.0;
            nextAvg = Game.Rand.GaussianOE(nextAvg, nextDev * 2.1, nextOE * 1.3, nextAvg > nextMin ? nextMin : 0);
            nextAvg = (_researchLast + nextAvg + _nextAvg) / 2.0 - _researchLast;
        }
        private double CalcCost(Type type, double nextAvg, double nextDev, double nextOE, double nextMin)
        {
            _researchedTypes.TryGetValue(type, out double last);
            double mult = (last + GetNext(last)) / (_researchLast + nextAvg);
            mult = 1 + mult * mult;
            mult *= (double)type / _avgTypeCost;
            double add = Game.Rand.OE(13 * mult);

            nextAvg = nextAvg * mult + add;
            double progress = _progress[type];
            nextMin += progress;
            if (nextAvg > nextMin)
                nextAvg = Game.Rand.GaussianOE(nextAvg, nextDev, nextOE, nextMin);
            else
                nextAvg = nextMin + add;

            return nextAvg;
        }
        private static double GetNext(double v) => Math.Pow(v * 1.69 + 130, .65);

        internal bool HasType(Type research)
        {
            return _researchedTypes.ContainsKey(research);
        }

        public double GetLevel()
        {
            return _researchLast;
        }
        Type IResearch.GetType()
        {
            return _researching;
        }
        public double GetMinCost()
        {
            return Math.Pow(GetLevel() + 1.04 * Consts.ResearchFactor, .78);
        }
        public double GetMaxCost()
        {
            return Math.Pow(GetLevel() + 1.04 * Consts.ResearchFactor, .91);
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
                    chance *= Math.Pow(GetLast(type) / GetLevel(), .65);
                    break;
                default: throw new Exception();
            }
            return HasType(type) && Game.Rand.Bool(chance);
        }
        public double GetMult(Type type, double pow)
        {
            return Math.Pow(GetResearchMult(GetLevel()) * GetResearchMult(GetLast(type)), pow / 2.0);
        }

        private static readonly Type[] BaseTypes = new Type[] { Type.Mech, Type.Constructor, Type.Turret, Type.Factory, Type.FactoryConstructor, Type.ExtractorAutoRepair, Type.FactoryAutoRepair, Type.TurretAutoRepair };
        private static readonly Dictionary<Type, Type[]> Dependencies = new()
        {
            { Type.Mech, Array.Empty<Type>() },
            { Type.Constructor, new Type[] { Type.Mech } },
            { Type.CoreShields, new Type[] { Type.Constructor } },
            { Type.Turret, new Type[] { Type.CoreShields } },
            { Type.Factory, new Type[] { Type.CoreShields } },

            { Type.MechShields, new Type[] { Type.Mech } },
            { Type.MechVision, new Type[] { Type.MechShields } },
            { Type.MechMove, new Type[] { Type.MechVision } },
            { Type.MechHits, new Type[] { Type.MechShields } },
            { Type.MechArmor, new Type[] { Type.MechHits } },
            { Type.MechResilience, new Type[] { Type.MechArmor } },
            { Type.MechDamage, new Type[] { Type.MechShields } },
            { Type.MechSP, new Type[] { Type.MechDamage, Type.MechShields } },
            { Type.MechRange, new Type[] { Type.MechSP } },
            { Type.MechAP, new Type[] { Type.MechRange, Type.MechArmor } },

            { Type.TurretDefense, new Type[] { Type.Turret, Type.CoreShields, Type.MechArmor } },
            { Type.TurretAttack, new Type[] { Type.Turret, Type.MechDamage, Type.MechSP, Type.MechAP } },
            { Type.TurretRange, new Type[] { Type.Turret, Type.MechRange } },

            { Type.ConstructorMove, new Type[] { Type.Constructor, Type.MechMove } },
            { Type.ConstructorDefense, new Type[] { Type.Constructor, Type.MechShields, Type.MechVision, Type.MechArmor } },
            { Type.ConstructorRepair, new Type[] { Type.Constructor, Type.ConstructorDefense, Type.Factory } },
            { Type.FactoryRepair, new Type[] { Type.Factory, Type.ConstructorRepair } },
            { Type.FactoryConstructor, new Type[] { Type.Factory, Type.FactoryRepair, Type.ConstructorMove } },

            { Type.BuildingCost, new Type[] { Type.CoreShields } },
            { Type.ExtractorAutoRepair, new Type[] { Type.BuildingCost } },
            { Type.FactoryAutoRepair, new Type[] { Type.Factory, Type.ExtractorAutoRepair } },
            { Type.TurretAutoRepair, new Type[] { Type.Turret, Type.FactoryAutoRepair, Type.TurretDefense } },
            { Type.ConstructorCost, new Type[] { Type.Constructor, Type.BuildingCost } },
            { Type.BuildingHits, new Type[] { Type.ConstructorCost, Type.Turret, Type.MechVision, Type.MechHits } },
            { Type.ExtractorValue, new Type[] { Type.BuildingHits, Type.MechResilience } },
        };

        private static bool IsMech(Type type) => type.ToString().StartsWith("Mech");
        private const double _avgTypeCost = (double)Type.Mech;
        // int value is used as relative cost
        public enum Type
        {
            CoreShields = 100,

            Mech = 169,
            MechVision = 106,
            MechMove = 127,
            MechHits = 113,
            MechArmor = 124,
            MechResilience = 131,
            MechShields = 102,
            MechDamage = 115,
            MechAP = 138,
            MechSP = 120,
            MechRange = 139,

            Constructor = 250,
            ConstructorCost = 190,
            ConstructorMove = 240,
            ConstructorDefense = 230,
            ConstructorRepair = 270,

            Turret = 200,
            TurretAutoRepair = 285,
            TurretDefense = 110,
            TurretAttack = 130,
            TurretRange = 140,

            Factory = 300,
            FactoryAutoRepair = 205,
            FactoryRepair = 165,
            FactoryConstructor = 350,

            ExtractorAutoRepair = 245,
            ExtractorValue = 335,

            BuildingCost = 135,
            BuildingHits = 125,
        }
    }
}
