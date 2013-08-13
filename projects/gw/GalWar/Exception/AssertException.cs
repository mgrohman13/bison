using System;
using System.Collections.Generic;
using System.Linq;

namespace GalWar
{
    public class AssertException : Exception
    {
        internal static void Assert(bool value)
        {
            if (!value)
                throw new AssertException();
        }
    }
}
