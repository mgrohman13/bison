using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Timers;
using System.IO;

namespace Trogdor
{
    static class Game
    {
        public const int FrameRate = 26;
        public const string ScoresFile = "inf.dat";

        public const double ReactionTime = 500;

        public const double HutSize = 300;
        public const float CreateHut = .006f;
        public const float CreateOther = CreateHut / 2f;

        public const float DeathChance = .05f;
        public const double DeathConst = 2.6;

        public const double PlayerSpeed = .6;
        public const double OtherSpeed = .0003;
        public const float DriftChance = .3f;
        public const float PlayerDrift = .169f;
        public const float OtherDrift = .6f;
        public const double HitDamage = 13.0 / PlayerSpeed;

        public static MattUtil.MTRandom Random;

        private static Main MainForm;
        private static Stats StatsForm;
        private static System.Timers.Timer Timer = new System.Timers.Timer(333);

        public static int Width, Height;
        public static bool Up, Down, Left, Right, GameOver, Paused;
        public static double Score;
        public static List<Piece> Pieces;
        private static Piece Player;

        public static double TotalHut;
        public static double TotalEnemy;
        public static double TotalAlly;
        public static double TotalPlayer;
        public static double DecayHut;
        public static double DecayAlly;
        public static double DecayEnemy;
        public static double DecayPlayer;
        public static double CollectHut;
        public static double CollectAlly;
        public static double CollectEnemy;

        [STAThread]
        static void Main()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();

            if (!File.Exists(ScoresFile))
                File.Create(ScoresFile);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Timer.Elapsed += new ElapsedEventHandler(Interval);
            Timer.Enabled = false;

            MainForm = new Main();
            StatsForm = new Stats();

            NewGame();
            MainForm.ResizeGame();
            Application.Run(MainForm);

            Random.Dispose();
        }

        public static void Pause()
        {
            if (Paused)
            {
                Game.Timer.Enabled = true;
                Game.Paused = false;
            }
            else
            {
                Game.Timer.Enabled = false;
                Game.Paused = true;
            }
        }

        static void Interval(object sender, EventArgs e)
        {
            Timer.Enabled = false;

            const long maxSlow = -130;
            long totalTime = 0;

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            while (!GameOver && !Paused)
            {
                MainForm.Invalidate();

                int timeDiff = (int)( ( totalTime += FrameRate ) - watch.ElapsedMilliseconds );
                if (timeDiff > 0)
                    System.Threading.Thread.Sleep(timeDiff);
                else if (timeDiff < maxSlow)
                    totalTime += maxSlow - timeDiff;

                RunGame();
            }
        }

        private static void RunGame()
        {
            if (Paused)
                return;

            Player.CheckCollisions();

            double total = 0;
            foreach (Piece piece in Random.Iterate(Pieces))
            {
                piece.Increment();
                if (piece.Size > 0 && piece.Type == Type.Hut)
                    total += piece.Size;
            }

            if (Random.Bool(CreateOther))
                new Piece(Type.Enemy, Random.DoubleHalf(total));
            if (Random.Bool(CreateHut))
                new Piece(Type.Hut, Random.OE(HutSize));
        }

        public static void EndGame()
        {
            if (GameOver)
                return;

            Timer.Enabled = false;

            GameOver = true;
            MainForm.Invalidate();
            ShowStats();

            using (FileStream fs = new FileStream(ScoresFile, FileMode.Append))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                uint[] uints = GetUInts(Score);
                foreach (uint ui in uints)
                    writer.Write(ui);
            }

