using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Enemy : Piece
    {
        private float rate, sizeRate, densityRate;

        public Enemy(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, Color.Black)
        {
            Console.WriteLine("new Enemy");
            this.rate = GetRate();
            this.sizeRate = 0;
            this.densityRate = density;
        }

        private static float GetRate()
        {
            return Game.rand.Weighted(.025f);
        }

        public static Color GetColor(float size, float density, bool grey)
        {
            float avgMass = GetMass(Game.avgSize, 1) * 1.5f;
            float mass = GetMass(size, density);
            int scale = Game.rand.Round(255f * Math.Pow(avgMass / (avgMass + mass), grey ? 1.75 : 1.25));
            return Color.FromArgb(grey ? scale : Game.rand.Round((scale + 255) / 2f), scale, grey ? scale : 0);
        }
        public float GetDmg()
        {
            return this.Mass / GetMass(Game.avgSize, 1);
        }

        internal override void Step(float count)
        {
            base.Step(count);

            if (Game.rand.Bool(rate))
            {
                this.density = this.density * (1 - rate) + rate * densityRate;
                densityRate = densityRate * (1 - rate) + rate * Game.GetDensity(size);

                this.size += sizeRate;
                if (size > 0)
                {
                    float inc = Game.rand.GaussianFloat();
                    inc = FlipInc(inc, size, Game.avgSize);
                    inc = FlipInc(inc, count, game.EnemyMult());
                    float sqrt = sizeRate + inc;
                    if (sqrt < 0)
                        sqrt = (float)-(Math.Pow(-sqrt + 1, .75) - 1);
                    else
                        sqrt = (float)(Math.Pow(sqrt + 1, .75) - 1);
                    sizeRate = sqrt;

                    rate = (rate + GetRate()) / 2f;

                    //Console.WriteLine("{0}\t{1}\t{2}", color.R.ToString().PadLeft(3), sizeRate.ToString("+0.######;-0.######").PadRight(11), densityRate);
                }
                else
                {
                    game.Remove(this);
                }
            }

            if (Game.ExistChance(this.Mass, GetMass(Game.avgSize, 1), 2.5f))
                game.Remove(this);
            else
                color = GetColor(size, density, GetDmg() < game.Player.Shield);
        }

        private static float FlipInc(float inc, float value, float target)
        {
            if (((value < target) == (inc < 0)) && Game.rand.Bool(Math.Abs(value - target) / (value + target)))
                inc *= -1;
            return inc;
        }

        internal override float GetGravity(Type type)
        {
            if (typeof(Center).IsAssignableFrom(type))
                return 2;
            if (typeof(Enemy).IsAssignableFrom(type))
                return 1;
            if (typeof(Player).IsAssignableFrom(type))
                return 1 / 2f;
            if (typeof(PowerUp).IsAssignableFrom(type))
                return 1;
            if (typeof(Target).IsAssignableFrom(type))
                return (float)(1 / 2.0 / game.DiffMult(.6f, .7f));
            throw new Exception();
        }
    }
}
