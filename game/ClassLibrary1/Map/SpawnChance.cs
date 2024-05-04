using System;
using System.Diagnostics;

namespace ClassLibrary1.Map
{
    public partial class Map
    {
        [Serializable]
        internal class SpawnChance
        {
            private int chance, target;
            private double rate;

            public int Chance => chance;

            public SpawnChance()
            {
                chance = GenValue();
                target = GenValue();
                rate = Game.Rand.Gaussian();
                SetTarget(-10);
            }

            public void Turn(int turn)
            {
                double avg = chance + rate;
                if (avg > 1)
                {
                    int sign = Math.Sign(target - chance);
                    chance = Game.Rand.GaussianCappedInt(avg, rate / avg / 2.6, 1);
                    if (sign != Math.Sign(target - chance) || Game.Rand.Next(52) == 0)
                        SetTarget(turn);
                    else
                        SetRate(turn, target, 65);
                }
                else
                {
                    chance = 1 + Game.Rand.OEInt(Math.Abs(avg));
                    SetTarget(turn);
                }
            }
            private void SetTarget(int turn)
            {
                double prev = target;

                target = Game.Rand.Round((target + 2 * GenValue()) / 3.0);
                SetRate(turn, target, 1);

                int sign = Math.Sign(target - chance);
                if (sign != 0)
                    rate = sign * (1 + Math.Abs(rate));

                SetRate(turn, GenValue(), 4);

                Debug.WriteLine($"SpawnChance.SetTarget({turn}): {chance} {(rate > 0 ? "+" : "")}{rate:0.0} -> {target} ({prev})");
            }

            private void SetRate(int turn, int value, double prevWeight)
            {
                double diff = value - chance;
                double time = Game.Rand.GaussianOEInt((turn + 3900) / (double)(turn + 169), .13, .13, 1);
                double newRate = diff / time;

                int factor = GenValue() + GenValue();
                newRate *= factor / (factor + Math.Abs(diff));

                double newWeight = 1 + Game.Rand.OE();
                newRate = (newRate * newWeight + rate * prevWeight) / (newWeight + prevWeight);

                rate = newRate + Game.Rand.Gaussian();
            }

            private static int GenValue() => Game.Rand.GaussianOEInt(650, .78, .26, 1);

            internal void Spawned() => Mult(1 / 3.9);
            internal void Mult(double mult)
            {
                double add = 0;
                if (mult > 1)
                {
                    mult = (mult - 1) / 2.0;
                    add = GenValue() * mult;
                    mult++;
                }
                int M(double v) => Game.Rand.Round(v * mult + add);
                this.chance = M(chance);
                if (Game.Rand.Bool())
                    this.target = M(target);
            }
        }
    }
}
