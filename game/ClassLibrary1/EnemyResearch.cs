using System;
using System.Collections.Generic;
using System.Linq;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1
{
    [Serializable]
    internal class EnemyResearch : IResearch
    {
        private const Type noType = Type.BuildingCost;

        public Game Game { get; private set; }

        private Type _type;
        private double _research;
        private double _difficulty;

        private readonly HashSet<Type> _available;

        //in order of liklihood
        private static readonly Type[] Unlocks = new Type[] { Type.MechEnergyWeapons, Type.MechShields, Type.MechRange, Type.MechArmor, Type.MechLasers, Type.MechExplosives, };

        public EnemyResearch(Game game)
        {
            Game = game;
            _type = noType;
            _research = 0;
            _difficulty = 1;
            _available = new HashSet<Type>();
        }

        public void EndTurn(double difficulty)
        {
            if (Game.Rand.Bool())
                _type = Game.Rand.Bool() ? noType : Game.Rand.SelectValue(Enum.GetValues<Type>()
                    .Where(t => Research.IsMech(t) && TypeVailable(t)));
            _research += Game.Rand.OE(difficulty);
            _difficulty = difficulty;

            if (_available.Count < Unlocks.Length)
            {
                double cost = Math.Pow(1.3, _available.Count) * Math.Pow(_available.Count + 1, 1.3) * 1.69;
                if (_research > Game.Rand.GaussianCapped(cost * 2.1, .13))
                {
                    _research -= cost;
                    while (!_available.Add(Game.Rand.SelectValue(Unlocks, t => Game.Rand.Round(Math.Pow(1 + Unlocks.Length - Array.IndexOf(Unlocks, t), 2.1))))) ;
                }
            }
        }

        private bool TypeVailable(Type type)
        {
            return (_available.Contains(type) || !Unlocks.Contains(type));
        }

        public int GetLevel()
        {
            return Game.Rand.Round(Consts.ResearchFactor * (_difficulty - 1) + _research);
        }

        public int GetMinCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + 7.8 * Consts.ResearchFactor, 0.65));
        }
        public int GetMaxCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + 0.169 * Consts.ResearchFactor, 1.04)) + 390;
        }

        public double GetMult(Type type, double pow)
        {
            double start = 1, mult = 1;
            switch (type)
            {
                case Type.MechAttack:
                    start = 1.5;
                    mult = 0.8;
                    break;
                case Type.MechEnergyWeapons:
                    start = 1.3;
                    mult = 1.0;
                    break;
                case Type.MechLasers:
                    start = 0.7;
                    mult = 1.5;
                    break;
                case Type.MechExplosives:
                    mult = 0.7;
                    break;
                case Type.MechRange:
                    start = 0.5;
                    mult = 0.9;
                    break;
                case Type.MechDefense:
                    start = 0.9;
                    mult = 1.1;
                    break;
                case Type.MechShields:
                    start = 1.2;
                    mult = 0.6;
                    break;
                case Type.MechArmor:
                    mult = 1.3;
                    break;
                case Type.MechMove:
                    start = 1.4;
                    mult = 0.5; //.4?
                    break;
                case Type.MechResilience:
                    mult = 0.4;
                    break;
                case Type.MechVision:
                    break;
                default: throw new Exception();
            }
            return Math.Pow(start + mult * (_difficulty - 1), pow);
        }

        bool IResearch.HasType(Type research)
        {
            return true;
        }
        public bool MakeType(Type type)
        {
            double start, mult;
            switch (type)
            {
                case Type.MechEnergyWeapons:
                    start = 1;
                    mult = .65;
                    break;
                case Type.MechLasers:
                    start = .13;
                    mult = 1.3;
                    break;
                case Type.MechExplosives:
                    start = 0;
                    mult = 1;
                    break;
                case Type.MechRange:
                    start = .52;
                    mult = .78;
                    break;
                case Type.MechShields:
                    start = 1.3;
                    mult = .91;
                    break;
                case Type.MechArmor:
                    start = .39;
                    mult = 1.13;
                    break;
                default: throw new Exception();
            }

            double difficulty = start + mult * (_difficulty - 1);
            return TypeVailable(type) && Game.Rand.Bool(.91 * Math.Pow(difficulty / (difficulty + 1), 1.3));
        }

        Type IResearch.GetType()
        {
            return _type;
        }
    }
}
