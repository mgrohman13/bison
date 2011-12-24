using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using D3D = Microsoft.DirectX.Direct3D;
using DInput = Microsoft.DirectX.DirectInput;
using dSound = Microsoft.DirectX.DirectSound;

namespace assignment4.powerUp
{
    abstract class powerUp : gameObject
    {
        public powerUp(int radius)
            : base(radius)
        {
        }

        static CreateController[] counters = new CreateController[9];

        public static void NewGame()
        {
            counters[0] = new CreateController(09000, CreateController.PowerUpType.bombs);
            counters[1] = new CreateController(13000, CreateController.PowerUpType.capture);
            counters[2] = new CreateController(06000, CreateController.PowerUpType.clone);
            counters[3] = new CreateController(06000, CreateController.PowerUpType.laserBeam);
            counters[4] = new CreateController(06000, CreateController.PowerUpType.life);
            counters[5] = new CreateController(16900, CreateController.PowerUpType.nuke);
            counters[6] = new CreateController(13000, CreateController.PowerUpType.pellets);
            counters[7] = new CreateController(03000, CreateController.PowerUpType.spinners);
            counters[8] = new CreateController(02100, CreateController.PowerUpType.Weapons);

            capture.used = false;
            capture.time = capture.captTime;

            laserBeam.NewGame();

            nuke.used = false;
            nuke.time = nuke.nukeTime;
        }

        protected bool collected = false;

        public static List<powerUp> all;

        public bool Collected
        {
            get
            {
                return collected;
            }
        }

        public Size Size
        {
            get
            {
                return size;
            }
        }

        public static void inc()
        {
            //don't allow rapid-fire nukes
            if (nuke.used)
            {
                if (--nuke.time == 0)
                {
                    nuke.used = false;
                    nuke.time = nuke.nukeTime;
                }
            }

            if (capture.used)
            {
                if (--capture.time == 0)
                {
                    capture.used = false;
                    capture.time = capture.captTime;
                }
            }

            if (bombs.used)
            {
                if (--bombs.time == 0)
                {
                    bombs.used = false;
                    bombs.time = bombs.bombTime;
                }
            }

            //check collisions and move
            for (int i = all.Count ; --i > -1 ; )
                if (i < all.Count)
                {
                    powerUp pUp;
                    if (( ( pUp = all[i] ).y += Data.scrollSpeed ) > Data.Height + pUp.size.Height / 2 && !pUp.collected)
                        all.Remove(pUp);
                    pUp.Inc();
                    if (!pUp.collected)
                        pUp.collisions();
                }
        }

        //collected by the player
        private void collisions()
        {
            if (Data.Collide(this, Data.player))
                collected = true;
        }

        //method to draw at the bottom corner
        public void Draw(Sprite s, int x, int y)
        {
            this.x = x - size.Width / 2;
            this.y = y - size.Height / 2;
            base.draw(s);
        }

        //draw non collected power ups
        public static void DrawAll(Sprite s)
        {
            foreach (powerUp e in all)
                if (!e.collected)
                    e.draw(s);
        }

        public static void loadTextures(System.Windows.Forms.ProgressBar pb)
        {
            spinners.loadTexture();
            ++pb.Value;
            nuke.loadTexture();
            pb.Value += 2;
            clone.loadTexture();
            ++pb.Value;
            life.loadTexture();
            bombs.loadTexture();
            ++pb.Value;
            Weapons.loadTexture();
            ++pb.Value;
            laserBeam.loadTexture();
            ++pb.Value;
            pellets.loadTexture();
            ++pb.Value;
            capture.loadTexture();
            pb.Value += 2;
        }

        public static void Remove(powerUp e)
        {
            all.Remove(e);
        }

        public static void create()
        {
            foreach (CreateController cc in counters)
                cc.Check();
        }

        public static void disposeTextures()
        {
            spinners.disposeTextures();
            nuke.disposeTextures();
            clone.disposeTextures();
            life.disposeTextures();
            bombs.disposeTextures();
            Weapons.disposeTextures();
            laserBeam.disposeTextures();
            pellets.disposeTextures();
            capture.disposeTextures();
        }
    }

    class CreateController
    {
        public CreateController(int rate, PowerUpType type)
        {
            this.type = type;
            this.needed = rate;
            count = Data.Random.GaussianInt(rate / 13f);
        }

        PowerUpType type;
        int count;
        int needed;

        public void Check()
        {
            float amt;
            if (++count > ( amt = Data.Random.Gaussian(needed, .03f) ))
            {
                Create();
                count -= Data.Random.Round(needed * 2 - amt);
            }
        }

        void Create()
        {
            switch (type)
            {
            case PowerUpType.bombs:
                new bombs();
                break;
            case PowerUpType.capture:
                new capture();
                break;
            case PowerUpType.clone:
                new clone();
                break;
            case PowerUpType.laserBeam:
                new laserBeam();
                break;
            case PowerUpType.life:
                new life();
                break;
            case PowerUpType.nuke:
                new nuke();
                break;
            case PowerUpType.pellets:
                new pellets();
                break;
            case PowerUpType.spinners:
                new spinners();
                break;
            case PowerUpType.Weapons:
                Weapons.create();
                break;
            }
        }

        public enum PowerUpType
        {
            spinners,
            nuke,
            life,
            clone,
            Weapons,
            laserBeam,
            pellets,
            capture,
            bombs,
        }
    }
}