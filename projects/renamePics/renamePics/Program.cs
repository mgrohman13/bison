using System;
using System.Collections.Generic;
using System.IO;
using MattUtil;

namespace renamePics
{
    class Program
    {
        static void Main(string[] args)
        {
            MTRandom Random = new MTRandom();
            Random.StartTick();

            if (args.Length > 0)
            {
                string path = args[0];
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    foreach (string file in files)
                        File.Move(file, file + "_");
                    int num = files.Length;
                    string format = "".PadLeft(num.ToString().Length, '0');
                    foreach (string file in Random.Iterate<string>(files))
                        File.Move(file + "_", Path.GetDirectoryName(file) + @"\" + ( num-- ).ToString(format) + Path.GetExtension(file).TrimEnd('_'));
                }
                else
                {
                    Console.WriteLine("Cannot find path " + path);
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("No path argument.");
                Console.ReadKey();
            }

            Random.Dispose();
        }
    }
}
