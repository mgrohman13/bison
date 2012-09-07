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

        private Dictionary<int, Tuple<short, short>> input = null;
        private HashSet<int> turbo = null, fire = null;

        private int length = NoLength;

        [NonSerialized]
        private Tuple<short, short> lastInput;
        [NonSerialized]
        private bool lastTurbo, lastFire;

        internal Replay(uint[] Seed)
        {
            this.Seed = Seed;

            this.input = new Dictionary<int, Tuple<short, short>>();
            this.turbo = new HashSet<int>();
            this.fire = new HashSet<int>();

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
            foreach (int shortTick in this.fire)
                this.length = Math.Max(length, shortTick);
        }

        internal void Record(int tickCount, int inputX, int inputY, bool turbo, bool fire)
        {
            var input = new Tuple<short, short>((short)inputX, (short)inputY);
            if (this.lastInput != input)
            {
                this.lastInput = input;
                this.input.Add(tickCount, input);
            }

            RecordBool(tickCount, this.turbo, ref lastTurbo, turbo);
            RecordBool(tickCount, this.fire, ref lastFire, fire);
        }

        internal void EndRecord(int tickCount)
        {
            this.length = tickCount;
        }

        internal void Play(int tickCount, ref int inputX, ref int inputY, ref bool turbo, ref bool fire)
        {
            Tuple<short, short> input;
            if (this.input.TryGetValue(tickCount, out input))
            {
                inputX = input.Item1;
                inputY = input.Item2;
            }

            PlayBool(tickCount, this.turbo, ref turbo);
            PlayBool(tickCount, this.fire, ref fire);
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
