using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DInput = Microsoft.DirectX.DirectInput;
using dSound = Microsoft.DirectX.DirectSound;
using System.Drawing;
using System.Threading;

namespace assignment4
{
	static class explosionManager
	{
		public static void NewGame()
		{
			active = new List<explosion>();
		}

		const float dmgPerSize = .006f;

		static List<explosion> active;
		static Dictionary<int, explosion> loaded;

		public static void Init(System.Windows.Forms.ProgressBar pb)
		{
			loaded = new Dictionary<int, explosion>();

			//always load the smallest explosion
			string always = Data.path + "023~3~4~.bmp";
			string[] alwaysSplit = always.Split('\\');
			loaded.Add(int.Parse(alwaysSplit[alwaysSplit.Length - 1].Split('~')[0]), loadExplosion(always));

			//randomly load 10 of the other explosions

			string[] info = System.IO.Directory.GetFiles(Data.path + "explosion", "*.bmp");
			//split into groups of 10
			int num = info.Length / 10;
			int extra = info.Length % 10;
			int curVal = 0;
			for (int i = -1; ++i < 10; )
			{
				int temp = num;
				if (extra > 0)
				{
					--extra;
					++temp;
				}
				//get the group
				List<string> strings = new List<string>();
				for (int a = curVal; a < curVal + temp; ++a)
					strings.Add(info[a]);
				curVal += temp;

				//pick a random one from the group
				string newExp = strings[Data.Random.Next(strings.Count)];
				string[] fileInfo = newExp.Split('\\');
				loaded.Add(int.Parse(fileInfo[fileInfo.Length - 1].Split('~')[0]), loadExplosion(newExp));

				//increment the progress bar
				++pb.Value;
				if (i > 2)
					++pb.Value;
				if (i > 6)
					++pb.Value;
			}
		}

		public static int[] getAllKeys()
		{
			int[] result = new int[loaded.Keys.Count];
			int x = -1;
			foreach (int i in loaded.Keys)
				result[++x] = i;
			return result;
		}

		//find a random key for a specific value
		public static int getForSize(float size)
		{
			if (size < .000666)
				return 0;

			//get the chance of selecting each key
			int total = 0;
			Dictionary<int, int> chances = new Dictionary<int, int>();
			int[] keys = getAllKeys();
			foreach (int actualSize in keys)
			{
				float diff = Math.Abs(size - actualSize);
				if (diff < .000666)
				{
					return actualSize;
				}
				int chance = Data.Random.Round(1300 / (diff * diff * diff));
				total += chance;
				chances.Add(actualSize, chance);
			}

			//if there is no chance for a key
			if (total == 0)
				return getForSize(size);

			//randomly select based on the chances
			int x = Data.Random.Next(total);
			foreach (int actualSize in chances.Keys)
			{
				if (x < chances[actualSize])
				{
					return actualSize;
				}
				x -= chances[actualSize];
			}

			//should have returned a value by now
			throw new Exception();

		}

		private static explosion loadExplosion(string file)
		{
			//information for the file is stored in the file name
			string[] info = file.Split('\\');
			info = info[info.Length - 1].Split('~');
			int size = int.Parse(info[0]);
			int width = int.Parse(info[1]);
			int height = int.Parse(info[2]);

			Texture t = TextureLoader.FromFile(mainForm.device, file, 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());

			explosion newExp = new explosion(t, new Size(size, size), 0, width, height);

			return newExp;
		}

		//create an active explosion based on a picture size
		public static void createSize(int size, float x, float y)
		{
			createDamage(size * dmgPerSize, x, y);
		}

		//create an active explosion based on an amount of damage
		public static void createDamage(float damage, float x, float y)
		{
			damage = (float)Math.Sqrt(damage / dmgPerSize);
			int actual = getForSize(damage);
			ActualCreate(actual, x, y);
		}

		//create the active explosion
		static void ActualCreate(int size, float x, float y)
		{
			if (size == 0)
				return;
			explosion newExp = loaded[size].clone();
			newExp.setXY(x, y);
			active.Add(newExp);
		}

		//draw active explosions
		public static void DrawExplosions(Sprite sprite)
		{
			//System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

			for (int i = -1; ++i < active.Count; )
			{
				explosion exp = active[i];

				//stopWatch.Reset();
				//stopWatch.Start();

				exp.draw(sprite);

				//stopWatch.Stop();

				//if (stopWatch.ElapsedMilliseconds > mainForm.FrameRate / 3f)
				//{
				//    loaded.Remove(exp.Size.Width);
				//    active.Remove(exp);
				//    --i;
				//    continue;
				//}

				//remove once all the frames have been used
				if (exp.CheckFrame())
				{
					active.Remove(exp);
					--i;
				}
			}
		}

		public static void disposeTextures()
		{
			foreach (explosion exp in loaded.Values)
			{
				exp.disposeTextures();
			}
		}
	}
}