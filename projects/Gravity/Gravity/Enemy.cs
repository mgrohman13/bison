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

        public Enemy(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, GetColor(size, density))
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

        public static Color GetColor(float size, float density)
        {
            float avgMass = getMass(Game.avgSize, 1);
            float mass = getMass(size, density);
            int scale = Game.rand.Round(255f * Math.Pow(avgMass / (avgMass + mass), 2.5));
            return Color.FromArgb(scale, scale, scale);
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
                    if (size < Game.avgSize == inc < 0 && Game.rand.Bool(Math.Abs(size - Game.avgSize) / (size + Game.avgSize)))
                        inc *= -1;
                    if (count < Game.avgNum == inc < 0 && Game.rand.Bool(Math.Abs(count - Game.avgNum) / (count + Game.avgNum)))
                        inc *= -1;
                    float sqrt = sizeRate + inc;
                    if (sqrt < 0)
                        sqrt = (float)-Math.Sqrt(-sqrt);
                    else
                        sqrt = (float)Math.Sqrt(sqrt);
                    sizeRate = sqrt;

                    this.color = GetColor(size, density);

                    rate = (rate + GetRate()) / 2f;

                    Console.WriteLine("{0}\t{1}\t{2}", color.R.ToString().PadLeft(3), sizeRate.ToString("+0.######;-0.######").PadRight(11), densityRate);
                }
                else
                {
                    game.Remove(this);
                }
            }

            if (Game.ExistChance(color.R / 255f))
                game.Remove(this);
        }

        internal override float GetGravity(Type type)
        {
            if (type == typeof(Center))
                return 2;
            if (type == typeof(Enemy))
                return 1;
            if (type == typeof(Player))
                return 1 / 2f;
            if (type == typeof(PowerUp))
                return 1;
            if (type == typeof(Target))
                return 1 / 4f;
            throw new Exception();
        }
    }
}
