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
        [NonSerialized]
        private const ushort NoLength = default(ushort);

        public readonly uint[] Seed;

        private Dictionary<ushort, Tuple<short, short>> input = null;
        private HashSet<ushort> turbo = null, fire = null;

        private ushort length = NoLength;

        [NonSerialized]
        private Tuple<short, short> lastInput;
        [NonSerialized]
        private bool lastTurbo, lastFire;

        public Replay(uint[] Seed)
        {
            this.Seed = Seed;

            this.input = new Dictionary<ushort, Tuple<short, short>>();
            this.turbo = new HashSet<ushort>();
            this.fire = new HashSet<ushort>();

            this.length = NoLength;
        }

        public int Length
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
            foreach (ushort shortTick in this.input.Keys)
                this.length = Math.Max(length, shortTick);
            foreach (ushort shortTick in this.turbo)
                this.length = Math.Max(length, shortTick);
            foreach (ushort shortTick in this.fire)
                this.length = Math.Max(length, shortTick);
        }

        public void Record(int tickCount, int inputX, int inputY, bool turbo, bool fire)
        {
            ushort shortTick = (ushort)tickCount;

            Tuple<short, short> input = new Tuple<short, short>((short)inputX, (short)inputY);
            if (this.lastInput != input)
            {
                this.lastInput = input;
                this.input.Add(shortTick, input);
            }

            RecordBool(shortTick, this.turbo, ref lastTurbo, turbo);
            RecordBool(shortTick, this.fire, ref lastFire, fire);
        }

        public void EndRecord(int tickCount)
        {
            this.length = (ushort)tickCount;
        }

        public void Play(int tickCount, ref int inputX, ref int inputY, ref bool turbo, ref bool fire)
        {
            ushort shortTick = (ushort)tickCount;

            Tuple<short, short> input;
            if (this.input.TryGetValue(shortTick, out input))
            {
                inputX = input.Item1;
                inputY = input.Item2;
            }

            PlayBool(shortTick, this.turbo, ref turbo);
            PlayBool(shortTick, this.fire, ref fire);
        }

        private static void RecordBool(ushort shortTick, HashSet<ushort> save, ref bool last, bool incoming)
        {
            if (last != incoming)
            {
                last = incoming;
                save.Add(shortTick);
            }
        }

        private static void PlayBool(ushort shortTick, HashSet<ushort> save, ref bool value)
        {
            if (save.Contains(shortTick))
                value = !value;
        }
    }
}
