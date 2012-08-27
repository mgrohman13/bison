using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace SpaceRunner
{
    [Serializable]
    public class Replay
    {
        public readonly uint[] Seed;

        private Dictionary<int, Point> input;
        private HashSet<int> turbo, fire;

        [NonSerialized]
        private Point lastInput;
        [NonSerialized]
        private bool lastTurbo, lastFire;

        public Replay(uint[] Seed)
        {
            this.Seed = Seed;

            this.input = new Dictionary<int, Point>();
            this.turbo = new HashSet<int>();
            this.fire = new HashSet<int>();
        }

        public void Record(int tickCount, int inputX, int inputY, bool turbo, bool fire)
        {
            Point input = new Point(inputX, inputY);
            if (this.lastInput != input)
            {
                this.lastInput = input;
                this.input.Add(tickCount, input);
            }

            RecordBool(tickCount, this.turbo, ref lastTurbo, turbo);
            RecordBool(tickCount, this.fire, ref lastFire, fire);
        }

        private static void RecordBool(int tickCount, HashSet<int> save, ref bool last, bool incoming)
        {
            if (last != incoming)
            {
                last = incoming;
                save.Add(tickCount);
            }
        }

        public void Play(int tickCount, ref int inputX, ref int inputY, ref bool turbo, ref bool fire)
        {
            Point input;
            if (this.input.TryGetValue(tickCount, out input))
            {
                inputX = input.X;
                inputY = input.Y;
            }

            PlayBool(tickCount, this.turbo, ref turbo);
            PlayBool(tickCount, this.fire, ref fire);
        }

        private static void PlayBool(int tickCount, HashSet<int> save, ref bool value)
        {
            if (save.Contains(tickCount))
                value = !value;
        }
    }
}
