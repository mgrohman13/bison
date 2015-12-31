using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MattUtil;

namespace CollectorKiller
{
    class CollectorKiller
    {
        static readonly MTRandom rand = new MTRandom();

        const int startLife = 13;
        const int startAmmo = 13;

        const int minDimension = 6;
        const double startWidth = 39.0;
        const double startHeight = 16.9;

        const double startEnemy = .21;
        const double startPickup = 1 / 3.9;

        const double startRand = .169;

        const double createEnemyChance = .3;
        const double createPickupChance = 1 / 3.0;

        const int pause = 780;

        static readonly HashSet<Point> _invalid = new HashSet<Point>();
        static readonly Stopwatch timer = new Stopwatch();

        readonly Piece[,] _map;
        readonly int width, height;
        int xP, yP, score, life, ammo;
        double time;

        static void Main(string[] args)
        {
            rand.StartTick();
            new CollectorKiller().PlayGame();
            rand.Dispose();
        }

        void DrawBorder()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;

            Console.Clear();

            Console.WindowHeight = height + 4;
            Console.WindowWidth = Math.Max(width + 2, 33);
            Console.BufferWidth = Console.WindowWidth;

            for (int y = 0 ; y < height ; ++y)
            {
                Console.SetCursorPosition(width, y);
                Console.Write('|');
            }
            Console.SetCursorPosition(0, height);
            Write('-', width);
            Console.Write('+');
        }

