using System.Collections;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Timers;
using System.IO;

namespace Trogdor
{
	static class Game
	{
		public const double createChildTime = .00333;
		public const double createTime = .00666;
		public const int FrameRate = 26;
		public const double death = .666;
		public const double playerSpeed = .6;
		public const double playerOffset = .21;
		public const double otherOffset = .9;
		public const double otherSpeed = .0003;
		public const double hitDamage = 13;
		public const double hutSize = 300;

		private static Stats statsForm;
		public static System.Timers.Timer timer = new System.Timers.Timer(333);
		public static int MaxWidth, MaxHeight;
		public static bool up, down, left, right, gameOver, paused;
		public static double score;
		public static ArrayList pieces;
		public static MattUtil.MTRandom rand;
		private static Main form;

		internal static double totalHut;
		internal static double totalAlly;
		internal static double totalEnemy;
		internal static double totalPlayer;
		internal static double decayHut;
		internal static double decayAlly;
		internal static double decayEnemy;
		internal static double decayPlayer;
		internal static double collectHut;
		internal static double collectAlly;
		internal static double collectEnemy;

		const string file = "inf.dat";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			if (!File.Exists(file))
				File.Create(file);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			timer.Elapsed += new ElapsedEventHandler(Interval);
			timer.Enabled = false;

			form = new Main();
			statsForm = new Stats();

			MaxHeight = form.ClientSize.Height;
			MaxWidth = form.ClientSize.Width;

			NewGame();

			Application.Run(form);
		}

		public static void Pause()
		{
			if (paused)
			{
				Game.timer.Enabled = true;
				Game.paused = false;
			}
			else
			{
				Game.timer.Enabled = false;
				Game.paused = true;
			}
		}

		//static double tot, count;

		static void Interval(object sender, EventArgs e)
		{
			timer.Enabled = false;

			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

			while (!gameOver && !paused)
			{
				watch.Reset();
				watch.Start();

				RunGame();

				form.Invalidate();

				//count++;
				//tot += watch.ElapsedMilliseconds;

				//if (count > 1000)
				//{
				//    double meh = tot / count;
				//}

				long timeDiff = FrameRate - watch.ElapsedMilliseconds;
				if (timeDiff > 0)
					System.Threading.Thread.Sleep((int)timeDiff);

				watch.Stop();
			}
		}

		private static void RunGame()
		{
			if (paused)
				return;

			ArrayList temp = (ArrayList)pieces.Clone();

			foreach (Piece piece in temp)
			{
				piece.Increment();
			}

			if (rand.Bool(createChildTime))
			{
				double total = 0;

				foreach (Piece piece in pieces)
				{
					if (piece.Type == Type.Hut)
						total += piece.Size;
				}

				double size = rand.DoubleHalf(total);
				totalEnemy += size;
				pieces.Add(new Piece(Type.Enemy, size));
			}

			if (rand.Bool(createTime))
			{
				double size = rand.OE(hutSize);
				totalHut += size;
				pieces.Add(new Piece(Type.Hut, size));
			}

			temp = (ArrayList)pieces.Clone();

			foreach (Piece piece in temp)
			{
				if (piece.Type == Type.Player)
				{
					piece.checkCollisions();
					break;
				}
			}
		}

		public static void GameOver()
		{
			if (gameOver)
				return;

			gameOver = true;

			timer.Enabled = false;

			FileStream fs = new FileStream(file, FileMode.Append);
			BinaryWriter writer = new BinaryWriter(fs);

			uint[] uints = GetUInts(score);

			foreach (uint ui in uints)
				writer.Write(ui);

			writer.Flush();
			writer.Close();
			fs.Close();
			fs.Dispose();

			ShowStats();
			ShowScores();
		}

		private static void ShowStats()
		{
			statsForm.Init();
			statsForm.ShowDialog();
		}

		public static void ShowScores()
		{
			try
			{
				double total = 0, games = 0, high = 0;

				FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
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
				catch { }

				reader.Close();
				fs.Close();
				fs.Dispose();

				if (!gameOver)
				{
					paused = false;
					Pause();
				}

				System.Windows.Forms.MessageBox.Show(string.Format(
					"Games Played:\t{0:f0}\nHighest Score:\t{1:f0}\nAverage Score:\t{2:f0}",
					games, high, total / games), "Scores", MessageBoxButtons.OK, MessageBoxIcon.Information);

				if (!gameOver)
				{
					Pause();
				}
			}
			catch { }
		}

		static uint[] GetUInts(double value)
		{
			uint main = (uint)value;
			uint dec = uint.Parse(((string)(value.ToString() + ".0")).Split('.')[1].PadRight(9).Substring(0, 9));

			shift(ref main, true);
			shift(ref dec, true);

			uint key = rand.NextUInt();

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

			if (maxShift < 0) maxShift *= -1;
			if (otherShift < 0) otherShift *= -1;
			if (counter < 0) counter *= -1;

			foreach (char c in value)
			{
				uint val = Convert.ToUInt32(c) ^ (uint)c.GetHashCode();
				result += val >> (32 - counter);
				result += val << counter;
				if (++counter == maxShift)
					counter = (int)((result >> otherShift) % maxShift);
			}
			return result;
		}

		public static void NewGame()
		{
			const double playerSize = 1300;

			rand = new MattUtil.MTRandom();

			up = false;
			down = false;
			left = false;
			right = false;
			gameOver = false;
			paused = false;

			score = 0;

			totalHut = 0;
			totalAlly = 0;
			totalEnemy = 0;
			totalPlayer = playerSize;
			decayHut = 0;
			decayAlly = 0;
			decayEnemy = 0;
			decayPlayer = 0;
			collectHut = 0;
			collectAlly = 0;
			collectEnemy = 0;

			pieces = new ArrayList();

			pieces.Add(new Piece(Type.Player, playerSize));

			while (rand.Bool(.75))
			{
				double val = rand.OE(hutSize);

				double val2 = rand.DoubleHalf(val);

				totalAlly += val2;
				pieces.Add(new Piece(Type.Ally, val2));
				totalEnemy += val2;
				pieces.Add(new Piece(Type.Enemy, val2));
				totalHut += val;
				pieces.Add(new Piece(Type.Hut, val));
			}

			timer.Enabled = true;
		}
	}
}