            ShowScores();
        }

        private static void ShowStats()
        {
            StatsForm.Init();
            StatsForm.ShowDialog();
        }

        public static void ShowScores()
        {
            try
            {
                double total = 0, games = 0, high = 0;

                FileStream fs = new FileStream(ScoresFile, FileMode.OpenOrCreate);
                BinaryReader reader = new BinaryReader(fs);

                try
                {
                    while (true)
                    {
                        uint[] uints = new uint[] { reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32() };

                        double temp = GetDouble(uints);

                        high = Math.Max(temp, high);
                        games++;
                        total += temp;
                    }
                }
                catch
                {
                }

                reader.Close();
                fs.Close();
                fs.Dispose();

                if (!GameOver)
                {
                    Paused = false;
                    Pause();
                }

                System.Windows.Forms.MessageBox.Show(string.Format(
                    "Games Played:\t{0:f0}\nHighest Score:\t{1:f0}\nAverage Score:\t{2:f0}",
                    games, high, total / games), "Scores", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (!GameOver)
                {
                    Pause();
                }
            }
            catch
            {
            }
        }

        static uint[] GetUInts(double value)
        {
            uint main = (uint)value;
            uint dec = uint.Parse(( (string)( value.ToString() + ".0" ) ).Split('.')[1].PadRight(9).Substring(0, 9));

            shift(ref main, true);
            shift(ref dec, true);

            uint key = Random.NextUInt();

            main += key;
            dec += key;

            return new uint[] { main, dec, hash(dec.ToString() + main.ToString() + key.ToString()), key };
        }

        static double GetDouble(uint[] value)
        {
            uint main = value[0];
            uint dec = value[1];

            uint key = value[3];

            uint hashVal = value[2];
            if (hashVal != hash(dec.ToString() + main.ToString() + key.ToString()))
                throw new Exception();

            main -= key;
            dec -= key;

            shift(ref main, false);
            shift(ref dec, false);

            string finalVal = main.ToString() + "." + dec.ToString();
            return double.Parse(finalVal);
        }

        static void shift(ref uint value, bool direction)
        {
            if (direction)
            {
                uint v1 = value >> 13;
                uint v2 = value << 19;
                value = v1 | v2;
            }
            else
            {
                uint v1 = value >> 19;
                uint v2 = value << 13;
                value = v1 | v2;
            }
        }

        static uint hash(string value)
        {
            int extra = value.Length - value.TrimStart('~').Length;
            value = value.PadLeft(30 + extra, '~');

            uint result = 0x5C6C6A9C;

            int origHash = value.GetHashCode();
            int maxShift = 26 + origHash % 6,
                otherShift = origHash % 27,
                counter = origHash % maxShift;

            if (maxShift < 0)
                maxShift *= -1;
            if (otherShift < 0)
                otherShift *= -1;
            if (counter < 0)
                counter *= -1;

            foreach (char c in value)
            {
                uint val = Convert.ToUInt32(c) ^ (uint)c.GetHashCode();
                result += val >> ( 32 - counter );
                result += val << counter;
                if (++counter == maxShift)
                    counter = (int)( ( result >> otherShift ) % maxShift );
            }
            return result;
        }

        public static void NewGame()
        {
            const double playerSize = 1300;

            Up = false;
            Down = false;
            Left = false;
            Right = false;
            GameOver = false;
            Paused = true;

            Width = Random.GaussianCappedInt(900, .078, 550);
            Height = Random.GaussianCappedInt(650, .078, 450);

            Score = 0;

            TotalHut = 0;
            TotalAlly = 0;
            TotalEnemy = 0;
            TotalPlayer = playerSize;
            DecayHut = 0;
            DecayAlly = 0;
            DecayEnemy = 0;
            DecayPlayer = 0;
            CollectHut = 0;
            CollectAlly = 0;
            CollectEnemy = 0;

            Pieces = new List<Piece>();

            Player = new Piece(Type.Player, playerSize);

            while (Random.Bool(.78f))
            {
                double val = Random.OE(HutSize);
                double val2 = Random.DoubleHalf(val);

                new Piece(Type.Hut, val);
                new Piece(Type.Enemy, val2);
                new Piece(Type.Ally, val2);
            }

            Timer.Enabled = true;

            MainForm.Invalidate();
        }

        internal static MattUtil.PointD GetRandomPoint(double diameter)
        {
            double x, y;
            do
            {
                x = Game.Random.DoubleHalf(Game.Width - diameter);
                y = Game.Random.DoubleHalf(Game.Height - diameter);
            } while (Player != null && Player.CheckCourse(x, y, diameter));
            return new MattUtil.PointD(x, y);
        }
    }
}