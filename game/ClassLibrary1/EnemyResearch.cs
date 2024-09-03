using System;
using System.Collections.Generic;
using System.Linq;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1
{
    [Serializable]
    internal class EnemyResearch : IResearch
    {
        private const Type NoType = Type.BuildingCost;
        internal const Type PortalType = Type.BuildingDefense;

        public Game Game { get; private set; }

        private Type _type;
        private double _research;
        private double _difficulty;

        private readonly Dictionary<Type, int> _unlockTurns;

        internal EnemyResearch()
        {
        }
        public EnemyResearch(Game game)
        {
            Game = game;
            _type = NoType;
            _research = 0;
            _difficulty = 1;

            _unlockTurns = GenUnlockTurns();
        }
        private static Dictionary<Type, int> GenUnlockTurns()
        {
            Type[] Skips = new[] { Type.ConstructorCost, Type.ConstructorDefense, Type.ConstructorMove };
            //in order of liklihood
            Type[] unlocks = new Type[] { Type.MechEnergyWeapons, Type.MechShields, PortalType, Skips[0],
                Type.MechRange, Type.MechArmor, Skips[1], Type.MechLasers, Type.MechExplosives, Skips[2] };
            int count = unlocks.Length;
            Dictionary<Type, int> chances = unlocks.ToDictionary(t => t, t =>
                Game.Rand.Round(Math.Pow(1.69, count - Array.IndexOf(unlocks, t))));

            Dictionary<Type, int> result = new();
            for (int a = 0; a < count; a++)
            {
                Type next;
                do next = Game.Rand.SelectValue(chances);
                while (Skips.Contains(next) && a < Game.Rand.Next(count));
                chances.Remove(next);

                if (!Skips.Contains(next))
                {
                    double avg = (a + 1) * Consts.EnemyUnlockTurns / count;
                    double dev = (1 + count - a) * .39 / (count + 1);
                    if (avg < 13) throw new Exception();
                    int min = Game.Rand.RangeInt(Game.Rand.RangeInt(1, 13), Game.Rand.RangeInt(13, Game.Rand.Round(avg / Math.PI)));
                    int value = Game.Rand.GaussianOEInt(avg, dev, dev / Math.E, min);

                    result.Add(next, value);
                }
            }
            return result;
        }

        public void EndTurn(double difficulty)
        {
            if (Game.Rand.Bool())
                _type = Game.Rand.Bool() ? NoType : Game.Rand.SelectValue(Enum.GetValues<Type>()
                    .Where(t => Research.IsMech(t) && TypeVailable(t)));
            _research += Game.Rand.OE(difficulty);
            _difficulty = difficulty;
        }

        internal bool TypeVailable(Type type) => !_unlockTurns.ContainsKey(type) || _unlockTurns[type] < Game.Turn;
        public int GetBlueprintLevel() => Game.Rand.Round(Consts.ResearchFactor * (_difficulty - 1) + _research);
        public int GetMinCost() => Game.Rand.Round(Math.Pow(GetBlueprintLevel() + 7.8 * Consts.ResearchFactor, 0.65));
        public int GetMaxCost() => Game.Rand.Round(Math.Pow(GetBlueprintLevel() + 0.169 * Consts.ResearchFactor, 1.04)) + 390;

        public double GetMult(Type type, double pow)
        {
            double start = 1, mult = 1;
            switch (type)
            {
                case Type.MechAttack:
                    start = 1.4;
                    mult = 1.2;
                    break;
                case Type.MechEnergyWeapons:
                    start = 1.1;
                    break;
                case Type.MechLasers:
                    start = 0.6;
                    mult = 0.8;
                    break;
                case Type.MechExplosives:
                    mult = 0.7;
                    break;
                case Type.MechRange:
                    start = 0.5;
                    mult = 0.9;
                    break;
                case Type.MechDefense:
                    start = 0.7;
                    break;
                case Type.MechShields:
                    start = 1.2;
                    mult = 0.6;
                    break;
                case Type.MechArmor:
                    mult = 1.1;
                    break;
                case Type.MechMove:
                    start = 1.3;
                    mult = 0.2;
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

        bool IResearch.HasType(Type type) => TypeVailable(type);
        public bool MakeType(Type type)
        {
            double start, mult;
            switch (type)
            {
                case Type.MechEnergyWeapons:
                    start = .91;
                    mult = .91;
                    break;
                case Type.MechLasers:
                    start = .13;
                    mult = .39;
                    break;
                case Type.MechExplosives:
                    start = 0;
                    mult = .65;
                    break;
                case Type.MechRange:
                    start = .52;
                    mult = .52;
                    break;
                case Type.MechShields:
                    start = 1;
                    mult = .78;
                    break;
                case Type.MechArmor:
                    start = .39;
                    mult = 1;
                    break;
                default: throw new Exception();
            }

            double difficulty = start + mult * (_difficulty - 1);
            return TypeVailable(type) && Game.Rand.Bool(.78 * Math.Pow(difficulty / (difficulty + 1), 1.3));
        }

        Type IResearch.GetType()
        {
            return _type;
        }
    }
}
