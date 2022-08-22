using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace WebTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonString;
            using (var client = new WebClient())
                jsonString = client.DownloadString("http://files.olo.com/pizzas.json");

            JArray json = JArray.Parse(jsonString);
            Console.WriteLine(json);
            Console.WriteLine();

            Console.WriteLine(json.Count);
            Console.WriteLine();

            var toppings = json
                .SelectMany(pizza => pizza["toppings"])
                .Distinct()
                .OrderBy(topping => topping);

            foreach (var topping in toppings)
                Console.WriteLine(topping);
            Console.WriteLine();

            Console.WriteLine(toppings.Count());
            Console.WriteLine();
        }
    }
}
