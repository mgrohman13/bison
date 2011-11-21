using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using MattUtil;

namespace CollectorKiller
{
    class CollectorKiller
    {
        const int startLife = 13;
        const int startAmmo = 13;

        const int minDimension = 6;
        const float startWidth = 39;
        const float startHeight = 16.9f;

        const float startEnemy = .26f;
        const float startPickup = 1 / 3.9f;

        const float startRand = .13f;

        const double createEnemyChance = .3;
        const double createPickupChance = 1 / 3.0;

        const int pause = 666;

        static MTRandom rand;

        static List<Point> _invalid;

        Piece[,] _map;
        int width, height, x, y, score, life, ammo;

        static void Main(string[] args)
        {
            rand = new MTRandom();
            rand.StartTick();

            new CollectorKiller().PlayGame();

            rand.Dispose();
        }

        void DrawBorder()
        {
            Console.ForegroundColor = ConsoleColor.White;
            for (int y = 0 ; y < height ; ++y)
            {
                Console.SetCursorPosition(width, y);
                Console.Write('|');
                Write(' ', Console.BufferWidth - width - 1);
            }
            Write('-', width);
            Console.Write('+');
            Write(' ', 3 * Console.BufferWidth - width - 1);
        }

        void ReadScores()
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader("scores.txt");

                double avg = 0, scores = 0;
                string input;
                while (( input = reader.ReadLine() ) != null)
                {
                    avg += int.Parse(input);
                    scores++;
                }
                reader.Close();

