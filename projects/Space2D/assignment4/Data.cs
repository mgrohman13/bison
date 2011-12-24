using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DInput = Microsoft.DirectX.DirectInput;
using dSound = Microsoft.DirectX.DirectSound;

namespace assignment4
{
    static class Data
    {
#if TRACE
		public static Texture tracer;//, tracer2;
#endif

        public static int frameCount;
        public static int score;

        public const float TimeChange = 60000;
        public const int frameStartAmt = 60000;
        public const string path = "..\\..\\..\\";
        public static MattUtil.MTRandom Random;

        const int backSize = 1024;

        static int width, height;
        public static int Width
        {
            get
            {
                return width;
            }
        }
        public static int Height
        {
            get
            {
                return height;
            }
        }

        public const float scrollSpeed = 1.3f;

        public static player player;
        static Texture background;
        static float backY, backX;

        public static float hitMult
        {
            get
            {
                return Data.frameCount / 1.3f / Data.TimeChange;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Splash());

            Random.Dispose();
        }

        /// <summary>
        /// rotate a vector towards another
        /// </summary>
        /// <param name="direction">original direction vector</param>
        /// <param name="desired">direction to rotate towards</param>
        /// <param name="div">maximum angle to rotate</param>
        /// <returns>the new direction vector</returns>
        public static Vector3 rotateTowards(Vector3 direction, Vector3 desired, float div)
        {
            Vector3 tempDir, tempDes;
            ( tempDir = direction ).Normalize();
            ( tempDes = desired ).Normalize();

            float dot;
            if (( dot = Vector3.Dot(tempDir, tempDes) ) > 1)
                dot = 1;
            else if (dot < -1)
                dot = -1;

            direction.TransformCoordinate(Matrix.RotationAxis(Vector3.Cross(tempDir, tempDes), Math.Min(div, (float)Math.Acos(dot))));

            return direction;
        }

        public static void StartNewGame()
        {
            frameCount = frameStartAmt;

            score = 0;
            //clear lists
            enemies.enemy.all = new List<enemies.enemy>();
            weapons.weapon.all = new List<weapons.weapon>();
            powerUp.powerUp.all = new List<powerUp.powerUp>();

            backY = Data.Random.Next(backSize);
            backX = Data.Random.Next(( backSize * 2 - width ) + 1);

            //some random other game reset stuff
            player.NewGame();
            powerUp.powerUp.NewGame();
            explosionManager.NewGame();
            powerUp.Weapons.NewGame();

            //create the player
            player = new player();

            powerUp.Weapons.create();
        }

        public static void Inc()
        {
            ++frameCount;

            //increment each object
            enemies.enemy.inc();
            weapons.weapon.inc();
            powerUp.powerUp.inc();

            //create random enemies and powerups
            enemies.enemy.create();
            powerUp.powerUp.create();

            //scroll background
            backY += scrollSpeed;
            if (backY > backSize)
                backY -= backSize;
        }

        public static void Render(Microsoft.DirectX.Direct3D.Sprite sprite)
        {
            DrawBackground(sprite);

            //draw objects
            enemies.enemy.DrawAll(sprite);
            powerUp.powerUp.DrawAll(sprite);
            weapons.weapon.DrawAll(sprite);

            explosionManager.DrawExplosions(sprite);
        }

        private static void DrawBackground(Microsoft.DirectX.Direct3D.Sprite sprite)
        {
            //draw background
            sprite.Draw(background, new Rectangle(0, 0, backSize, backSize),
                new Vector3(0, 0, 1), new Vector3(-backX, backY, 1), Color.White.ToArgb());
            sprite.Draw(background, new Rectangle(0, 0, backSize, backSize),
                new Vector3(0, 0, 1), new Vector3(backSize - backX, backY, 1), Color.White.ToArgb());
            sprite.Draw(background, new Rectangle(0, 0, backSize, backSize),
                new Vector3(0, 0, 1), new Vector3(-backX, backY - backSize, 1), Color.White.ToArgb());
            sprite.Draw(background, new Rectangle(0, 0, backSize, backSize),
                new Vector3(0, 0, 1), new Vector3(backSize - backX, backY - backSize, 1), Color.White.ToArgb());
            sprite.Draw(background, new Rectangle(0, 0, backSize, backSize),
                new Vector3(0, 0, 1), new Vector3(-backX, backY + backSize, 1), Color.White.ToArgb());
            sprite.Draw(background, new Rectangle(0, 0, backSize, backSize),
                new Vector3(0, 0, 1), new Vector3(backSize - backX, backY + backSize, 1), Color.White.ToArgb());
        }

        public static void setWidthHeight(int Width, int Height)
        {
            width = Width;
            height = Height;
        }

        public static void loadTextures(ProgressBar pb)
        {
#if TRACE
			tracer = TextureLoader.FromFile(mainForm.device, path + "Debug.bmp", 0, 0, 0, Usage.Dynamic,
				Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.White.ToArgb());
#endif

            player.loadTexture();
            ++pb.Value;

            LoadBackGround();
            pb.Value += 4;

            enemies.enemy.loadTextures(pb);
            weapons.weapon.loadTextures(pb);
            powerUp.powerUp.loadTextures(pb);

            explosionManager.Init(pb);
        }

        public static void disposeTextures()
        {
            player.disposeTextures();

            disposeBackGround();

            enemies.enemy.disposeTextures();
            weapons.weapon.disposeTextures();
            powerUp.powerUp.disposeTextures();

            explosionManager.disposeTextures();
        }

        private static void disposeBackGround()
        {
            background.Dispose();
        }

        private static void LoadBackGround()
        {
            //load a random one of the backgrounds
            string[] files = System.IO.Directory.GetFiles(path + "background", "*.bmp");
            string back = files[Random.Next(files.Length)];
            background = TextureLoader.FromFile(mainForm.device, back);
        }

        internal static bool Collide(gameObject object1, gameObject object2)
        {
            float distX = object1.X - object2.X;
            float distY = object1.Y - object2.Y;
            float collisionDist = object1.Radius + object2.Radius;
            return ( distX * distX + distY * distY < collisionDist * collisionDist );
        }
    }
}