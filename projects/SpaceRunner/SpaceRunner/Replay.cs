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

        public Dictionary<ushort, Tuple<short, short>> input;
        public HashSet<ushort> turbo, fire;

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
