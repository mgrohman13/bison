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

            int x = 0, y = 0;
            ConsoleKeyInfo? k = null;
            do
            {
                width = Console.WindowWidth;
                height = Console.WindowHeight;
                if (width > Console.WindowWidth)
                    Console.WindowWidth = Console.BufferWidth = width;
                else
                    Console.BufferWidth = Console.WindowWidth = width;
                if (height > Console.WindowHeight)
                    Console.WindowHeight = ( Console.BufferHeight = height + 1 ) - 1;
                else
                    Console.BufferHeight = ( Console.WindowHeight = height ) + 1;

                if (k.HasValue)
                {
                    int amt = Random.GaussianCappedInt(13, .26, 1);
                    switch (k.Value.Key)
                    {
                    case ConsoleKey.DownArrow:
                        y += amt;
                        break;
                    case ConsoleKey.LeftArrow:
                        x -= amt;
                        break;
                    case ConsoleKey.RightArrow:
                        x += amt;
                        break;
                    case ConsoleKey.UpArrow:
                        y -= amt;
                        break;
                    }
                }
                map.OnMove(new Point(x, y));
                map.DrawAll(new Point(x - width / 2, y - height / 2), width, height);
            }
            while (( k = Console.ReadKey(true) ).Value.Key != ConsoleKey.Q);
        }
    }
}
