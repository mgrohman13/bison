using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1
{
    [Serializable]
    [DataContract(IsReference = true)]
    internal class EnemyResearch : IResearch
    {
        private const Type NoType = Type.BuildingCost;
        internal const Type PortalType = Type.BuildingDefense;

        public Game Game { get; private set; }

        private Type _type;
        //private double _research;
        private double _difficulty;

        private readonly double[] _startMult, _multsMult, _startMake, _multsMake;

        private readonly Dictionary<Type, int> _unlockTurns;

        internal EnemyResearch()
        {
            //Type.MechAttack
            //Type.MechEnergyWeapons
            //Type.MechLasers
            //Type.MechExplosives
            //Type.MechRange
            //Type.MechDefense
            //Type.MechShields
            //Type.MechArmor
            //Type.MechMove
            //Type.MechResilience
            _startMult = [1.4, 1.1, 0.6, 1.0, 0.5, 0.7, 1.2, 1.0, 1.3, 1.0];
            _multsMult = [1.2, 1.0, 0.8, 0.7, 0.9, 1.0, 0.6, 1.1, 0.2, 0.4];

            //Type.MechEnergyWeapons
            //Type.MechLasers
            //Type.MechExplosives
            //Type.MechRange
            //Type.MechShields
            //Type.MechArmor
            _startMake = [.91, .13, .01, .52, 1.0, .39];
            _multsMake = [.91, .39, .65, .52, .78, 1.0];

            for (int a = 0; a < _startMult.Length; a++)
                _startMult[a] = Game.Rand.GaussianCapped(_startMult[a], .21, .1);
            for (int a = 0; a < _multsMult.Length; a++)
                _multsMult[a] = Game.Rand.GaussianCapped(_multsMult[a], .26, .1);

            for (int a = 0; a < _startMake.Length; a++)
                _startMake[a] = Game.Rand.GaussianCapped(_startMake[a], .169);
            for (int a = 0; a < _multsMake.Length; a++)
                _multsMake[a] = Game.Rand.GaussianCapped(_multsMake[a], .13, .1);
        }
        public EnemyResearch(Game game)
        {
            Game = game;
            _type = NoType;
            //_research = 0;
            _difficulty = 1;

            _unlockTurns = GenUnlockTurns(game.Consts);
        }
        private static Dictionary<Type, int> GenUnlockTurns(Consts consts)
        {
            Type[] Skips = [Type.ConstructorCost, Type.ConstructorDefense, Type.ConstructorMove];
            //in order of liklihood
            Type[] unlocks = [ Type.MechEnergyWeapons, Type.MechShields, PortalType, Skips[0],
                Type.MechRange, Type.MechArmor, Skips[1], Type.MechLasers, Type.MechExplosives, Skips[2] ];
            int count = unlocks.Length;
            Dictionary<Type, int> chances = unlocks.ToDictionary(t => t, t =>
                Game.Rand.Round(Math.Pow(1.69, count - Array.IndexOf(unlocks, t))));

            Dictionary<Type, int> result = [];
            for (int a = 0; a < count; a++)
            {
                Type next;
                do next = Game.Rand.SelectValue(chances);
                while (Skips.Contains(next) && a < Game.Rand.Next(count));
                chances.Remove(next);

                if (!Skips.Contains(next))
                {
                    double avg = (a + 1) * consts.EnemyUnlockTurns / count;
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
            //_research += Game.Rand.OE(difficulty);
            _difficulty = difficulty;
        }

        internal bool TypeVailable(Type type) => !_unlockTurns.ContainsKey(type) || _unlockTurns[type] < Game.Turn;
        public int GetBlueprintLevel() => Game.Rand.Round(Game.Consts.ResearchFactor * (_difficulty - 1));// + _research);
        public int GetMinCost() => Game.Rand.Round(Math.Pow(GetBlueprintLevel() + 7.8 * Game.Consts.ResearchFactor, 0.65));
        public int GetMaxCost() => Game.Rand.Round(Math.Pow(GetBlueprintLevel() + .39 * Game.Consts.ResearchFactor, 1.04)) + 390;

        public double GetMult(Type type, double pow)
        {
            double start = 1, mult = 1;
            switch (type)
            {
                case Type.MechAttack:
                    start = _startMult[0];
                    mult = _multsMult[0];
                    break;
                case Type.MechEnergyWeapons:
                    start = _startMult[1];
                    mult = _multsMult[1];
                    break;
                case Type.MechLasers:
                    start = _startMult[2];
                    mult = _multsMult[2];
                    break;
                case Type.MechExplosives:
                    start = _startMult[3];
                    mult = _multsMult[3];
                    break;
                case Type.MechRange:
                    start = _startMult[4];
                    mult = _multsMult[4];
                    break;
                case Type.MechDefense:
                    start = _startMult[5];
                    mult = _multsMult[5];
                    break;
                case Type.MechShields:
                    start = _startMult[6];
                    mult = _multsMult[6];
                    break;
                case Type.MechArmor:
                    start = _startMult[7];
                    mult = _multsMult[7];
                    break;
                case Type.MechMove:
                    start = _startMult[8];
                    mult = _multsMult[8];
                    break;
                case Type.MechResilience:
                    start = _startMult[9];
                    mult = _multsMult[9];
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
                    start = _startMake[0];
                    mult = _multsMake[0];
                    break;
                case Type.MechLasers:
                    start = _startMake[1];
                    mult = _multsMake[1];
                    break;
                case Type.MechExplosives:
                    start = _startMake[2];
                    mult = _multsMake[2];
                    break;
                case Type.MechRange:
                    start = _startMake[3];
                    mult = _multsMake[3];
                    break;
                case Type.MechShields:
                    start = _startMake[4];
                    mult = _multsMake[4];
                    break;
                case Type.MechArmor:
                    start = _startMake[5];
                    mult = _multsMake[5];
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
