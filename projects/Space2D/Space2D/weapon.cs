using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DInput = Microsoft.DirectX.DirectInput;
using dSound = Microsoft.DirectX.DirectSound;

namespace assignment4.weapons
{
	abstract class weapon : gameObject
	{
		public weapon(int radius) : base(radius) { }

		protected bool friendly;
		protected int damage;
		protected bool stops = true;

		public static List<weapon> all;

		public int Damage
		{
			get
			{
				return damage;
			}
		}

		public bool Friendly
		{
			get
			{
				return friendly;
			}
		}

		public static void inc()
		{
			int numEnemies = enemies.enemy.all.Count;
			//check collisions with each enemy
			enemies.enemy[] temp = enemies.enemy.all.ToArray();

			for (int i = all.Count; --i > -1; )
			{
				weapon w;
				(w = all[i]).y += Data.scrollSpeed;
				w.Inc();
				w.collisions(temp);
			}
		}

		public virtual void collisions(enemies.enemy[] temp)
		{
			for (int i = temp.Length; --i > -1; )
			{
				enemies.enemy e = temp[i];
				if (e.Alive && (friendly != e.Friendly) && Data.Collide(this, e))
				{
					explosionManager.createDamage(damage, x, y);

					e.hit(damage);

					if (stops)
					{
						all.Remove(this);
						break;
					}
				}
			}
		}

		public static void DrawAll(Sprite s)
		{
			foreach (weapon e in all)
				e.draw(s);
		}

		public static void loadTextures(System.Windows.Forms.ProgressBar pb)
		{
			pellet.loadTexture();
			regLaser.loadTexture();
			bomb.loadTexture();
			++pb.Value;
			ball.loadTexture();
			++pb.Value;
			nuke.loadTexture();
			++pb.Value;
			laserBeam.loadTexture();
			++pb.Value;
			capture.loadTexture();
			++pb.Value;
		}

		public static void disposeTextures()
		{
			pellet.disposeTextures();
			regLaser.disposeTextures();
			bomb.disposeTextures();
			ball.disposeTextures();
			nuke.disposeTextures();
			laserBeam.disposeTextures();
			capture.disposeTextures();
		}
	}
}