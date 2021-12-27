using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1.Pieces
{
    [Serializable]
    internal class EnemyResearch : IResearch
    {
        public Game Game { get; private set; }

        private Type _type;
        private double _research;
        private double _difficulty;

        private readonly HashSet<Type> _available;

        private static readonly Type[] Unlocks = new Type[] { Type.MechShields, Type.MechSP, Type.MechAP, Type.MechArmor };

        public EnemyResearch(Game game)
        {
            this.Game = game;
            this._type = Type.BuildingCost;
            this._research = 0;
            this._difficulty = 1;
            this._available = new HashSet<Type>();
        }

        public void EndTurn(double difficulty)
        {
            if (Game.Rand.Bool())
                this._type = Game.Rand.Bool() ? Type.BuildingCost : Game.Rand.SelectValue(Enum.GetValues<Type>()
                    .Where(t => t != Type.Mech && Research.IsMech(t) && (_available.Contains(t) || !Unlocks.Contains(t))));
            this._research += Game.Rand.OE(difficulty);
            this._difficulty = difficulty;

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
            return Game.Rand.Round(Math.Pow(GetLevel() + 6.5 * Consts.ResearchFactor, 0.65));
        }
        public int GetMaxCost()
        {
            return Game.Rand.Round(Math.Pow(GetLevel() + 0.39 * Consts.ResearchFactor, 1.04));
        }

        public double GetMult(Research.Type type, double pow)
        {
            double mult = 1, add = 0;
            switch (type)
            {
                case Type.MechSP:
                case Type.MechAP:
                    mult = .91;
                    add = .13;
                    break;
                case Type.MechArmor:
                    add = .169;
                    break;
                case Type.MechDamage:
                    add = .39;
                    break;
                case Type.MechHits:
                    mult = 1.3;
                    add = .52;
                    break;
                case Type.MechMove:
                    mult = .52;
                    add = .21;
                    break;
                case Type.MechRange:
                    mult = .78;
                    add = .65;
                    break;
                case Type.MechResilience:
                    mult = .65;
                    break;
                case Type.MechShields:
                    add = .26;
                    break;
                case Type.MechVision:
                    mult = 0;
                    add = -1;
                    break;
                default: throw new Exception();
            }
            return Math.Pow(add + mult * (_difficulty - 1) + 1, pow);
        }

        public bool MakeType(Research.Type type)
        {
            double mult = 1, add = 0;
            switch (type)
            {
                case Type.MechSP:
                case Type.MechAP:
                    mult = 1.3;
                    add = -.91;
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

        Research.Type IResearch.GetType()
        {
            return _type;
        }
    }
}
