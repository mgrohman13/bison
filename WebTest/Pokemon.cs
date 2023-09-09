using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;

namespace WebTest
{
    class Pokemon
    {
        static void Main(string[] args)
        {
            IEnumerable<JToken> pokemon = Enumerable.Empty<JToken>();

            string jsonString;
            using (var client = new WebClient())
            {
                string nextPage = "https://pokeapi.co/api/v2/pokemon/?limit=200";
                while (nextPage.ToString() != string.Empty)
                {
                    jsonString = client.DownloadString(nextPage);
                    JObject json = JObject.Parse(jsonString);
                    pokemon = pokemon.Concat((JArray)json["results"]);
                    nextPage = ((JValue)json["next"]).ToString();

                    Console.WriteLine(nextPage);
                }
            }

            var ids = pokemon.Select(pokemon => pokemon["url"].ToString())
                .Select(url => url[..^1])
                .Select(url => url[(url.LastIndexOf("/") + 1)..])
                .Select(url => int.Parse(url)).ToHashSet();

            int minId = ids.Min();
            int maxId = ids.Max();
            int rangeStart = -1;
            for (int id = minId; id <= maxId + 1; id++)
            {
                if (!ids.Contains(id))
                {
                    if (rangeStart == -1)
                        rangeStart = id;
                }
                else
                {
                    if (rangeStart > -1)
                    {
                        Console.Write("missing range: ");
                        Console.Write(rangeStart);
                        Console.Write("-");
                        Console.Write(id - 1);

                        rangeStart = -1;
                    }
                }
            }
        }
    }
}
