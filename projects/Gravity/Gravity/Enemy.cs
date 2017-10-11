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
        float rate, sizeRate, densityRate;

        public Enemy(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, GetColor(size, density))
        {
            Console.WriteLine("new Enemy");
            this.rate = Game.rand.Weighted(.05f);
            this.sizeRate = 0;
            this.densityRate = 1;
        }

        private static Color GetColor(float size, float density)
        {
            float avgMass = getMass(Game.avgSize, 1);
            float mass = getMass(size, density);
            int scale = Game.rand.Round(255f * Math.Pow(avgMass / ( avgMass + mass ), 2.5));
            Console.WriteLine(scale);
            return Color.FromArgb(scale, scale, scale);
        }

        internal override void Step(float count)
        {
            base.Step(count);

            if (Game.rand.Bool(rate))
            {
                this.density = this.density * ( 1 - rate ) + rate * densityRate;
                densityRate = densityRate * ( 1 - rate ) + rate * Game.GetDensity(size);

                this.size += sizeRate;
                if (size > 0)
                {
                    float inc = Game.rand.GaussianFloat();
                    if (size < Game.avgSize == inc < 0 && Game.rand.Bool(Math.Abs(size - Game.avgSize) / ( size + Game.avgSize )))
                        inc *= -1;
                    if (count < Game.avgNum == inc < 0 && Game.rand.Bool(Math.Abs(count - Game.avgNum) / ( count + Game.avgNum )))
                        inc *= -1;
                    float sqrt = sizeRate + inc;
                    if (sqrt < 0)
                        sqrt = (float)-Math.Sqrt(-sqrt);
                    else
                        sqrt = (float)Math.Sqrt(sqrt);
                    sizeRate = sqrt;
                    Console.WriteLine(sizeRate);

                    this.color = GetColor(size, density);
                }
                else
                {
                    game.Remove(this);
                }
            }

            if (Game.rand.Bool(.025 * Math.Pow(color.R / 255f, 5)))
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
            if (type == typeof(Target))
                return 1 / 4f;
            throw new Exception();
        }
    }
}
