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

namespace assignment4
{
    class player : enemies.enemy
    {
        public static void NewGame()
        {
            playerInc = true;
            healing = 0;
        }

        //change these values according to the number of passed frames
        static int healAmt
        {
            get
            {
                float multThing = ( ( Data.frameCount - Data.frameStartAmt ) / Data.TimeChange );
                return Data.Random.Round(210f / ( 2.1 + multThing * multThing ));
            }
        }
        static int scoreAmt
        {
            get
            {
                float multThing = ( ( Data.frameCount - Data.frameStartAmt ) / Data.TimeChange );
                return Data.Random.Round(210f / ( 1f + multThing * multThing ));
            }
        }
        //static int maxLife = 100;
        //public static void AddMaxHits()
        //{
        //    maxLife += 10;
        //}
        public const int MaxHits = 100;

        static bool playerInc;
        bool isPlayer;
        int lives;
        int reload, deathCounter;
        public const int reloadTime = 30;
        static int healing;

        static Texture playerTexture, allyTexture;

        const float playerSpeed = 3.9f;

        int dir;

        public int Hits
        {
            get
            {
                return hits;
            }
        }

        public int Lives
        {
            get
            {
                return lives;
            }
        }

        public bool Dead
        {
            get
            {
                return animate;
            }
        }

        //public override Rectangle rect
        //{
        //    get
        //    {
        //        if (animate) //dont allow collisions when already dead
        //            return new Rectangle(-1111111, -1111111, 1, 1);
        //        else
        //            return base.rect;
        //    }
        //}

        public override bool Capture()
        {
            //dont allow the actual player to be captured
            if (!isPlayer)
                base.Capture();
            return ( !isPlayer );
        }

        public player()
            : base(26)
        {
            isPlayer = playerInc;
            playerInc = false;
            if (isPlayer)
                hits = 100;
            else
                hits = Data.Random.GaussianCappedInt(100 * Data.hitMult, .06f, 13);
            lives = 2;
            reload = reloadTime;

            this.curFrame = 0;
            this.height = 4;
            if (isPlayer)
                this.pic = playerTexture;
            else
                this.pic = allyTexture;
            this.size = new Size(80, 80);
            this.width = 4;
            this.x = Data.Random.Next(Data.Width);
            this.y = Data.Height / 2;

            friendly = true;

            dir = Data.Random.Next(10);

            enemies.enemy.all.Add(this);

            //only animate when dead
            animate = false;
        }

        public void addLife()
        {
            ++lives;
        }

        public override void hit(int damage)
        {
            hits -= damage;

            if (hits < 1)
            {
                explosionManager.createSize(size.Height * size.Width, x, y);

                if (isPlayer)
                {
                    --lives;
                    animate = true; //marks that the player is dead
                    deathCounter = 169; //time to be dead
                }
                else
                    enemies.enemy.all.Remove(this);
            }
        }

        public static void loadTexture()
        {
            playerTexture = TextureLoader.FromFile(mainForm.device, Data.path + "ship.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
            allyTexture = TextureLoader.FromFile(mainForm.device, Data.path + "ships\\clone.bmp", 0, 0, 0, Usage.Dynamic,
                Format.Unknown, Pool.Default, Filter.None, Filter.None, Color.Black.ToArgb());
        }

        public override void Inc()
        {
            if (animate && deathCounter < 0 && curFrame == 0)
            {
                hits = MaxHits;
                animate = false;
            }

            --deathCounter;
            --reload;

            if (isPlayer)
            {
                //if (++healing > healAmt && hits < MaxHits)
                //{
                //    healing -= healAmt;
                //    ++hits;
                //}
                //else if (healing > scoreAmt)
                //{
                //		healing -= scoreAmt;
                //    ++Data.score;
                //}

                if (animate)
                {
                    healing = 0;
                }
                else
                {
                    float healAmt = ( 50f + hits ) / 10f;
                    healAmt *= healAmt;
                    if (++healing > healAmt)
                    {
                        healing -= Data.Random.Round(healAmt);
                        ++hits;
                    }
                }

                //move with wasd or arrows
                KeyboardState state;
                if (( state = mainForm.state )[Key.W] || state[Key.UpArrow] || state[Key.Up])
                    y -= playerSpeed;
                if (state[Key.A] || state[Key.LeftArrow] || state[Key.Left])
                    x -= playerSpeed;
                if (state[Key.S] || state[Key.DownArrow] || state[Key.Down])
                    y += playerSpeed;
                if (state[Key.D] || state[Key.RightArrow] || state[Key.Right])
                    x += playerSpeed;

                if (state[Key.Space] && reload < 0 && !animate)
                {
                    reload = reloadTime;
                    new weapons.regLaser(x, y, true);
                }
            }
            else //for clones - not the real player
            {
                //random movement
                switch (dir)
                {
                case 0:
                    y += playerSpeed;
                    break;
                case 1:
                    y -= playerSpeed;
                    break;
                case 2:
                    y += playerSpeed;
                    x -= playerSpeed;
                    break;
                case 3:
                    y -= playerSpeed;
                    x -= playerSpeed;
                    break;
                case 4:
                    y += playerSpeed;
                    x += playerSpeed;
                    break;
                case 5:
                    y -= playerSpeed;
                    x += playerSpeed;
                    break;
                case 6:
                    x += playerSpeed;
                    break;
                case 7:
                    x -= playerSpeed;
                    break;
                default:
                    break;
                }

                if (Data.Random.Bool(.01f))
                    dir = Data.Random.Next(10);

                if (reload < 0)
                {
                    reload = reloadTime;
                    new weapons.regLaser(x, y, true);
                }
            }

            //stop at screen edges
            if (x < 0 + size.Width / 2)
                x = 0 + size.Width / 2;
            else if (x > Data.Width - size.Width / 2)
                x = Data.Width - size.Width / 2;
            if (y < 0 + size.Height / 2)
                y = 0 + size.Height / 2;
            else if (y > Data.Height - size.Height / 2)
                y = Data.Height - size.Height / 2;
        }

        new public static void disposeTextures()
        {
            playerTexture.Dispose();
            allyTexture.Dispose();
        }
    }
}