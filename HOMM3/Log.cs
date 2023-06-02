using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOMM3
{
    public class Log
    {
        private readonly static List<string> logs = new();
        public static void Out(string format, params object[] args)
        {
            Out(2, format, args);
        }
        public static void Out(int digits, string format, params object[] args)
        {
            string FormatDigits(object a) => a is double d ? d.ToString("0." + new string('0', digits)) : a.ToString();
            args = args.Select(a =>
            {
                IEnumerable<object> enumerable;
                if (a is IList l && (enumerable = Enumerable.OfType<object>(l)).Any())
                    return "[" + enumerable.Select(FormatDigits).Aggregate((b, c) => b.ToString() + "," + c.ToString()) + "]";
                return a;
            }).Select(FormatDigits).ToArray();
            string log = string.Format(format, args);
            Debug.WriteLine(log);
            logs.Add(log);
        }
        public static void Write(string file)
        {
            File.WriteAllText(file, logs.Aggregate((a, b) => a + Environment.NewLine + b));
        }
    }
}
