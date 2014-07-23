using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace SpaceRunner
{
    internal class Replay
    {
        internal readonly uint[] Seed;

        private readonly Dictionary<int, float> input = new Dictionary<int, float>();
        private readonly HashSet<int> turbo = new HashSet<int>();
        private readonly Dictionary<int, float> fire = new Dictionary<int, float>();

        private int length = -1;

        private float lastInput = float.NaN;
        private bool lastTurbo = false;

        internal Replay(uint[] Seed)
        {
            this.Seed = Seed;
        }

        internal int Length
        {
            get
            {
                return length;
            }
        }

        internal void Record(int tickCount, float inputAngle, bool turbo, float? fire)
        {
            if (this.lastInput != inputAngle)
            {
                this.lastInput = inputAngle;
                this.input.Add(tickCount, inputAngle);
            }

            if (this.lastTurbo != turbo)
            {
                this.lastTurbo = turbo;
                this.turbo.Add(tickCount);
            }

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

            if (this.turbo.Contains(tickCount))
                turbo = !turbo;

            if (this.fire.TryGetValue(tickCount, out saved))
                fire = saved;
            else
                fire = null;
        }

        internal void Save(string path)
        {
            TBSUtil.SaveGame(new ReplaySave(this), path);
        }
        internal static Replay Load(string path)
        {
            return new Replay(TBSUtil.LoadGame<ReplaySave>(path));
        }
        private Replay(ReplaySave save)
        {
            this.Seed = save.Seed;

            for (int a = 0 ; a < save.input.Length ; ++a)
                if (!float.IsNaN(save.input[a]))
                    this.input.Add(a, save.input[a]);

            this.turbo = new HashSet<int>(save.turbo);

            for (int b = 0 ; b < save.fire1.Length ; ++b)
                this.fire.Add(save.fire1[b], save.fire2[b]);

            this.length = save.input.Length;
        }
        //cut down on replay file size by saving a simplified data structure consisting of only primitive arrays
        [Serializable]
        private class ReplaySave
        {
            internal readonly uint[] Seed;
            internal readonly float[] input;
            internal readonly int[] turbo;
            internal readonly int[] fire1;
            internal readonly float[] fire2;

            internal ReplaySave(Replay replay)
            {
                this.Seed = replay.Seed;

                this.input = new float[replay.Length];
                for (int a = 0 ; a < replay.Length ; ++a)
                {
                    float value;
                    if (!replay.input.TryGetValue(a, out value))
                        value = float.NaN;
                    this.input[a] = value;
                }

                this.turbo = replay.turbo.ToArray();

                this.fire1 = new int[replay.fire.Count];
                this.fire2 = new float[replay.fire.Count];
                int b = 0;
                foreach (var pair in replay.fire)
                {
                    this.fire1[b] = pair.Key;
                    this.fire2[b] = pair.Value;
                    b++;
                }
            }
        }
    }
}
