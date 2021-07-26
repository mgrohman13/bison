using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace ConsoleApplication1
{
    class Program
    {
     public    static readonly MTRandom Random = new MTRandom();

        static void Main(string[] args)
        {
            Random.StartTick();
        }
    }
}
