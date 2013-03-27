using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace game1
{
    class Program
    {
        public const int extraHeight = 3, framerate = 20, startTime = 10000;

        static int size = 33;
        public static int Height
        {
            get
            {
                return size;
            }
        }
        public static int Width
        {
            get
            {
                return size * 2;
            }
        }

        static int time;

        public static double TimeMult
        {
            get
            {
                return (double)time / (double)startTime;
            }
        }

        static List<Piece> pieces;
        static List<Piece>[,] pieceMap;
        static Terrain[,] map;
        public static Player player;
        public static bool OutPutValid = false;
        static bool pause = false;

        public static Piece[] Pieces
        {
            get
            {
                return pieces.ToArray();
            }
        }

        public static Piece[] getPieces(int X, int Y)
        {
            return pieceMap[X, Y].ToArray();
        }

        public static Terrain getTerrain(int X, int Y)
        {
            return map[X, Y];
        }

        public static void MovePiece(Piece p, int X, int Y)
        {
            Invalidate(p.X, p.Y);
            Invalidate(X, Y);

            pieceMap[p.X, p.Y].Remove(p);
            pieceMap[X, Y].Add(p);
        }

        public static void RemovePiece(Piece p)
        {
            Invalidate(p.X, p.Y);

            pieceMap[p.X, p.Y].Remove(p);
            pieces.Remove(p);
        }

        public static void AddPiece(Piece p)
        {
            Invalidate(p.X, p.Y);

            pieceMap[p.X, p.Y].Add(p);
            pieces.Add(p);
        }

        public static MattUtil.MTRandom rand;
        static bool[,] valid;

        static void Main(string[] args)
        {
            rand = new MattUtil.MTRandom();
            rand.StartTick();

            Console.CursorVisible = false;

            Console.WindowHeight = Height + extraHeight;
            Console.BufferHeight = Height + extraHeight;
            Console.WindowWidth = Width;
            Console.BufferWidth = Width;

            NewGame();

            rand.Dispose();
        }

        private static void NewGame()
        {
            time = startTime;

            valid = new bool[Width, Height];
            map = new Terrain[Width, Height];
            pieceMap = new List<Piece>[Width, Height];
            pieces = new List<Piece>();

            player = new Player(ConsoleColor.DarkMagenta);

            for (int X = 0 ; X < Width ; X++)
                for (int Y = 0 ; Y < Height ; Y++)
                {
                    valid[X, Y] = false;
                    map[X, Y] = new Dirt(X, Y);
                    pieceMap[X, Y] = new List<Piece>();
                    Draw(X, Y);
                }

            AddPiece(player);

            for (int i = 0 ; i < 13 ; i++)
            {
                AddPiece(new SquirrelEater());
                AddPiece(new Squirrel());
            }

            Scout();

            Thread gameThread = new Thread(InputStuff);
            gameThread.Start();

            GameLoop();
        }

        private static void GameLoop()
        {
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            while (player.Health > 0)
            {
                timer.Reset();
                timer.Start();

                if (!pause)
                {
                    CreateStuff();

                    pieces = RandomSort(pieces);

                    foreach (Piece p in Pieces)
                        p.Move();

                    player.MoveHere();
                    Scout();

                    for (int X = 0 ; X < Width ; X++)
                        for (int Y = 0 ; Y < Height ; Y++)
                            if (!valid[X, Y])//&& map[X, Y].Visible)
                                Draw(X, Y);

                    time++;
                }

                long timeDiff = framerate - timer.ElapsedMilliseconds;
                if (timeDiff > 0)
                {
                    System.Threading.Thread.Sleep((int)timeDiff);
                }

                timer.Stop();
            }

            GameOverStuff();
        }

        private static void GameOverStuff()
        {
            int finalScore = rand.Round(player.Score);

            int high = -1;
            double average = -1;

            //try
            //{
            SaveScore(finalScore);

            ReadScores(out high, out average);
            //}
            //catch { }

            string output = string.Format("Game Over!  Final Score: {0}", finalScore);
            if (high != -1 && average != -1)
                output = string.Format("{2}Avg: {0}   High: {1}", average.ToString("0"), high, output.PadRight(Width));
            output = string.Format("{0}Play Again? (y/n)", output.PadRight(Width * 2));

            Console.SetCursorPosition(0, Height);
            Console.ResetColor();
            Console.Write(output.PadRight(Width * ( extraHeight - 1 )));
        }

        const string fileName = "sq.dat";

        private static void SaveScore(int finalScore)
        {
            FileStream fs = File.Open(fileName, FileMode.Append);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write((ushort)finalScore);
            bw.Flush();
            bw.Close();
            fs.Close();
            fs.Dispose();
        }

        private static void ReadScores(out int High, out double Avg)
        {
            FileStream fs = File.Open(fileName, FileMode.OpenOrCreate);
            BinaryReader br = new BinaryReader(fs);

            int total = 0, amt = 0, high = 0;

            while (true)
            {
                try
                {
                    ushort current = br.ReadUInt16();
                    total += current;
                    amt++;
                    high = Math.Max(high, current);
                }
                catch
                {
                    break;
                }
            }

            High = high;
            Avg = (double)total / (double)amt;

            fs.Close();
            br.Close();
            fs.Dispose();
        }

        private static void CreateStuff()
        {
            if (rand.Bool(TimeMult * .00075))
            {
                AddPiece(new Squirrel(rand.Next(Width), rand.Next(Height)));
                AddPiece(new SquirrelEater(rand.Next(Width), rand.Next(Height)));
            }
        }

        private static void InputStuff()
        {
            bool loop = true;
            while (loop)
            {
                switch (Console.ReadKey(true).Key)
                {
                case ConsoleKey.W:
                    player.MoveUp();
                    break;

                case ConsoleKey.A:
                    player.MoveLeft();
                    break;

                case ConsoleKey.S:
                    player.MoveDown();
                    break;

                case ConsoleKey.D:
                    player.MoveRight();
                    break;

                case ConsoleKey.UpArrow:
                    player.fire(0);
                    break;

                case ConsoleKey.LeftArrow:
                    player.fire(1);
                    break;

                case ConsoleKey.DownArrow:
                    player.fire(2);
                    break;

                case ConsoleKey.RightArrow:
                    player.fire(3);
                    break;

                case ConsoleKey.Spacebar:
                    player.Stop();
                    break;

                //case ConsoleKey.Enter:
                //    player.DropSquirrels();
                //    break;

                case ConsoleKey.Y:
                    if (player.Health <= 0)
                    {
                        Thread gameThread = new Thread(NewGame);
                        gameThread.Start();
                        loop = false;
                    }
                    break;

                case ConsoleKey.N:
                    if (player.Health <= 0)
                    {
                        loop = false;
                    }
                    break;

                case ConsoleKey.P:
                    pause = !pause;
                    break;
                }
            }
        }

        private static void Scout()
        {
            if (!OutPutValid)
            {
                string output = string.Format("Life: {0}   Squirrles: {1}   Score: {2}", player.Health, player.Squirrels, player.Score.ToString("0"));

                Console.SetCursorPosition(0, Height);
                Console.ResetColor();
                Console.Write(output.PadRight(Width * 3 - 1));
                OutPutValid = true;
            }

            const double viewSize = 4.5;

            for (int X = player.X - (int)Math.Ceiling(viewSize) ; X <= player.X + (int)Math.Ceiling(viewSize) ; X++)
                for (int Y = player.Y - (int)Math.Ceiling(viewSize) ; Y <= player.Y + (int)Math.Ceiling(viewSize) ; Y++)
                    if (GetDist(X, Y, player.X, player.Y) < viewSize)
                        if (X >= 0 && X < Width && Y >= 0 && Y < Height)
                            map[X, Y].Visible = true;
        }

        public static int GetCornerDist(int x1, int y1, int x2, int y2)
        {
            return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }

        public static int GetDistXAndY(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        public static double GetDist(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static List<Piece> RandomSort(List<Piece> objects)
        {
            List<Piece> result = new List<Piece>();
            while (objects.Count > 0)
            {
                int index = rand.Next(objects.Count);
                result.Add(objects[index]);
                objects.RemoveAt(index);
            }
            return result;
        }

        public static void Invalidate(int X, int Y)
        {
            if (map[X, Y].Visible)
                valid[X, Y] = false;
        }

        public static void ForceInvalidate(int X, int Y)
        {
            valid[X, Y] = false;
        }

        static void Draw(int X, int Y)
        {
            valid[X, Y] = true;

            if (!map[X, Y].Visible)
            {
                Console.SetCursorPosition(X, Y);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write('*');

                return;
            }

            if (player.X == X && player.Y == Y)
            {
                player.Draw(map[X, Y].Color);
                return;
            }

            foreach (Piece p in Pieces)
                if (p.X == X && p.Y == Y)
                {
                    p.Draw(map[X, Y].Color);
                    return;
                }

            Console.SetCursorPosition(X, Y);
            Console.BackgroundColor = map[X, Y].Color;
            if (map[X, Y].ForeGround)
            {
                Console.ForegroundColor = map[X, Y].ForeColor;
                Console.Write(map[X, Y].Character);
            }
            else
                Console.Write(' ');
        }

        public static int[] RandXY()
        {
            return new int[] { RandX(), RandY() };
        }

        public static int RandX()
        {
            return rand.Next(Width);
        }

        public static int RandY()
        {
            return rand.Next(Height);
        }
    }
}