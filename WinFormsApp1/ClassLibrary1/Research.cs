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
    public class Research
    {
        public readonly Game Game;

        private Type _researching;
        private readonly Dictionary<Type, double> _research;
        private double _researchCur;
        //private double _researchLast;
        private double _researchNext;
        private double _nextAvg;

        private readonly HashSet<Type> _hasTypes;

        public IReadOnlyCollection<Type> Available => _research.Keys;
        public double ResearchCur => _researchCur;
        //public double ResearchLast => _researchLast;
        public double ResearchNext => _researchNext;

        public Research(Game Game)
        {
            this.Game = Game;

            this._researching = Type.Mech;
            this._research = new() { { _researching, 0 } };
            this._researchCur = 0;
            //this._researchLast = 0;
            this._researchNext = 25;
            this._nextAvg = 26;

            this._hasTypes = new();
        }

        internal Type? AddResearch(double research)
        {
            Type? result = null;

            const int choices = 3;

            this._researchCur += research;
            this._research[_researching] += research;

            if (this._research[_researching] >= ResearchNext)
            {
                _hasTypes.Add(_researching);
                Game.Player.OnResearch(_researching, GetResearchMult(ResearchNext));

                result = _researching;
                _research.Clear();
                foreach (Type available in Game.Rand.Iterate(Enum.GetValues<Type>()))
                {
                    if (_hasTypes.Contains(available) && BaseTypes.Contains(available))
                        continue;
                    static bool IsMech(Type type) => type.ToString().StartsWith("Mech");
                    if (_research.Count == choices - 1 && (IsMech(available) ? _research.Keys.All(IsMech) : !_research.Keys.Any(IsMech)))
                        continue;
                    if (Dependencies[available].All(d => _hasTypes.Contains(d)))
                    {
                        _research.Add(available, ResearchCur);
                        if (_research.Count == 1)
                            _researching = available;
                        else if (_research.Count == choices)
                            break;
                    }
                }

                static double GetNext(double v) => Math.Pow(v * 2.6 + 130 - v, .65);
                double avg = GetNext(_nextAvg);
                this._nextAvg += avg;

                avg = (avg + GetNext(ResearchNext)) / 2.0;
                double min = ResearchCur - ResearchNext + Game.Rand.GaussianOE(30, .13, .13, 1);
                if (avg > min)
                {
                    double devDiv = Math.Pow(ResearchNext + avg, .21);
                    avg = Game.Rand.GaussianOE(avg, 1.3 / devDiv, 2.1 / devDiv / devDiv, min);
                    avg = (ResearchNext + avg + _nextAvg) / 2.0 - ResearchNext;
                }
                this._researchNext += Math.Max(avg, min);
            }

            return result;
        }
        private static double GetResearchMult(double research)
        {
            return (research + Consts.ResearchFactor) / Consts.ResearchFactor;
        }

        internal bool HasType(Type research)
        {
            return _hasTypes.Contains(research);
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
            { Type.MechRange, new Type[] { Type.MechDamage } },
            { Type.MechAP, new Type[] { Type.MechRange } },
            { Type.MechSP, new Type[] { Type.MechRange } },

            { Type.TurretDefense, new Type[] { Type.Turret, Type.MechHits } },
            { Type.TurretAttack, new Type[] { Type.Turret, Type.MechDamage, Type.MechAP, Type.MechSP } },
            { Type.TurretRange, new Type[] { Type.Turret, Type.MechRange } },

            { Type.ConstructorMove, new Type[] { Type.Constructor, Type.MechMove } },
            { Type.ConstructorDefense, new Type[] { Type.Constructor, Type.MechArmor } },
            { Type.ConstructorRepair, new Type[] { Type.Constructor, Type.ConstructorDefense, Type.Factory } },
            { Type.FactoryRepair, new Type[] { Type.Factory, Type.ConstructorRepair } },
            { Type.FactoryConstructor, new Type[] { Type.Factory, Type.FactoryRepair, Type.ConstructorMove } },

            { Type.BuildingCost, new Type[] { Type.CoreShields } },
            { Type.ExtractorAutoRepair, new Type[] { Type.BuildingCost } },
            { Type.FactoryAutoRepair, new Type[] { Type.Factory, Type.ExtractorAutoRepair } },
            { Type.TurretAutoRepair, new Type[] { Type.Turret, Type.FactoryAutoRepair, Type.TurretDefense } },
            { Type.ConstructorCost, new Type[] { Type.Constructor, Type.BuildingCost } },
            { Type.BuildingHits, new Type[] { Type.ConstructorCost, Type.Turret, Type.MechVision } },
            { Type.ExtractorValue, new Type[] { Type.BuildingHits, Type.MechResilience } },
        };

        public enum Type
        {
            CoreShields,

            Mech,
            MechVision,
            MechMove,
            MechHits,
            MechArmor,
            MechResilience,
            MechShields,
            MechDamage,
            MechAP,
            MechSP,
            MechRange,

            Constructor,
            ConstructorCost,
            ConstructorMove,
            ConstructorDefense,
            ConstructorRepair,

            Turret,
            TurretAutoRepair,
            TurretDefense,
            TurretAttack,
            TurretRange,

            Factory,
            FactoryAutoRepair,
            FactoryRepair,
            FactoryConstructor,

            ExtractorAutoRepair,
            ExtractorValue,

            BuildingCost,
            BuildingHits,
        }
    }
}