        void ReadScores()
        {
            try
            {
                using (StreamReader reader = new StreamReader("scores.txt"))
                {

                    ulong avg = 0, scores = 0, high = 0;
                    string input;
                    while (( input = reader.ReadLine() ) != null)
                    {
                        ulong score;
                        if (ulong.TryParse(input, out score))
                        {
                            avg += score;
                            scores++;
                            if (high < score)
                                high = score;
                        }
                    }

                    reader.Close();

                    WriteMessage(string.Format("High Score: {0}  Avg: {1}", high,
                            ( scores < 1 ? 0.0 : avg / (double)scores ).ToString("0.0")));
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }
        void UpdateScores()
        {
            try
            {
                using (StreamWriter output = new StreamWriter("scores.txt", true))
                {
                    output.WriteLine(score);
                    output.Flush();
                    output.Close();
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        void HandleException(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(exception);
            Console.ReadKey(true);
        }

        CollectorKiller()
        {
            _invalid.Clear();

            width = rand.GaussianCappedInt(startWidth, startRand, minDimension);
            height = rand.GaussianCappedInt(startHeight, startRand, minDimension);

            _map = new Piece[width, height];

            score = 0;
            life = startLife;
            ammo = startAmmo;
            time = 0;
            xP = rand.GaussianCappedInt(( width - 1 ) / 2.0, startRand);
            yP = rand.GaussianCappedInt(( height - 1 ) / 2.0, startRand);

            CreateStartPieces();

            DrawBorder();
            ReadScores();
            DrawAll();
        }

        void PlayGame()
        {
            new Thread(ThreadStart).Start();
            time = 6500;
            timer.Restart();

            while (life >= 0)
            {
                MovePlayer();
                MoveEnemies();
                CreatePieces();
                CheckPiece(true);

                double elapsed = GetElapsedTime();
                time = elapsed + rand.Gaussian(( time - elapsed ) / 1.69, .13) + rand.Gaussian(3900, .013);
            }

            EndGame();
        }
        int GetDisplayTime()
        {
            return (int)Math.Floor(( time - GetElapsedTime() ) / 1000.0);
        }
        double GetElapsedTime()
        {
            return 0;
            //return timer.ElapsedTicks / MattUtil.RealTimeGame.GameTicker.TicksPerMilisecond;
        }

        void ThreadStart()
        {
            while (life >= 0)
            {
                WriteTime();
                Thread.Sleep(1000);
            }
        }

        static Thread inputThread;
        static AutoResetEvent getInput, gotInput;
        static ConsoleKeyInfo input;
        static CollectorKiller()
        {
            getInput = new AutoResetEvent(false);
            gotInput = new AutoResetEvent(false);
            inputThread = new Thread(reader);
            inputThread.IsBackground = true;
            inputThread.Start();
        }
        static void reader()
        {
            while (true)
            {
                getInput.WaitOne();
                input = Console.ReadKey(true);
                gotInput.Set();
            }
        }
        ConsoleKeyInfo ReadKey()
        {
            getInput.Set();
            bool success = gotInput.WaitOne(1000);
            if (success)
                return input;
            else if (GetDisplayTime() < 0)
                return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
            else
                return ReadKey();
        }

        void EndGame()
        {
            UpdateScores();
            Pause();

            WriteMessage("You Lost!   Final Score: " + score, 1);
            ClearPlayerInfo();
            WriteMessage("Play again? (y/n)");

            string input;
            do
            {
                input = Console.ReadKey(true).KeyChar.ToString().ToUpper();
            }
            while (!( input.Equals("Y") || input.Equals("N") ));

            if (input.Equals("Y"))
            {
                new CollectorKiller().PlayGame();
            }
            else
            {
                WriteMessage("Thanks for playing!  Press any key...");
                Console.ReadKey(true);
            }
        }

        void CreateStartPieces()
        {
            const double sizePow = .26, numPow = .52;
            const double minStartPickup = 1.0;

            double size = width * height - 1.0;
            double factor = Math.Pow(( startWidth * startHeight - 1.0 ) / size, sizePow);
            double pickups = startPickup * factor;

            factor = startEnemy * size;
            int cap = (int)Math.Ceiling(Math.Pow(minStartPickup * Math.Pow(factor, numPow) / pickups, 1.0 / ( 1.0 + numPow )));
            if (cap > factor)
                cap = (int)Math.Floor(factor);
            int numEnemies = rand.GaussianCappedInt(factor, startRand, cap);
            pickups *= Math.Pow(numEnemies / factor, numPow) * numEnemies;
            int numPickups = rand.Round(pickups);

            for (int a = 0 ; a < numEnemies ; ++a)
                CreateStartPiece(Piece.Enemy);
            for (int b = 0 ; b < numPickups ; ++b)
                CreateStartPiece(Piece.Pickup);
        }
        void CreateStartPiece(Piece piece)
        {
            int x, y;
            do
            {
                x = rand.Next(width);
                y = rand.Next(height);
            } while (_map[x, y] != Piece.None || ( x == this.xP && y == this.yP ));
            _map[x, y] = piece;
        }

        void MovePlayer()
        {
            Move move = GetInput();

            Invalidate(xP, yP);

            switch (move)
            {
            case Move.Up:
                if (yP != 0)
                {
                    yP--;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }
            case Move.Left:
                if (xP != 0)
                {
                    xP--;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }
            case Move.Down:
                if (yP + 1 < height)
                {
                    yP++;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }
            case Move.Right:
                if (xP + 1 < width)
                {
                    xP++;
                    break;
                }
                else
                {
                    MovePlayer();
                    return;
                }

            case Move.Attack:
                if (ammo > 0 && GetAdjacent(xP, yP).Any(point => Get(point) == Piece.Enemy))
                {
                    ammo--;
                    foreach (Point point in GetAdjacent(xP, yP))
                    {
                        if (Get(point) == Piece.Enemy)
                            score++;
                        Set(point, Piece.None);
                    }
                }
                break;

            default:
                throw new Exception();
            }

            life -= GetAdjacent(xP, yP).Count(point => Get(point) == Piece.Enemy);

            Invalidate(xP, yP);

            CheckPiece(false);
        }
        Move GetInput()
        {
            Refresh();

            if (ammo == 0 || life == 0)
                Console.Beep();
            int lastKey = Environment.TickCount;
            while (true)
            {
                WriteTime();
                Console.SetCursorPosition(xP, yP);
                ConsoleKeyInfo key = ReadKey();
                if (Environment.TickCount - lastKey < 260)
                    continue;

                char input;
                if (key.Key == ConsoleKey.LeftArrow)
                    input = 'a';
                else if (key.Key == ConsoleKey.UpArrow)
                    input = 'w';
                else if (key.Key == ConsoleKey.RightArrow)
                    input = 'd';
                else if (key.Key == ConsoleKey.DownArrow)
                    input = 's';
                else
                    input = key.KeyChar;

                switch (input)
                {
                case ' ':
                    return Move.Attack;
                case 'W':
                case 'w':
                    return Move.Up;
                case 'A':
                case 'a':
                    return Move.Left;
                case 'S':
                case 's':
                    return Move.Down;
                case 'D':
                case 'd':
                    return Move.Right;
                case 'P':
                case 'p':
                    throw new Exception("pause");
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
                int pickups = rand.OEInt(enemies * createPickupChance);
                //Log(enemies + " : " + pickups);

                var create = Enumerable.Repeat(Piece.Enemy, enemies);
                if (pickups > 0)
                    create = create.Concat(Enumerable.Repeat(Piece.Pickup, pickups));
                foreach (Piece piece in rand.Iterate(create))
                    CreatePiece(piece);
            }
        }
        void CreatePiece(Piece piece)
        {
            int x = rand.Next(width);
            int y = rand.Next(height);
            if (Get(x, y) == Piece.None)
            {
                Set(x, y, piece);
                //Log(piece.ToString());
            }
        }
        //int inc = 4;
        //void Log(string msg)
        //{
        //    Console.ForegroundColor = ConsoleColor.Gray;
        //    Console.SetCursorPosition(0, height + inc++);
        //    Console.WriteLine(msg);
        //    Console.SetWindowPosition(0, 0);
        //}

        void CheckPiece(bool pause)
        {
            Piece piece = Get(xP, yP);

            if (pause && piece != Piece.None)
                Pause();

            switch (piece)
            {
            case Piece.Pickup:
                life++;
                ammo++;
                break;
            case Piece.Enemy:
                life--;
                score++;
                break;
            }

            Set(xP, yP, Piece.None);
        }

        IEnumerable<Point> GetAdjacent(int x, int y)
        {
            for (int yAdd = -1 ; yAdd <= 1 ; ++yAdd)
            {
                int yTot = y + yAdd;
                if (yTot >= 0 && yTot < height)
                    for (int xAdd = -1 ; xAdd <= 1 ; ++xAdd)
                    {
                        int xTot = x + xAdd;
                        if (xTot >= 0 && xTot < width)
                            if (x != xTot || y != yTot)
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

        Piece Get(Point point)
        {
            return Get(point.X, point.Y);
        }
        Piece Get(int x, int y)
        {
            return _map[x, y];
        }

        void Set(Point point, Piece piece)
        {
            Set(point.X, point.Y, piece);
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
            lock (this)
            {
                foreach (Point point in rand.Iterate(_invalid))
                    Draw(point);

                DrawPlayer();
                WritePlayerInfo();
            }

            _invalid.Clear();
        }

        void DrawAll()
        {
            foreach (Point point in rand.Iterate(width, height))
            {
                Draw(point);
                if (Get(point) != Piece.None)
                    Thread.Sleep(6);
            }
        }

        void Draw(Point point)
        {
            int x = point.X, y = point.Y;
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

        void DrawPlayer(ConsoleColor color = ConsoleColor.Yellow)
        {
            if (Get(xP, yP) == Piece.None)
            {
                Console.SetCursorPosition(xP, yP);
                if (ammo == 0 || life == 0)
                    Console.ForegroundColor = ConsoleColor.Green;
                else
                    Console.ForegroundColor = color;
                Console.Write('X');
            }
        }

        void WritePlayerInfo()
        {
            ClearPlayerInfo();
            Console.SetCursorPosition(0, height + 2);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Score: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(score);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("   Ammo: ");
            if (ammo == 0)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(ammo);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("   Life: ");
            if (life == 0)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(life);
        }
        void WriteTime()
        {
            int display = Math.Max(0, GetDisplayTime());
            lock (this)
            {
                bool critical = ( display == 0 );
                if (critical)
                    DrawPlayer(ConsoleColor.Green);
                WriteMessage(display.ToString(), 1, critical ? ConsoleColor.Green : ConsoleColor.Yellow);
            }
        }

        void ClearPlayerInfo()
        {
            WriteMessage("", 2);
        }
        void WriteMessage(string message)
        {
            WriteMessage(message, 3);
        }
        void WriteMessage(string message, int sizeInc, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.SetCursorPosition(0, height + sizeInc);
            Console.ForegroundColor = color;
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
            time += pause;
        }

        enum Piece
        {
            None = 0,
            Pickup,
            Enemy,
        }

        enum Move
        {
            Up,
            Down,
            Left,
            Right,
            Attack,
        }
    }
}
