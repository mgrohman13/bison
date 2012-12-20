using System;
using System.Collections.Generic;

namespace GalWar
{
    [Serializable]
    public class AssertException : Exception
    {
        internal static void Assert(bool value)
        {
            if (!value)
                throw new AssertException();
        }
    }
}
