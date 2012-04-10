using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace z2
{
    public class Game
    {
        public static MTRandom Random;
        public static void Main(string[] args)
        {
            Random = new MTRandom();
            Random.StartTick();
            do
            {
                Game game = new Game();
                game.Run();
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Random.Dispose();
        }

        private Map map;

        private Game()
        {
            map = new Map();
        }

        private void Run()
        {
            int width = Console.LargestWindowWidth, height = Console.LargestWindowHeight;
            if (width > Console.WindowWidth)
                Console.WindowWidth = Console.BufferWidth = width;
            else
                Console.BufferWidth = Console.WindowWidth = width;
            if (height > Console.WindowHeight)
                Console.WindowHeight = ( Console.BufferHeight = height + 1 ) - 1;
            else
                Console.BufferHeight = ( Console.WindowHeight = height ) + 1;

            int x = (int)map.minX;
            int y = (int)map.minY;
            while (true)
            {
                map.DrawAll(new Point(x, y), width, height);
                System.Threading.Thread.Sleep(1000);
                if (Random.Bool())
                {
                    x += width / 2;
                    if (x + width / 2 > map.maxX)
                    {
                        x = (int)map.minX - width / 2;
                        y += height;
                    }
                }
            }
        }
    }
}
