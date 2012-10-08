using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace SpaceRunner
{
    [Serializable]
    internal class Replay
    {
        [NonSerialized]
        private const int NoLength = -1;

        internal readonly uint[] Seed;

        private Dictionary<int, float> input = null;
        private HashSet<int> turbo = null;
        private Dictionary<int, Point?> fire = null;

        private int length = NoLength;

        [NonSerialized]
        private float lastInput;
        private bool lastTurbo;

        internal Replay(uint[] Seed)
        {
            this.Seed = Seed;

            this.input = new Dictionary<int, float>();
            this.turbo = new HashSet<int>();
            this.fire = new Dictionary<int, Point?>();

            this.length = NoLength;
        }

        internal int Length
        {
            get
            {
                if (length == NoLength)
                    GetLength();
                return length;
            }
        }
        private void GetLength()
        {
            foreach (int shortTick in this.input.Keys)
                this.length = Math.Max(length, shortTick);
            foreach (int shortTick in this.turbo)
                this.length = Math.Max(length, shortTick);
            foreach (int shortTick in this.fire.Keys)
                this.length = Math.Max(length, shortTick);
        }

        internal void Record(int tickCount, float inputAngle, bool turbo, Point? fire)
        {
            float input = inputAngle;
            if (this.lastInput != input)
            {
                this.lastInput = input;
                this.input.Add(tickCount, input);
            }

            RecordBool(tickCount, this.turbo, ref lastTurbo, turbo);

            if (fire.HasValue)
                this.fire.Add(tickCount, fire);
        }

        internal void EndRecord(int tickCount)
        {
            this.length = tickCount;
        }

        internal void Play(int tickCount, ref float inputAngle, ref bool turbo, ref Point? fire)
        {
            float saved;
            if (this.input.TryGetValue(tickCount, out saved))
                inputAngle = saved;

            PlayBool(tickCount, this.turbo, ref turbo);

            this.fire.TryGetValue(tickCount, out fire);
        }

        private static void RecordBool(int tickCount, HashSet<int> save, ref bool last, bool incoming)
        {
            if (last != incoming)
            {
                last = incoming;
                save.Add(tickCount);
            }
        }

        private static void PlayBool(int tickCount, HashSet<int> save, ref bool value)
        {
            if (save.Contains(tickCount))
                value = !value;
        }
    }
}
