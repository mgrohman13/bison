using System;
using System.Collections;
using System.Collections.Generic;

namespace Ideas
{
    [Serializable]
    public class EnumFlags<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable where T : struct
    {
        #region constructors

        public EnumFlags()
        {
            flags = 0;
        }

        public EnumFlags(T value)
        {
            flags = TToInt(value);
        }

        public EnumFlags(EnumFlags<T> value)
        {
            flags = value.flags;
        }

        public EnumFlags(int value)
        {
            flags = value;
        }

        #endregion //constructors
        #region integer representation

        public int ToInt()
        {
            return flags;
        }

        public void FromInt(int value)
        {
            flags = value;
        }

        public static EnumFlags<T> LoadFromInt(int value)
        {
            return new EnumFlags<T>(value);
        }

        #endregion //integer representation
        #region fields and properties

        private int flags;

        public T Flags
        {
            get
            {
                return IntToT(flags);
            }
            set
            {
                flags = TToInt(value);
            }
        }

        public int Count
        {
            get
            {
                int count = 0;

                //count the number of flags that are on
                for (int bitMask = 0x1; bitMask != 0; bitMask <<= 1)
                    if ((flags & bitMask) != 0)
                        count++;

                return count;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion //fields and properties
        #region main methods

        #region contains

        //actual check
        private bool Contains(int value)
        {
            return ((flags & value) == value);
        }

        public bool Contains(T value)
        {
            return Contains(TToInt(value));
        }

        public bool Contains(EnumFlags<T> value)
        {
            return Contains(value.flags);
        }

        public EnumFlags<T> ContainsWhich(int value)
        {
            return new EnumFlags<T>(flags & value);
        }

        public EnumFlags<T> ContainsWhich(T value)
        {
            return ContainsWhich(TToInt(value));
        }

        public EnumFlags<T> ContainsWhich(EnumFlags<T> value)
        {
            return ContainsWhich(value.flags);
        }

        #endregion //contains
        #region add

        //actual addition
        private bool Add(int value)
        {
            bool existed = Contains(value);
            flags |= value;
            return existed;
        }

        public void Add(T value)
        {
            Add(TToInt(value));
        }

        public bool Add(EnumFlags<T> value)
        {
            return Add(value.flags);
        }

        public static EnumFlags<T> operator +(EnumFlags<T> e1, T e2)
        {
            return e1 + new EnumFlags<T>(e2);
        }

        public static EnumFlags<T> operator +(T e1, EnumFlags<T> e2)
        {
            return new EnumFlags<T>(e1) + e2;
        }

        public static EnumFlags<T> operator +(EnumFlags<T> e1, EnumFlags<T> e2)
        {
            //return a new instance containing all the combined flags of e1 and e2
            EnumFlags<T> result = e1.Clone();
            result.Add(e2);
            return result;
        }

        #endregion //add
        #region remove

        //actual removal
        private bool Remove(int value)
        {
            bool existed = Contains(value);
            flags &= ~(value);
            return existed;
        }

        public bool Remove(T value)
        {
            return Remove(TToInt(value));
        }

        public bool Remove(EnumFlags<T> value)
        {
            return Remove(value.flags);
        }

        public static EnumFlags<T> operator -(EnumFlags<T> e1, T e2)
        {
            return e1 - new EnumFlags<T>(e2);
        }

        public static EnumFlags<T> operator -(T e1, EnumFlags<T> e2)
        {
            return new EnumFlags<T>(e1) - e2;
        }

        public static EnumFlags<T> operator -(EnumFlags<T> e1, EnumFlags<T> e2)
        {
            //return a new instance containing all the flags in e1 that are not in e2
            EnumFlags<T> result = e1.Clone();
            result.Remove(e2);
            return result;
        }

        #endregion //remove
        #region equality

        public static bool operator ==(EnumFlags<T> e1, T e2)
        {
            return e1.Equals(e2);
        }

        public static bool operator ==(T e1, EnumFlags<T> e2)
        {
            return e2.Equals(e1);
        }

        public static bool operator ==(EnumFlags<T> e1, EnumFlags<T> e2)
        {
            return e1.Equals(e2);
        }

        public static bool operator !=(EnumFlags<T> e1, T e2)
        {
            return !e1.Equals(e2);
        }

        public static bool operator !=(T e1, EnumFlags<T> e2)
        {
            return !e2.Equals(e1);
        }

        public static bool operator !=(EnumFlags<T> e1, EnumFlags<T> e2)
        {
            return !e1.Equals(e2);
        }

        public override bool Equals(object obj)
        {
            //compare to another EnumFlags instance
            if (obj is EnumFlags<T>)
                return (this.flags == ((EnumFlags<T>)obj).flags);

            //compare to an instance of T
            if (obj is T)
                return (this.flags == TToInt((T)obj));

            ////not really sure whether to include this...
            //if (obj is int)
            //    return (this.flags == (int)obj);

            return base.Equals(obj);
        }

        #endregion //equality

        public void Clear()
        {
            flags = 0;
        }

        public void CopyTo(T[] array, int index)
        {
        }

        public EnumFlags<T> Clone()
        {
            //instantiate a new instance with the same data
            return new EnumFlags<T>(flags);
        }

        #endregion //main methods
        #region extra methods

        #region parsing

        public static EnumFlags<T> Parse(string value)
        {
            return new EnumFlags<T>((T)Enum.Parse(typeof(T), value));
        }

        public static EnumFlags<T> Parse(string value, bool ignoreCase)
        {
            return new EnumFlags<T>((T)Enum.Parse(typeof(T), value, ignoreCase));
        }

        public static bool TryParse(string value, out EnumFlags<T> result)
        {
            return TryParse(value, false, out result, false);
        }

        public static bool TryParse(string value, bool ignoreCase, out EnumFlags<T> result)
        {
            return TryParse(value, ignoreCase, out result, true);
        }

        //abstract out try-catch logic
        private static bool TryParse(string value, bool ignoreCase, out EnumFlags<T> result, bool specifyIgnoreCase)
        {
            bool worked;

            try
            {
                //check if a value was givin for ignoreCase, and if so, use it
                if (specifyIgnoreCase)
                    result = Parse(value, ignoreCase);
                else
                    result = Parse(value);

                //the parse succeeded if execution reaches this line
                worked = true;
            }
            catch
            {
                result = null;
                worked = false;
            }

            return worked;
        }

        #endregion //parsing
        #region conversion
        //the entire class is based around this hack, but I see no other way to do this

        private static int TToInt(T value)
        {
            return (int)(ValueType)value;
        }

        private static T IntToT(int value)
        {
            return (T)(ValueType)value;
        }

        #endregion //conversion
        #region ICollection implementation
        public void CopyTo(Array array, int index) { }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return this; } }
        #endregion //ICollection implementation
        #region GetEnumerator implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerator<T> GetEnumerator()
        {
            //If no flags are set and the enumeration type T includes the none instance 0x0,
            //enumerate once, through that none instance.  Otherwise, enumerate through nothing.
            if (flags == 0)
                //only way I could find to check if the type specifies the none instance
                if (Flags.ToString() == "0")
                    return new EnumFlags<T>.Enumerator(new T[] { });
                else
                    return new EnumFlags<T>.Enumerator(new T[] { IntToT(0) });

            //collect each bit and represent as an instance of type T
            List<T> objects = new List<T>();
            for (int bitMask = 0x1; bitMask != 0; bitMask <<= 1)
            {
                int curItem = flags & bitMask;
                //if the current bit is true, add the appropriate instance of type T to the object list
                if (curItem != 0)
                    objects.Add(IntToT(curItem));
            }

            return new EnumFlags<T>.Enumerator(objects.ToArray());
        }

        struct Enumerator : IEnumerator<T>, IEnumerator
        {
            T[] objects;
            int curIndex;

            public Enumerator(T[] objects)
            {
                //initialize with the given array of objects and reset
                this.objects = objects;
                curIndex = -1;
            }

            public void Reset()
            {
                curIndex = -1;
            }

            object IEnumerator.Current { get { return Current; } }
            T IEnumerator<T>.Current { get { return Current; } }
            public T Current
            {
                get
                {
                    return objects[curIndex];
                }
            }

            public bool MoveNext()
            {
                //check for end of array
                return (++curIndex < objects.Length);
            }

            public void Dispose() { }
        }

        #endregion //GetEnumerator implementation

        public override int GetHashCode()
        {
            //return the hash code of the integer storing the data
            return flags.GetHashCode();
        }

        public override string ToString()
        {
            //use Flags property to call ToString() for the represented enumeration type T
            return Flags.ToString();
        }

        #endregion //extra methods
    }
}
