using System;
using System.Collections.Generic;
using System.Linq;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1
{
    [Serializable]
    internal class EnemyResearch : IResearch
    {
        public Game Game { get; private set; }

        private Type _type;
        private double _research;
        private double _difficulty;

        private readonly HashSet<Type> _available;

        private static readonly Type[] Unlocks = new Type[] { Type.MechShields, Type.MechLasers, Type.MechExplosives, Type.MechArmor };

        public EnemyResearch(Game game)
        {
            Game = game;
            _type = Type.BuildingCost;
            _research = 0;
            _difficulty = 1;
            _available = new HashSet<Type>();
        }

        public void EndTurn(double difficulty)
        {
            if (Game.Rand.Bool())
                _type = Game.Rand.Bool() ? Type.BuildingCost : Game.Rand.SelectValue(Enum.GetValues<Type>()
                    .Where(t => Research.IsMech(t) && (_available.Contains(t) || !Unlocks.Contains(t))));
            _research += Game.Rand.OE(difficulty);
            _difficulty = difficulty;

            if (_available.Count < Unlocks.Length)
            {
                double cost = Math.Pow(1.3, _available.Count) * Math.Pow(_available.Count + 1, 1.3) * 1.69;
                if (_research > cost * Game.Rand.Gaussian(2.1, .13))
                {
                    _research -= cost;
                    while (!_available.Add(Game.Rand.SelectValue(Unlocks))) ;
                }
            }
        }

        public int GetLevel()
        {
            return Game.Rand.Round(Consts.ResearchFactor * (_difficulty - 1) + _research);
        }

        public int GetMinCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + 6.5 * Consts.ResearchFactor, .65));
        }
        public int GetMaxCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + .39 * Consts.ResearchFactor, 1.04));
        }

        public double GetMult(Type type, double pow)
        {
            double start = 1, mult = 1;
            switch (type)
            {
                case Type.MechAttack:
                    start = .61;
                    mult = .91;
                    break;
                case Type.MechEnergyWeapons:
                    start = 0.87;
                    mult = 1.13;
                    break;
                case Type.MechLasers:
                    start = 1.13;
                    mult = .91;
                    break;
                case Type.MechExplosives:
                    start = 1.13;
                    mult = .91;
                    break;
                case Type.MechRange:
                    start = 1.65;
                    mult = .78;
                    break;
                case Type.MechDefense:
                    start = 1.39;
                    mult = 1;
                    break;
                case Type.MechShields:
                    start = 1.26;
                    mult = 1;
                    break;
                case Type.MechArmor:
                    start = 1.169;
                    mult = 1;
                    break;
                case Type.MechMove:
                    start = 1.26;
                    mult = .52;
                    break;
                case Type.MechResilience:
                    start = 1;
                    mult = .65;
                    break;
                case Type.MechVision:
                    start = 1;
                    mult = 1;
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
            double mult = 1, add = 0;
            switch (type)
            {
                case Type.MechEnergyWeapons:
                    add = -.39;
                    break;
                case Type.MechLasers:
                case Type.MechExplosives:
                    mult = 1.3;
                    add = -.91;
                    break;
                case Type.MechRange:
                    mult = .78;
                    add = -.13;
                    break;
                case Type.MechArmor:
                    break;
                case Type.MechShields:
                    mult = .91;
                    add = .13;
                    break;
                default: throw new Exception();
            }
            double difficulty = add + mult * (_difficulty - 1);
            return (_available.Contains(type) || !Unlocks.Contains(type)) && Game.Rand.Bool((1.3 + difficulty) / (5.2 + difficulty));
        }

        Type IResearch.GetType()
        {
            return _type;
        }
    }
}
