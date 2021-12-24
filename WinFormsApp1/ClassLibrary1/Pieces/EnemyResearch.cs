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

        private Type type;
        private double _research;
        private double _difficulty;
        private readonly HashSet<Type> available;
        private static readonly Type[] Unlocks = new Type[] { Type.MechShields, Type.MechSP, Type.MechAP, Type.MechArmor };

        public EnemyResearch(Game game)
        {
            this.Game = game;
            this._research = 0;
            this.available = new HashSet<Type>();
        }

        public void EndTurn(double difficulty)
        {
            this.type = Game.Rand.SelectValue(Enum.GetValues<Type>().Where(t => t != Type.Mech && t.ToString().StartsWith("Mech") && (available.Contains(t) || !Unlocks.Contains(t))));
            this._research += Game.Rand.OE(difficulty);
            this._difficulty = difficulty;

            if (available.Count < Unlocks.Length)
            {
                double cost = Math.Pow(1.3, available.Count) * Math.Pow(available.Count + 1, 1.3) * 1.69;
                if (_research > cost * Game.Rand.Gaussian(2.1, .13))
                {
                    _research -= cost;
                    while (!available.Add(Game.Rand.SelectValue(Unlocks))) ;
                }
            }
        }

        public double GetLevel()
        {
            return Consts.ResearchFactor * (_difficulty - 1);
        }

        public double GetMinCost()
        {
            return Math.Pow(GetLevel() + 6.5 * Consts.ResearchFactor, .65);
        }
        public double GetMaxCost()
        {
            return Math.Pow(GetLevel() + .078 * Consts.ResearchFactor, 1.3);
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
            return (available.Contains(type) || !Unlocks.Contains(type)) && Game.Rand.Bool((1.3 + difficulty) / (5.2 + difficulty));
        }

        Research.Type IResearch.GetType()
        {
            return type;
        }
    }
}
