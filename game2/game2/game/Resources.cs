using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace game2.game
{
    public struct Resources : IEnumerable<int>
    {
        public const int NumResources = 4;

        private int _basic; // Food?
        private int _advanced; // Alloys?
        private int _mobility; // Gems?
        private int _special; // Favor
        private int _research; //  
        private int _upkeep; //  

        public Resources(Resources init) : this(init.ToArray()) { }
        public Resources(params int[] init)
        {
            for (int a = 0; a < NumResources; a++)
                this[a] = init[a];
        }

        public int Basic
        {
            readonly get => _basic;
            internal set => _basic = value;
        }
        public int Advanced
        {
            readonly get => _advanced;
            internal set => _advanced = value;
        }
        public int Mobility
        {
            readonly get => _mobility;
            internal set => _mobility = value;
        }
        public int Special
        {
            readonly get => _special;
            internal set => _special = value;
        }
        public int Research
        {
            readonly get => _research;
            internal set => _research = value;
        }
        public int Upkeep
        {
            readonly get => _upkeep;
            internal set => _upkeep = value;
        }

        public readonly float GetValue(Game game)
        {
            float result = 0;
            for (int a = 0; a < NumResources; a++)
                result += this[a] * game.Consts.ResourceValue[a];
            return result;
        }

        // Indexer: 0 => Basic, 1 => Advanced, 2 => Mobility, 3 => Special
        public int this[int index]
        {
            readonly get => index switch
            {
                0 => _basic,
                1 => _advanced,
                2 => _mobility,
                3 => _special,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Valid indices are 0..3")
            };
            internal set
            {
                switch (index)
                {
                    case 0: _basic = value; break;
                    case 1: _advanced = value; break;
                    case 2: _mobility = value; break;
                    case 3: _special = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Valid indices are 0..3");
                }
            }
        }

        readonly IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            var _this = this;
            return Enumerable.Range(0, NumResources).Select(a => _this[a]).GetEnumerator();
        }
        readonly IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<int>)this).GetEnumerator();

        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Resources r && this == r;
        public static bool operator >(Resources a, Resources b) => Combine(c => a[c] > b[c]);
        public static bool operator <(Resources a, Resources b) => Combine(c => a[c] < b[c]);
        public static bool operator >=(Resources a, Resources b) => Combine(c => a[c] >= b[c]);
        public static bool operator <=(Resources a, Resources b) => Combine(c => a[c] <= b[c]);
        public static bool operator ==(Resources a, Resources b) => Combine(c => a[c] == b[c]);
        public static bool operator !=(Resources a, Resources b) => !(a == b);
        private static bool Combine(Func<int, bool> Operator)
        {
            for (int i = 0; i < NumResources; i++)
                if (!Operator(i))
                    return false;
            return true;
        }

        public static Resources operator +(Resources a, Resources b) => Combine(c => a[c] + b[c]);
        public static Resources operator -(Resources a, Resources b) => Combine(c => a[c] - b[c]);
        public static Resources operator *(Resources a, int b) => Combine(c => a[c] * b);
        private static Resources Combine(Func<int, int> Operator)
        {
            Resources result = new();
            for (int i = 0; i < NumResources; i++)
                result[i] = Operator(i);
            return result;
        }

        public override readonly int GetHashCode() => HashCode.Combine(_basic, _advanced, _mobility, _special);
    }
}
