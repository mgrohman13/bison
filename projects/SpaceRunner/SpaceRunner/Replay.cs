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
        internal readonly uint[] Seed;

        private Dictionary<int, float> input = null;
        private HashSet<int> turbo = null;
        private Dictionary<int, float> fire = null;

        [NonSerialized]
        private int length;

        [NonSerialized]
        private float lastInput;
        [NonSerialized]
        private bool lastTurbo;

        internal Replay(uint[] Seed)
        {
            this.Seed = Seed;

            this.input = new Dictionary<int, float>();
            this.turbo = new HashSet<int>();
            this.fire = new Dictionary<int, float>();
        }

        internal int Length
        {
            get
            {
                if (length == default(int))
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

        internal void Record(int tickCount, float inputAngle, bool turbo, float? fire)
        {
            float input = inputAngle;
            if (this.lastInput != input)
            {
                this.lastInput = input;
                this.input.Add(tickCount, input);
            }

            RecordBool(tickCount, this.turbo, ref lastTurbo, turbo);

            if (fire.HasValue)
                this.fire.Add(tickCount, fire.Value);
        }

        internal void EndRecord(int tickCount)
        {
            this.length = tickCount;
        }

        internal void Play(int tickCount, ref float inputAngle, ref bool turbo, ref float? fire)
        {
            float saved;
            if (this.input.TryGetValue(tickCount, out saved))
                inputAngle = saved;

            PlayBool(tickCount, this.turbo, ref turbo);

            if (this.fire.ContainsKey(tickCount))
                fire = this.fire[tickCount];
            else
                fire = null;
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
