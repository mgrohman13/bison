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
        static Game()
        {
            Random = new MTRandom();
            Random.StartTick();
        }

        public static void Main(string[] args)
        {
            //Console.Write(Convert.ToChar(19));
            //Console.BufferWidth = Console.LargestWindowWidth;
            //for (int a = 0 ; a < short.MaxValue ; ++a)
            //    Console.Write(Convert.ToChar(a));
            //Console.ReadKey();

            try
            {
                do
                {
                    Game game = new Game();
                    game.Run();
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
            finally
            {
                if (Random != null)
                    Random.Dispose();
            }
        }

        private Map map;

        private Game()
        {
            Tile start;
            do
                start = ( map = new Map() ).Get(new Point(0, 0));
            while (!ValidStart(start));
            map.ClearCache();
        }
        private static bool ValidStart(Tile terrain)
        {
            return ( terrain.Terrain == Terrain.Grass && terrain.Feature == Feature.None );
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
                    double a = 21;
                    if (k.Value.Key == ConsoleKey.DownArrow || k.Value.Key == ConsoleKey.UpArrow)
                        a /= 1.5;

                    int amt = Random.GaussianCappedInt(a, .26, 1);
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
                double range = Random.GaussianCapped(26, .13);
                int r = (int)range + 1;
                range *= range;
                foreach (Point p in Random.Iterate(x - r, x + r, y - r, y + r))
                {
                    int vx = p.X, vy = p.Y;
                    double yDist = ( vy - y ) * Consts.YMult;
                    if (( vx - x ) * ( vx - x ) + yDist * yDist < range)
                        map.Get(new Point(vx, vy));
                }
                map.ClearCache();
                map.DrawAll(new Point(x - width / 2, y - height / 2), width, height);
            }
            while (( k = Console.ReadKey(true) ).Value.Key != ConsoleKey.Q);
        }
    }
}