                WriteMessage("Average score: " + ( (double)( scores < 1 ? 0 : avg / scores ) ).ToString("0.0"));
            }
            catch (Exception e)
            {
                WriteMessage(e.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        void UpdateScores()
        {
            StreamWriter output = null;
            try
            {
                output = new StreamWriter("scores.txt", true);
                output.WriteLine(score.ToString());
                output.Flush();
                output.Close();
            }
            catch (System.Exception e)
            {
                WriteMessage(e.Message);
            }
            finally
            {
                if (output != null)
                    output.Close();
            }
        }

        void PlayGame()
        {
            width = rand.GaussianCappedInt(startWidth, startRand, minDimension);
            height = rand.GaussianCappedInt(startHeight, startRand, minDimension);

            Console.WindowHeight = height + 3;
            Console.WindowWidth = Math.Max(width + 2, 32);
            Console.BufferWidth = Console.WindowWidth;
            DrawBorder();

            score = 0;
            life = startLife;
            ammo = startAmmo;
            x = rand.GaussianCappedInt(( width - 1 ) / 2f, startRand);
            y = rand.GaussianCappedInt(( height - 1 ) / 2f, startRand);
            InitGameMap();
            DrawAll();

            ReadScores();

            while (life >= 0)
            {
                MovePlayer();
                MoveEnemies();
                CreatePieces();
                CheckPiece(true);
            }

            Refresh();
            UpdateScores();

            WriteMessage("You Lost!   Final Score: " + score, 1);
            WriteMessage("Play again? (y/n)");

            string input;
            do
            {
                input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            }
            while (!( input.Equals("Y") || input.Equals("N") ));

            if (input.Equals("Y"))
            {
                PlayGame();
            }
            else
            {
                WriteMessage("Thank you for playing!  Press any key to exit.");
                Console.ReadKey(true);
            }
        }

        void InitGameMap()
        {
            _map = new Piece[width, height];
            _invalid = new List<Point>();

            float size = width * height - 1;
            float sizeFactor = (float)Math.Sqrt(( startWidth * startHeight - 1 ) / size);

            int numEnemies, numPickups;
            do
            {
                float gaussian = 1 + rand.Gaussian(startRand);
                numEnemies = rand.Round(startEnemy * size * gaussian);
                numPickups = rand.Round(startPickup * sizeFactor * numEnemies * gaussian);
            } while (numEnemies + numPickups > size || numPickups < 1 || numEnemies < 1);

            for (int e = 0 ; e < numEnemies ; ++e)
                CreateStart(Piece.Enemy);
            for (int p = 0 ; p < numPickups ; ++p)
                CreateStart(Piece.Pickup);
        }

        void CreateStart(Piece piece)
        {
            int x, y;
            do
            {
                x = rand.Next(width);
                y = rand.Next(height);
            } while (_map[x, y] != Piece.None || ( x == this.x && y == this.y ));
            _map[x, y] = piece;
        }

        void MovePlayer()
        {
            Move move = GetInput();

            Invalidate(x, y);

            switch (move)
            {
            case Move.Up:
                if (y != 0)
                {
                    y--;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }
            case Move.Left:
                if (x != 0)
                {
                    x--;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }
            case Move.Down:
                if (y + 1 < height)
                {
                    y++;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }
            case Move.Right:
                if (x + 1 < width)
                {
                    x++;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }

            case Move.Attack:
                bool useAmmo = false;
                foreach (Point p in GetAdjacent(x, y))
                    if (Get(p.x, p.y) == Piece.Enemy)
                    {
                        useAmmo = true;
                        break;
                    }
                if (useAmmo)
                {
                    if (ammo > 0)
                    {
                        ammo--;
                        foreach (Point p in GetAdjacent(x, y))
                        {
                            if (Get(p.x, p.y) == Piece.Enemy)
                                score++;
                            Set(p.x, p.y, Piece.None);
                        }
                    }
                    else
                    {
                        MovePlayer();
                        return;
                    }
                }
                break;

            case Move.None:
                break;
            default:
                throw new Exception();
            }

            foreach (Point p in GetAdjacent(x, y))
                if (Get(p.x, p.y) == Piece.Enemy)
                    life--;

            Invalidate(x, y);

            CheckPiece(false);
        }

        Move GetInput()
        {
            Refresh();

            while (true)
            {
                if (ammo == 0 || life == 0)
                    Console.Write('\a');

                string input;
                Console.SetCursorPosition(x, y);
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.LeftArrow)
                    input = "a";
                else if (key.Key == ConsoleKey.UpArrow)
                    input = "w";
                else if (key.Key == ConsoleKey.RightArrow)
                    input = "d";
                else if (key.Key == ConsoleKey.DownArrow)
                    input = "s";
                else
                    input = key.KeyChar.ToString();

                switch (input.ToUpper())
                {
                case " ":
                    return Move.Attack;
                case "W":
                    return Move.Up;
                case "A":
                    return Move.Left;
                case "S":
                    return Move.Down;
                case "D":
                    return Move.Right;
                case "P":
                    return Move.None;
                }
            }
        }

        void MoveEnemies()
        {
            switch (rand.Next(8))
            {
            case 0:
                for (int x = 0 ; x < width ; x++)
                    for (int y = 0 ; y < height ; y++)
                        MoveEnemy(x, y);
                break;
            case 1:
                for (int y = 0 ; y < height ; y++)
                    for (int x = 0 ; x < width ; x++)
                        MoveEnemy(x, y);
                break;
            case 2:
                for (int x = width ; --x >= 0 ; )
                    for (int y = height ; --y >= 0 ; )
                        MoveEnemy(x, y);
                break;
            case 3:
                for (int y = height ; --y >= 0 ; )
                    for (int x = width ; --x >= 0 ; )
                        MoveEnemy(x, y);
                break;
            case 4:
                for (int x = width ; --x >= 0 ; )
                    for (int y = 0 ; y < height ; y++)
                        MoveEnemy(x, y);
                break;
            case 5:
                for (int y = height ; --y >= 0 ; )
                    for (int x = 0 ; x < width ; x++)
                        MoveEnemy(x, y);
                break;
            case 6:
                for (int x = 0 ; x < width ; x++)
                    for (int y = height ; --y >= 0 ; )
                        MoveEnemy(x, y);
                break;
            case 7:
                for (int y = 0 ; y < height ; y++)
                    for (int x = width ; --x >= 0 ; )
                        MoveEnemy(x, y);
                break;
            default:
                throw new Exception();
            }
        }

        void MoveEnemy(int x, int y)
        {
            if (Get(x, y) == Piece.Enemy)
                switch (rand.Next(9))
                {
                case 0:
                    Swap(x, y, x - 1, y - 1);
                    break;
                case 1:
                    Swap(x, y, x - 1, y);
                    break;
                case 2:
                    Swap(x, y, x - 1, y + 1);
                    break;
                case 3:
                    Swap(x, y, x + 1, y - 1);
                    break;
                case 4:
                    Swap(x, y, x + 1, y);
                    break;
                case 5:
                    Swap(x, y, x + 1, y + 1);
                    break;
                case 6:
                    Swap(x, y, x, y - 1);
                    break;
                case 7:
                    Swap(x, y, x, y + 1);
                    break;
                case 8:
                    break;
                default:
                    throw new Exception();
                }
        }

        void CreatePieces()
        {
            int enemies = rand.OEInt(createEnemyChance);
            if (enemies > 0)
            {
                for (int a = 0 ; a < enemies ; ++a)
                    CreatePiece(Piece.Enemy);
                int pickups = rand.OEInt(enemies * createPickupChance);
                for (int b = 0 ; b < pickups ; ++b)
                    CreatePiece(Piece.Pickup);
            }
        }

        void CreatePiece(Piece piece)
        {
            int x = rand.Next(width);
            int y = rand.Next(height);
            if (Get(x, y) == Piece.None)
                Set(x, y, piece);
        }

        void CheckPiece(bool pause)
        {
            Piece piece = Get(x, y);

            switch (piece)
            {
            case Piece.None:
                break;

            case Piece.Pickup:
                life++;
                ammo++;
                break;

            case Piece.Enemy:
                life--;
                score++;
                break;
            }

            if (pause && piece != Piece.None)
                Pause();

            Set(x, y, Piece.None);
        }

        IEnumerable<Point> GetAdjacent(int x, int y)
        {
            for (int yAdd = -1 ; yAdd <= 1 ; ++yAdd)
            {
                int yTot = y + yAdd;
                if (yTot >= 0 && yTot < height)
                    for (int xAdd = -1 ; xAdd <= 1 ; ++xAdd)
                        if (!( xAdd == 0 && yAdd == 0 ))
                        {
                            int xTot = x + xAdd;
                            if (xTot >= 0 && xTot < width)
                                yield return new Point(xTot, yTot);
                        }
            }
        }

        void Swap(int x1, int y1, int x2, int y2)
        {
            if (x1 >= 0 && y1 >= 0 && x2 >= 0 && y2 >= 0 && x1 < width && x2 < width && y1 < height && y2 < height)
            {
                Piece temp = Get(x1, y1);
                Set(x1, y1, Get(x2, y2));
                Set(x2, y2, temp);
            }
        }

        Piece Get(int x, int y)
        {
            return _map[x, y];
        }

        void Set(int x, int y, Piece piece)
        {
            if (Get(x, y) != piece)
            {
                _map[x, y] = piece;
                Invalidate(x, y);
            }
        }

        void Invalidate(int x, int y)
        {
            _invalid.Add(new Point(x, y));
        }

        void Refresh()
        {
            foreach (Point p in _invalid)
                Draw(p.x, p.y);
            _invalid.Clear();

            DrawPlayer();
            WritePlayerInfo();
        }

        void DrawAll()
        {
            for (int y = 0 ; y < height ; ++y)
                for (int x = 0 ; x < width ; ++x)
                    Draw(x, y);
        }

        void Draw(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Piece piece = Get(x, y);
            switch (piece)
            {
            case Piece.None:
                Console.Write(' ');
                break;
            case Piece.Pickup:
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write('$');
                break;
            case Piece.Enemy:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write('@');
                break;
            }
        }

        void DrawPlayer()
        {
            if (Get(x, y) == Piece.None)
            {
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write('X');
            }
        }

        void WritePlayerInfo()
        {
            WriteMessage("", 1);
            Console.SetCursorPosition(0, height + 1);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(string.Format("Score: {0}   Ammo: ", score));
            if (ammo == 0)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(ammo);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("   Life: ");
            if (life == 0)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(life);
        }

        void WriteMessage(string message)
        {
            WriteMessage(message, 2);
        }

        void WriteMessage(string message, int sizeInc)
        {
            Console.SetCursorPosition(0, height + sizeInc);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(message);
            if (Console.BufferWidth > message.Length)
                Write(' ', Console.BufferWidth - message.Length);
            Console.SetWindowPosition(0, 0);
        }

        void Write(char value, int repeatCount)
        {
            StringBuilder str = new StringBuilder();
            str.Append(value, repeatCount);
            Console.Write(str);
        }

        void Pause()
        {
            Refresh();
            Thread.Sleep(pause);
        }

        struct Point
        {
            public int x, y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        enum Piece
        {
            None = 0,
            Pickup,
            Enemy,
        }

        enum Move
        {
            None = 0,
            Up,
            Down,
            Left,
            Right,
            Attack,
        }
    }
}
