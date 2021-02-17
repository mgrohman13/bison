using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DInput = Microsoft.DirectX.DirectInput;
using dSound = Microsoft.DirectX.DirectSound;

namespace assignment4.enemies
{
	abstract class enemy : gameObject
	{
		public enemy(int radius) : base(radius) { }

		protected bool friendly = false;
		protected int score, hits, maxHits;

		public static List<enemy> all;

		public bool Friendly
		{
			get
			{
				return friendly;
			}
		}

		public virtual bool Capture()
		{
			friendly = !friendly;
			//strengthen the captured unit
			if (friendly)
				Data.score += Data.Random.Round(score / 2.1f);
			hits += maxHits;
			return true;
		}

		public static void inc()
		{
			float scrollSpeed = Data.scrollSpeed;
			for (int i = all.Count; --i > -1; )
				if (i < all.Count)
				{
					enemy e;
					(e = all[i]).y += scrollSpeed;
					e.Inc();
					e.collisions(i);
				}
		}

		private void collisions(int index)
		{
			enemy[] temp = all.ToArray();

			for (int i = temp.Length; --i > index; )
			{
				enemy e = temp[i];
				if (this.Alive && e.Alive && (this.friendly != (e = temp[i]).friendly) && Data.Collide(this, e))
				{
					int min;
					e.hit(min = Math.Min(e.hits, hits));
					this.hit(min);
				}
			}
		}

		public virtual void hit(int damage)
		{
			hits -= damage;

			if (hits < 1)
			{
				all.Remove(this);
				//add dead enemy's worth to score
				if (!friendly)
					Data.score += score;

				explosionManager.createSize(size.Height * size.Width, x, y);
			}
		}

		public bool Alive
		{
			get
			{
				return hits > 0;
			}
		}

		public static void DrawAll(Sprite s)
		{
			foreach (enemy e in all)
				//player is drawn after all other ships so that it is always on top
				if (e != Data.player)
					e.draw(s);

			Data.player.draw(s);
		}

		public static void loadTextures(System.Windows.Forms.ProgressBar pb)
		{
			large.loadTexture();
			pb.Value += 2;
			spinner.loadTexture();
			++pb.Value;
			rocket.loadTexture();
			starlight.loadTexture();
			whitey.loadTexture();
			laserShip.loadTexture();
			++pb.Value;
			Skull.loadTexture(pb);
			bigSquare.loadTexture();
			++pb.Value;
			squareWeapon.loadTexture();
			otherBomber.loadTexture();
			++pb.Value;
		}

		protected static void Remove(enemy e)
		{
			if (e.friendly)
				throw new Exception();

			all.Remove(e);
			//decrease score for each enemy you let pass
			Data.score -= Data.Random.Round(e.score / 3f * e.hits / e.maxHits);
		}

		public static void create()
		{
			if (canCreate(2600))
				for (int i = Data.Random.OEInt(3.9); i > 0; --i)
					new rocket();

			if (canCreate(666))
				new large();

			if (canCreate(3000))
				new spinner();

			if (canCreate(1690))
				new starlight();

			if (canCreate(2100))
				new whitey();

			if (canCreate(3900))
				new otherBomber();

			if (canCreate(300))
				Skull.create();

			if (canCreate(6666))
				new bigSquare();

			if (canCreate(1300))
				new laserShip();
		}

		static bool canCreate(float inverseChance)
		{
			return (Data.Random.Next(Data.Random.Round(inverseChance * Math.Sqrt(Data.TimeChange / Data.frameCount))) == 0);
		}

		public static void disposeTextures()
		{
			large.disposeTextures();
			spinner.disposeTextures();
			rocket.disposeTextures();
			starlight.disposeTextures();
			whitey.disposeTextures();
			laserShip.disposeTextures();
			Skull.disposeTextures();
			bigSquare.disposeTextures();
			squareWeapon.disposeTextures();
			otherBomber.disposeTextures();
		}
	}
}