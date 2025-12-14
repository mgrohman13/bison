using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace game2.game
{
    public struct Resources : IEnumerable<int>
    {
        public const int NumResources = 6;
        public const int NumMapResources = 4;

        public const int BasicIdx = 0; // Food?
        public const int AdvancedIdx = 1; // Alloys?
        public const int MobilityIdx = 2; // Gems?
        public const int SpecialIdx = 3; // Favor
        public const int ResearchIdx = 4; //  
        public const int UpkeepIdx = 5; //  

        private int[] _resources;// = new int[NumResources];

        public Resources(Resources init) : this(init.ToArray()) { }
        public Resources(params int[] init)
        {
            _resources = new int[NumResources];
            for (int a = 0; a < init.Length; a++)
                this[a] = init[a];
        }
        //public Resources()
        //{
        //    _resources = new int[NumResources];
        //}

        public int Basic
        {
            get => this[BasicIdx];
            internal set => this[BasicIdx] = value;
        }
        public int Advanced
        {
            get => this[AdvancedIdx];
            internal set => this[AdvancedIdx] = value;
        }
        public int Mobility
        {
            get => this[MobilityIdx];
            internal set => this[MobilityIdx] = value;
        }
        public int Special
        {
            get => this[SpecialIdx];
            internal set => this[SpecialIdx] = value;
        }
        public int Research
        {
            get => this[ResearchIdx];
            internal set => this[ResearchIdx] = value;
        }
        public int Upkeep
        {
            get => this[UpkeepIdx];
            internal set => this[UpkeepIdx] = value;
        }

        public float GetValue(Game game)
        {
            float result = 0;
            for (int a = 0; a < NumResources; a++)
                result += this[a] * game.Consts.ResourceValue[a];
            return result;
        }

        // Indexer: 0 => Basic, 1 => Advanced, 2 => Mobility, 3 => Special
        public int this[int index]
        {
            get
            {
                _resources ??= new int[NumResources];
                return _resources[index];
            }
            internal set
            {
                _resources ??= new int[NumResources];
                _resources[index] = value;
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

        public override int GetHashCode()
        {
            HashCode hash = new();
            for (int a = 0; a < NumResources; a++)
                hash.Add(this[a]);
            return hash.ToHashCode();
        }
    }
}
