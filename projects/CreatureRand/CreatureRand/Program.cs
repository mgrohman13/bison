using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MattUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        private static string file = "../../data/creatures.json";
        private static MTRandom rand;

        public static void Main(string[] args)
        {
            rand = new MTRandom();

            RandValues();
            //RoundValues();

            Console.WriteLine();
            Console.WriteLine("done");
            Console.ReadKey(true);
        }

        private static void RandValues()
        {
            LoopValues(Rand);
        }
        private static double Rand(double value, double cap)
        {
            if (double.IsNaN(cap))
                cap = .1;
            else
                cap = Math.Max(.1, 2 * value - cap);
            //Console.WriteLine(cap);
            return rand.GaussianCapped(value, .1, cap);
        }

        private static void RoundValues()
        {
            LoopValues((value, cap) => rand.Round(value * 10) / 10.0);
        }

        private static void LoopValues(Func<double, double, double> modify)
        {
            string text = File.ReadAllText(file);
            //Console.WriteLine(text);

            JObject o = (JObject)JsonConvert.DeserializeObject(text);
            foreach (JProperty prop in o.Properties())
                if (!prop.Name.StartsWith("_"))
                    foreach (string key in new[] { "size", "health", "damage" })
                    {
                        double value = prop.Value[key].Value<double>();
                        if (!( key == "size" && value == 1 ))
                        {
                            double cap = double.NaN;
                            if (key == "damage")
                            {
                                JToken capProp = prop.Value["_cap"];
                                if (capProp == null || capProp.Value<string>() != "-")
                                {
                                    if (capProp != null)
                                        cap = capProp.Value<double>();
                                    else if (prop.Value["_wiki_damage"] != null)
                                        cap = 100 / prop.Value["_wiki_damage"].Value<double>();
                                    if (double.IsNaN(cap))
                                        Console.WriteLine("no cap for " + prop.Name);
                                }
                                //Console.WriteLine(prop.Name + ": " + cap);
                            }
                            value = modify(value, cap);
                        }
                        //Console.WriteLine(value);
                        prop.Value[key] = value;
                    }

            text = JsonConvert.SerializeObject(o, Formatting.Indented);
            //Console.WriteLine(text);

            using (StreamWriter writer = File.CreateText(file))
                writer.Write(text);
        }
    }
}
