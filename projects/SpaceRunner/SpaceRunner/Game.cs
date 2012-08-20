using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MattUtil.RealTimeGame;
using GameForm = SpaceRunner.Forms.GameForm;
using Point = MattUtil.Point;

namespace SpaceRunner
{
    internal class Game : MattUtil.RealTimeGame.Game, IDisposable
    {
        #region Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        internal static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if TRACE
            ScoresForm.Scoring = false;
#endif
            GameForm mainForm = new GameForm();
            StaticInit();

            Application.Run(mainForm);

            GameForm.Game.Dispose();
            StaticDispose();
            mainForm.Dispose();
        }

        private static void StaticInit()
        {
            Game game = GameForm.Game;
            game.InitGame(Random.Round(MapSize), Random.Round(MapSize), false, false);
            game.Running = false;
            game.Started = false;
            game.Dispose();

            InitializeImages();
        }
        //InitializeImages serves 2 purposes:
        // -call the static constructor for each object type to load all images
        // -show something interesting on the screen when the game is first launched
        private static void InitializeImages()
        {
            Game game = GameForm.Game;

            AlienShip.NewAlienShip();
            PointF p = game.RandomStartPoint(AlienShipSize);
            LifeDust.NewLifeDust(p.X, p.Y, NumLifeDustImages);

            int amt = Random.GaussianCappedInt(2, 1);
            while (--amt > -1)
            {
                p = game.RandomStartPoint(AlienSize);
                Alien.NewAlien(p.X, p.Y);
            }
            amt = Random.GaussianCappedInt(4, 1);
            while (--amt > -1)
            {
                p = game.RandomStartPoint(AsteroidMaxSize);
                Asteroid.NewAsteroid(p.X, p.Y);
            }
            p = game.RandomStartPoint(PowerUpSize);
            PowerUp.NewPowerUp(p.X, p.Y);

            p = game.RandomStartPoint(0);
            Bullet.BulletExplosion(p.X, p.Y, 1);
            p = game.RandomStartPoint(0);
            Explosion.NewExplosion(new GameObject.DummyObject(p.X, p.Y, 0, 0));
            p = RandomEdgePoint();
            FuelExplosion.NewFuelExplosion(p.X, p.Y);

            amt = Random.OEInt(6);
            while (--amt > -1)
            {
                game.MoveAndCollide(0, 0, 0);

                game.life = StartLife;
                game.deadCounter = -1;
            }
        }
        private static void StaticDispose()
        {
            PlayerImage.Dispose();
            NoAmmoImage.Dispose();
            Font.Dispose();

            PowerUp.Dispose();
            LifeDust.Dispose();
            FuelExplosion.Dispose();
            Explosion.Dispose();
            Bullet.Dispose();
            Asteroid.Dispose();
            AlienShip.Dispose();
            Alien.Dispose();
        }

        #endregion //Main

        #region consts

        //miliseconds per game iteration
        internal const float GameTick = 1000 / 65f;

        private const string PicLocation = "..\\..\\..\\pics\\";
        public override string ScoreFile
        {
            get
            {
                return PicLocation + "pics.dat";
            }
        }

        //should reflect actual information about the images in PicLocation
        internal const int NumExplosionImages = 5;
        internal const int NumImagesPerExplosion = 15;
        internal const int NumAsteroidImages = 8;
        internal const int NumFuelExplosionImages = 6;
        internal const int NumLifeDustImages = 6;
        internal const int NumFireworks = 13;

        //mathematical values
        internal const float HalfPi = (float)( Math.PI / 2 );
        internal const float TwoPi = (float)( Math.PI * 2 );
        internal const float RadToDeg = (float)( 180 / Math.PI );

        #endregion //consts

        #region game params

        // object	radius	diameter   area
        //life dust	 1.75     3.5	     9.6
        //bullet	 2.5      5.	    19.6
        //asteroid	 8.8	 17.6	   242.6    (min)
        //fuel exp.	 9.0     18.0	   254.5    (min)
        //power up	 9.      18.	   254.5
        //alien	    13. 	 26.	   530.9
        //explosion	15.7	 31.5	   777.3
        //asteroid	16.9	 33.8	   897.3    (avg)
        //player	17. 	 34.	   907.9
        //alien s.	21. 	 42.	 1,385.4
        //asteroid	25.0	 50.0	 1,965.4    (max)
        //fuel exp.	91.0	182.0	26,015.5    (max)

        //size constants are the radius of the object

        internal const float MapSize = 338f;
        //distance from the center of the map at which new objects are created
        internal const float CreationDist = MapSize + 39f;
        //distance from the CreationDist at which objects have a 50% chance of being removed per pixel the player moves
        internal const double RemovalDist = MapSize * MapSize * 1.3;
        //sectors for collision detection
        internal const float SectorSize = ( AsteroidMaxSize + FuelExplosionSize ) / 2;

#if TRACE
        internal const float GameSpeed = GameTick * 0.021f;
#else
        internal const float GameSpeed = (float)( GameTick * Math.PI * .013 );
#endif

        internal const float PlayerSize = 17f;
        internal const float BasePlayerSpeed = GameSpeed * 3f;
        //time spent dead before getting the next life
        internal const float DeathTime = 1 / GameSpeed * 65f;

#if TRACE
        internal const float StartLife = PlayerLife * 30f;
#else
        internal const float StartLife = PlayerLife * 3f;
#endif
        internal const float PlayerLife = 13f;
        internal const float PlayerDamageRandomness = .065f;

        internal const float StartFuel = FuelMult * 15;
        //average fuel per power up
        internal const float IncFuel = FuelMult * 3;
        internal const float IncFuelRandomness = .104f;
        //how many extra pixels each fuel point will take you
        internal const float FuelMult = 130f;
        //percentage of fuel consumed each iteration when using turbo
        internal static readonly double FuelRate = GameSpeed * 0.13;
        //exponent of fuel consumption
        internal const double FuelPower = .39;

        internal const int StartAmmo = 10;
        internal const int IncAmmo = 3;
        //time spent reloading (will be divided by ammo^FireTimePower)
        internal const double FireTimeMult = 1 / GameSpeed * 3900.0;
        internal const double FireTimePower = 1.69;
        //constant added to current ammo for fire time calculation
        internal const double FireTimeAmmoAdd = 1.69;

        private const float DeadBlinkDiv = 300f / GameTick;
        private const float DeadBlinkWindow = DeadBlinkDiv / 2.1f;

        //chances of objects being created each iteration (will be multiplied by players current speed)
        internal const float LifeDustCreationRate = (float)( Math.E * .0013 );
        internal const float PowerUpCreationRate = 0.0021f;
        internal const float AsteroidCreationRate = .078f;
        internal const float AlienCreationRate = .013f;
        //will be divided by number of alien ships
        internal const float AlienShipCreationRate = .00013f;

        internal const float AlienSize = 13f;
        internal const float AlienSpeed = GameSpeed * 2.6f;
        internal const float AlienSpeedRandomness = .39f;
        //fuel power up
        internal const float AlienSpeedInc = GameSpeed * 3f;
        //only when an alien picks up a first ammo power up
        internal const float AlienFireRate = (float)( GameSpeed * Math.PI / 130 );
        //subsequent ammo power ups
        internal const float AlienFireRateInc = GameSpeed * .0117f;
        //randomness for power up values
        internal const float AlienIncRandomness = .13f;
        //lower cap as a percentage of the average value
        internal const float AlienIncCap = .39f;
        internal const float AlienConstSpeedReduceWeight = .65f;

        //for both aliens and alien ships
        internal const float AlienDamageRandomness = .078f;
        internal const float AlienDamageOEPct = .26f;

        internal const float AlienShipSize = 21f;
        internal const float AlienShipLife = 260f;
        internal const float AlienShipLifeInc = 210f;
        internal const float AlienShipFireRate = GameSpeed * .021f;
        internal const float AlienShipFireRateInc = GameSpeed * .0104f;
        internal const float AlienShipSpeedMult = 1.69f;
        internal const float AlienShipSpeedMultInc = .39f;
        internal const float AlienShipStatRandomness = .169f;
        //lower cap as a percentage of the average value
        internal const float AlienShipStatCap = .13f;
        internal const float AlienShipFriendlyBulletDamageMult = 13f;
        internal const float AlienShipNeutralBulletDamageMult = AlienShipFriendlyBulletDamageMult / AlienShipExplosionBullets * 3f;
        internal const float AlienShipFuelExplosionDamageMult = 7.8f;
        //average number of bullets in the explosion on death
        internal const float AlienShipExplosionBullets = 5.2f;

        internal const float AsteroidAverageSize = 16.9f;
        internal const float AsteroidSizeRandomness = .091f;
        internal const float AsteroidMaxSize = AsteroidAverageSize * ( 2 - AsteroidSizeCap );
        //lower cap as a percentage of the average value
        internal const float AsteroidSizeCap = .52f;
        //damage to player and alien ship
        internal const float AsteroidAreaToDamageRatio = (float)( Math.PI * PlayerSize * PlayerSize / PlayerLife ) / 1.3f;
        //alien life is actually its speed in pixels, so this damage is in pixels
        internal const float AsteroidAreaToAlienDamageRatio = 1 / GameSpeed * 169f;
        //drift speed for new asteroids
        internal const float AsteroidInitialSpeed = GameSpeed * .13f;
        //when an asteroid breaks, the exponent for the chance (based on the breaking astroid size) of each new smaller piece to be created
        internal const float AsteroidPieceChancePower = .6f;
        //average number of smaller asteroids created when a larger one breaks
        internal const float AsteroidPieces = 2.6f;
        internal const float AsteroidPiecesRandomness = .117f;
        //speed of new smaller asteroids when a larger one breaks
        internal const float AsteroidPieceSpeed = GameSpeed * 1.69f;
        internal const float AsteroidPieceSpeedRandomness = .21f;
        //chance (based on the difference in asteroid area) for the asteroids to both break
        internal const float AsteroidCollisionChance = 520f;
        //asteroids smaller than this area are frequently destroyed uneventfully when colliding with other asteroids
        internal const float AsteroidCollisionCriticalArea = 117f;
        internal const float AsteroidRotateConst = GameSpeed * 0.0065f;
        internal const float AsteroidRotateMult = GameSpeed / AsteroidPieceSpeed * 0.0169f;

        internal const float BulletSize = 2.5f;
        //speed added to the speed of the object firing the bullet
        internal const float BulletSpeed = (float)( GameSpeed * Math.PI );
        //damage to player and alien ship (bullets always kill aliens and asteroids)
        internal const float BulletDamage = 3.9f;
        //average speed of bullets from bullet explosions
        internal const float BulletExplosionSpeed = BulletSpeed * 1.3f;
        internal const float BulletExplosionSpeedRandomness = .52f;
        //randomness on the angle (in radians) when an alien shoots at the player
        internal const float AlienFiringInaccuracy = 0.052f;
        //chance that, when a bullet hits and kills a piece of life dust, the bullet will also be killed
        internal const float BulletLifeDustDieChance = 6f / ( 6f + LifeDustClumpAmt );

        internal const float FuelExplosionSize = 91f;
        //number of iterations a fuel explosion lasts
        internal const float FuelExplosionTime = 1 / GameSpeed * 65f;
        //an object at the center of the explosion is cosidered to be this distance from it for damage purposes
        internal const float FuelExplosionDamageStartDist = FuelExplosionSize * .3f;
        //damage done each iteration to the player or an alien ship inside the explosion
        internal const float FuelExplosionDamage = 1 / FuelExplosionTime * 30f;
        internal const float FuelExplosionImagesPerSecond = 26f;

        //for both fuel and standard explosions
        internal const float ExplosionRotate = GameSpeed * 0.0052f;
        //speed the explosion shockwave is considered to be traveling
        internal const float ExplosionSpeed = ( FuelExplosionSize - PowerUpSize ) / FuelExplosionTime;

        internal const float ExplosionSize = AlienSize * 1.21f;
        internal const float ExplosionTime = 1 / GameSpeed * 39f;
        internal static readonly float ExplosionSpeedMult = (float)Math.Pow(1 - .052, GameSpeed);

        internal const float LifeDustSize = 1.75f;
        //average amount in new clumps
        internal const float LifeDustClumpAmt = 13f;
        internal const float LifeDustClumpOEPct = .13f;
        internal const float LifeDustAmtRandomness = .3f;
        //initial spacing between objects in a clump
        internal const float LifeDustSpacing = LifeDustSize * 2;
        //speed of the entire clump
        internal const float LifeDustClumpSpeed = GameSpeed * .3f;
        //speed of each individual
        internal const float LifeDustIndividualSpeed = GameSpeed * .013f;
        //exponent to the speed picked up from collisions with other objects
        internal const double LifeDustObjSpeedPower = GameSpeed / ( GameSpeed + .169 );
        //chance of life dust getting hit by a bullet or fuel explosion
        internal const float LifeDustHitChance = GameTick * 0.0065f;
        //how many particles needed to fully heal, also the amount in a clump created when a life power up explodes
        internal const float LifeDustAmtToHeal = 52f;

        internal const float PowerUpSize = 9f;
        //these three chance value are only relative to one another
        internal const int PowerUpAmmoChance = 5;
        internal const int PowerUpFuelChance = 6;
        internal const int PowerUpLifeChance = 2;
        //average number of bullets in the explosion when an ammo power up explodes
        internal const float PowerUpAmmoExplosionBullets = 13f;
        internal const float PowerUpRotate = GameSpeed * 0.03f;

        internal const decimal ScoreMult = 10m;
        //amount each damage point while dead reduces your score
        internal const decimal ScoreToDamageRatio = ScoreMult / (decimal)PlayerLife * 10m;
        //score added per pixel traveled
        internal const decimal DistanceScore = ScoreMult / (decimal)MapSize / 2m;
        //score an alien is worth for killing based on its speed
        internal const decimal AlienSpeedScoreMult = ScoreMult / (decimal)AlienSpeed * 1m;
        //extra score an alien that shoots is worth for killing
        internal const decimal AlienFireRateScoreMult = ScoreMult / (decimal)AlienFireRate * 1.5m;
        //score an alien ship is worth for injuring based on its stats compared to average
        internal const decimal AlienShipScoreMult = ScoreMult * 150m;
        //score an alien ship is worth for killing based on its stats compared to average
        internal const decimal AlienShipDeathScoreMult = AlienShipScoreMult / 10m;
        internal const decimal RemainingAmmoScore = ScoreMult;
        internal const decimal RemainingFuelScore = ScoreMult / (decimal)FuelMult / 2m;

        #endregion //game params

        #region fields

        private static readonly Font Font;
        private static readonly Image PlayerImage, NoAmmoImage;

        static Game()
        {
            Font = new Font("Arial", 12.75f, FontStyle.Bold);
            PlayerImage = LoadImage("player.bmp", PlayerSize);
            NoAmmoImage = LoadImage("noammo.bmp", PlayerSize);
        }

        //for fireworks only
        private bool fireworks, headingAngleDir;
        private float headingAngle;

        private HashSet<GameObject> objects;
        private int centerX, centerY, inputX, inputY;
        private decimal score;
        private float life, fuel;
        private int ammo;
        private int deadCounter, fireCounter, alienCount;
        internal bool Turbo;
        internal bool Fire;

        #endregion //fields

        #region properties

        internal bool Fireworks
        {
            get
            {
                return fireworks;
            }
        }

        public override decimal Score
        {
            get
            {
                return score;
            }
        }

        internal int Lives
        {
            get
            {
                return (int)Math.Ceiling(life / PlayerLife);
            }
        }
        internal float CurrentLifePart
        {
            get
            {
                float retVal = life % PlayerLife;
                return ( retVal > 0 ? retVal : PlayerLife );
            }
        }

        internal int Ammo
        {
            get
            {
                return ammo;
            }
        }

        internal float Fuel
        {
            get
            {
                return fuel / FuelMult;
            }
        }

        internal bool Dead
        {
            get
            {
                return ( deadCounter > -1 );
            }
        }

        public override bool GameOver()
        {
            return ( life <= 0 );
        }

        internal float TurboSpeed
        {
            get
            {
                return (float)Math.Min(Math.Pow(fuel, FuelPower) * FuelRate, fuel);
            }
        }
        internal float TotalSpeed
        {
            get
            {
                return BasePlayerSpeed + ( Turbo ? TurboSpeed : 0 );
            }
        }

        #endregion //properties

        #region internal functional methods

        public override void Draw(System.Drawing.Graphics graphics)
        {
#if TRACE
            graphics.ResetTransform();
            graphics.DrawEllipse(Pens.White, centerX - MapSize, centerY - MapSize, MapSize * 2, MapSize * 2);
            int drawSectors = (int)Math.Ceiling(CreationDist / SectorSize);
            for (int sect = -drawSectors ; sect <= drawSectors ; ++sect)
            {
                float x = centerX + ( sect * SectorSize );
                graphics.DrawLine(Pens.White, x, centerY - CreationDist, x, centerY + CreationDist);
                float y = centerY + ( sect * SectorSize );
                graphics.DrawLine(Pens.White, centerX - CreationDist, y, centerX + CreationDist, y);
            }
            graphics.DrawEllipse(Pens.White, centerX - PlayerSize, centerY - PlayerSize, PlayerSize * 2, PlayerSize * 2);
#endif
            DrawPlayer(graphics);
            if (!Paused && !GameOver())
            {
                DrawFireBar(graphics);
                DrawHealthBar(graphics);
            }

            if (Paused && !GameOver())
                DrawPaused(graphics);
            else
                DrawObjects(graphics);
        }

        private void DrawPlayer(Graphics graphics)
        {
            //not drawing when deadCounter is within a certain range causes the player to blink when dead
            if (!Dead || GameOver() || Paused || !Started || ( deadCounter % DeadBlinkDiv > DeadBlinkWindow ))
            {
                //choose between regular image and no ammo image
                Image image = ( ( fireCounter < 0 || GameOver() || Paused || Fireworks ) ? PlayerImage : NoAmmoImage );
                GameObject.DrawImage(graphics, image, centerX, centerY, 0, 0, 0, PlayerSize, GetAngleImageAdjusted(inputX, inputY));
            }
        }
        internal static float GetAngleImageAdjusted(float xSpeed, float ySpeed)
        {
            return GetAngle(xSpeed, ySpeed) + HalfPi;
        }

        private void DrawFireBar(Graphics graphics)
        {
            if (fireCounter > -1 || ammo < 1)
            {
                Rectangle bounds = GetBarRect(centerX, centerY, PlayerSize);
                bounds.Height += 2;
                bounds.Y += bounds.Height;
                float coolDown = ( ammo < 1 ? 1 : fireCounter / GetCoolDown() );
                DrawBar(graphics, Pens.White, Brushes.Cyan, bounds, coolDown);
            }
        }
        private void DrawHealthBar(Graphics graphics)
        {
            Brush brush = ( Dead ? Brushes.Lime : Brushes.White );
            float pct = ( Dead ? deadCounter / DeathTime : CurrentLifePart / PlayerLife );
            DrawHealthBar(graphics, Pens.White, brush, centerX, centerY, PlayerSize, pct);
        }

        private void DrawPaused(Graphics graphics)
        {
            const string PausedText = "PAUSED";
            float drawX = centerX - graphics.MeasureString(PausedText, Font).Width / 2f;

            graphics.ResetTransform();
            graphics.DrawString(PausedText, Font, Brushes.White, drawX, centerY + PlayerSize);
        }

        private void DrawObjects(Graphics graphics)
        {
            int count;
            GameObject[] array;
            lock (gameTicker)
            {
                count = objects.Count;
                array = new GameObject[count];
                objects.CopyTo(array, 0);
            }

            Array.Sort<GameObject>(array, delegate(GameObject p1, GameObject p2)
            {
                return GetDrawPriority(p1) - GetDrawPriority(p2);
            });

            for (int a = 0 ; a < count ; ++a)
            {
                GameObject obj = array[a];
                lock (gameTicker)
                    if (objects.Contains(obj))
                        obj.Draw(graphics, centerX, centerY);
            }
        }

        private static int GetDrawPriority(GameObject obj)
        {
            //z-index: higher values are drawn on top of lower values
            if (obj is Alien)
                return 4000;
            if (obj is AlienShip)
                return 5000 - (int)( 999 * obj.Y / ( MapSize + obj.Size ) );
            if (obj is Asteroid)
                return 1000;
            if (obj is Bullet)
                return 6000;
            if (obj is Explosion)
                return 8000;
            if (obj is FuelExplosion)
                return 7000;
            if (obj is LifeDust)
                return 3000;
            if (obj is PowerUp)
                return 2000;
#if DEBUG
            throw new Exception();
#else
            return 0;
#endif
        }

        internal static void DrawHealthBar(Graphics graphics, GameObject obj, float pct)
        {
            DrawHealthBar(graphics, Pens.White, Brushes.White, GameForm.Game.centerX + obj.X, GameForm.Game.centerY + obj.Y, obj.Size, pct);
        }
        private static void DrawHealthBar(Graphics graphics, Pen border, Brush fill, float x, float y, float size, float pct)
        {
            DrawBar(graphics, border, fill, GetBarRect(x, y, size), pct);
        }
        private static void DrawBar(Graphics graphics, Pen border, Brush fill, Rectangle rect, float pct)
        {
            Rectangle fillRect = new Rectangle(rect.X + 1, rect.Y + 1, Round(pct * ( rect.Width - 1 )), rect.Height - 1);

            graphics.ResetTransform();
            graphics.DrawRectangle(border, rect.X, rect.Y, rect.Width, rect.Height);
            graphics.FillRectangle(fill, fillRect);
        }
        private static Rectangle GetBarRect(float x, float y, float size)
        {
            return new Rectangle(Round(x - size), Round(y + size), Round(size * 2), 3);
        }

        internal void AddObject(GameObject obj)
        {
            objects.Add(obj);

            if (obj is AlienShip)
                ++alienCount;
        }
        internal void RemoveObject(GameObject obj)
        {
            objects.Remove(obj);

            IDisposable disposable;
            if (( disposable = obj as IDisposable ) != null)
                disposable.Dispose();
            if (obj is AlienShip)
                --alienCount;
        }

        internal void SetMouseCoordinates(int x, int y)
        {
            if (!GameOver() && Running && !( GetDistanceSqr(x, y) > MapSize * MapSize ))
            {
                if (!Started)
                {
                    Start();
                }
                else if (Paused)
                {
                    Paused = false;
                    //this fixes an exploit where you could use pause to instantly turn and fire any direction you wanted
                    lock (gameTicker)
                    {
                    }
                    System.Threading.Thread.Sleep(Random.Round(GameTick));
                    lock (gameTicker)
                    {
                    }
                }

                if (x == 0 && y == 0)
                {
                    SetRandomInput();
                }
                else
                {
                    inputX = x;
                    inputY = y;
                }
            }
            else if (Started)
            {
                //pause the game if the mouse is outside the playing area
                Paused = true;
            }
        }

        internal void AddScore(decimal amt)
        {
            score += amt;
        }
        internal void AddAmmo()
        {
            ammo += IncAmmo;
            fireCounter = -1;
        }
        internal void AddFuel()
        {
            fuel += Random.GaussianCapped(IncFuel, IncFuelRandomness);
        }
        internal void AddLife(float amt, bool allowNew)
        {
            if (allowNew)
            {
                AddLife(amt);
            }
            else
            {
                float maxLifeRegen = PlayerLife - CurrentLifePart;
                if (amt < maxLifeRegen)
                    amt = Random.GaussianCapped(amt, PlayerDamageRandomness);
                if (amt > maxLifeRegen)
                {
                    AddLife(maxLifeRegen);
                    amt -= maxLifeRegen;
                    //add to score when not regenerating life
                    AddScore((decimal)amt * ScoreToDamageRatio);
                }
                else
                {
                    AddLife(amt);
                }
            }
        }
        private void AddLife(float amt)
        {
            life += amt;
        }

        #endregion //internal functional methods

        #region internal abstraction methods

        internal static float RandDmgToAlien(float amt)
        {
            return Random.GaussianOE(amt, AlienDamageRandomness, AlienDamageOEPct, 0);
        }

        public void Dispose()
        {
            if (objects != null)
            {
                foreach (GameObject obj in objects)
                {
                    IDisposable disposable = obj as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                objects.Clear();
            }
        }

        private void SetRandomInput()
        {
            float fx, fy;
            GetRandomDirection(out fx, out fy, MapSize);
            inputX = Random.Round(fx);
            inputY = Random.Round(fy);
        }

        internal static float GetRingSpacing(int numPieces, double size)
        {
            return (float)( size * ( numPieces < 3 ? 1 : 1 / Math.Sin(Math.PI / numPieces) ) );
        }

        internal static void GetRandomDirection(out float xDir, out float yDir, float dist)
        {
            float angle = GetRandomAngle();
            GetDirs(out xDir, out yDir, angle, dist);
        }
        internal static void GetDirs(out float xDir, out float yDir, float angle)
        {
            GetDirs(out xDir, out yDir, angle, 1);
        }
        internal static void GetDirs(out float xDir, out float yDir, float angle, float dist)
        {
            xDir = (float)( Math.Cos(angle) * dist );
            yDir = (float)( Math.Sin(angle) * dist );
        }
        internal static float GetRandomAngle()
        {
            return Random.DoubleFull((float)Math.PI);
        }

        internal static Image LoadImage(string name)
        {
            return LoadImage(name, Color.Magenta);
        }
        internal static Image LoadImage(string name, Color color)
        {
            Bitmap retVal = new Bitmap(PicLocation + name);
            retVal.MakeTransparent(color);
            return retVal;
        }
        internal static Image LoadImage(string name, float size)
        {
            Image image = LoadImage(name);
            return ResizeImage(image, size);
        }
        internal static Image LoadImage(string name, Color color, float size)
        {
            Image image = LoadImage(name, color);
            return ResizeImage(image, size);
        }
        internal static Image ResizeImage(Image image, float size)
        {
            return ResizeImage(image, size, true);
        }
        internal static Image ResizeImage(Image image, float size, bool disposeOriginal)
        {
            int sizeInt = Random.Round(size * 2);
            if (sizeInt < 1)
                sizeInt = 1;
            Image retVal = new Bitmap(image, new Size(sizeInt, sizeInt));
            if (disposeOriginal)
                image.Dispose();
            return retVal;
        }

        internal static void ShootAtPlayer(float speed, float x, float y, float size)
        {
            float xDir = -x, yDir = -y;
            //half the time, adjust for the player's movement
            if (Random.Bool())
                AdjustForPlayerSpeed(ref xDir, ref yDir, speed + BulletSpeed, size + BulletSize);

            float bulletAngle = GetAngle(xDir, yDir);
            //randomize the angle slightly
            bulletAngle += Random.Gaussian(AlienFiringInaccuracy);

            GetDirs(out xDir, out yDir, bulletAngle);
            Bullet.NewBullet(x, y, xDir, yDir, speed, size, Bullet.FriendlyStatus.Enemy);
        }

        internal static void AdjustForPlayerSpeed(ref float xDir, ref float yDir, float speed, float spacing)
        {
            //angle between bullet and player movement vectors
            float angle = ( GetAngle(xDir, yDir) - GetAngle(GameForm.Game.inputX, GameForm.Game.inputY) );
            //distance from player
            float distance = GetDistance(xDir, yDir) - spacing;
            float xd, yd;
            GetDirs(out xd, out yd, angle, distance);
            double xDist = xd, yDist = yd;
            //ratio of speed between the bullet and the player
            double speedRatio = speed / GameForm.Game.TotalSpeed;
            speedRatio *= speedRatio;
            //make sure a zero will not be in the denominator
            while (speedRatio == 1.0)
                speedRatio = 1.0 + Random.Gaussian(1.0 / ( 1 << 53 ));
            yDist *= yDist;
            double sqrt = ( xDist * xDist + yDist ) * speedRatio - yDist;
            //handle negative square root
            if (sqrt < 0)
                sqrt = -Math.Sqrt(-sqrt);
            else
                sqrt = Math.Sqrt(sqrt);
            //lead is the extra distance the player will travel until the bullet hits
            double lead = ( sqrt + xDist ) / ( speedRatio - 1.0 );
            //negative lead means the player is traveling away too quickly
            if (lead > 0)
            {
                //add the lead to the firing direction
                float totalInput = GetDistance(GameForm.Game.inputX, GameForm.Game.inputY);
                xDir += (float)( lead * GameForm.Game.inputX / totalInput );
                yDir += (float)( lead * GameForm.Game.inputY / totalInput );
            }
        }

        internal static void NormalizeDirs(ref float xDir, ref float yDir, float speed)
        {
            float distance = GetDistance(xDir, yDir);
            if (distance > 0)
            {
                float mult = speed / distance;
                xDir *= mult;
                yDir *= mult;
            }
            else if (speed != 0)
            {
                GetRandomDirection(out xDir, out yDir, speed);
            }
        }
        internal static float GetAngle(double xSpeed, double ySpeed)
        {
            return (float)Math.Atan2(ySpeed, xSpeed);
        }

        internal static float GetDistanceSqr(float x, float y)
        {
            return ( x * x + y * y );
        }
        internal static float GetDistanceSqr(float x1, float y1, float x2, float y2)
        {
            return GetDistanceSqr(x1 - x2, y1 - y2);
        }
        internal static float GetDistance(float x, float y)
        {
            return (float)Math.Sqrt(GetDistanceSqr(x, y));
        }
        internal static float GetDistance(float x1, float y1, float x2, float y2)
        {
            return GetDistance(x1 - x2, y1 - y2);
        }

        internal static PointF RandomEdgePoint()
        {
            return GetPoint(GetRandomAngle(), CreationDist);
        }
        private PointF RandomStartPoint(float size)
        {
            float padding = 3.9f * ( PlayerSize + AlienSize );
            PointF retVal = GetPoint(GetRandomAngle(), padding + Random.DoubleHalf(MapSize - padding));

            foreach (GameObject obj in objects)
                if (GetDistanceSqr(retVal.X, retVal.Y, obj.X, obj.Y) < ( size + obj.Size ) * ( size + obj.Size ))
                    return RandomStartPoint(size);

            return retVal;
        }
        private static PointF GetPoint(float angle, float dist)
        {
            float x, y;
            GetDirs(out x, out y, angle, dist);
            return new PointF(x, y);
        }

        //redefines the exponent operation such that it behaves for all inputs 
        //very closely to its real behavior when base is greater than 1
        internal static float VectorExponent(double expBase, double expPower)
        {
            return (float)( Math.Sign(expBase) * ( Math.Pow(Math.Abs(expBase) + 1, expPower) - 1 ) );
        }

        internal static int Round(float value)
        {
            return (int)Math.Round(value);
        }

        #endregion //internal abstraction methods

        #region game logic

        internal Game(GameTicker.EventDelegate Refresh)
            : base(GameTick, Refresh)
        {
        }

        internal void InitGame(int centerX, int centerY, bool fireworks, bool scoring)
        {
            Dispose();

            this.Scoring = scoring;
            this.fireworks = fireworks;

            this.objects = new HashSet<GameObject>();
            this.centerX = centerX;
            this.centerY = centerY;

            SetRandomInput();
            if (Fireworks)
            {
                headingAngle = GetRandomAngle();
                headingAngleDir = Random.Bool();
            }

            this.score = 0;

            this.life = StartLife * ( Fireworks ? 100 : 1 );
            this.fuel = StartFuel;
            this.ammo = StartAmmo;

            this.deadCounter = -1;
            this.fireCounter = -1;

            this.alienCount = 1;

            this.Turbo = Fireworks;
            this.Fire = Fireworks;

            CreateStartObjects();

            Running = true;
            Started = Fireworks;
        }

        //1 power up, 3 aliens, random number of asteroids
        private void CreateStartObjects()
        {
            PointF start = RandomStartPoint(PowerUpSize);
            PowerUp.NewPowerUp(start.X, start.Y);

            for (int a = 0 ; a < 3 ; a++)
            {
                start = RandomStartPoint(AlienSize);
                Alien.NewAlien(start.X, start.Y);
            }

            int startAsteroids = Random.GaussianOEInt(7.8f, .39f, .26f, 1);
            for (int b = 0 ; b < startAsteroids ; b++)
            {
                start = RandomStartPoint(AsteroidMaxSize);
                Asteroid.NewAsteroid(start.X, start.Y);
            }
        }

        internal GameObject GetPlayerObject()
        {
            float xSpeed = inputX;
            float ySpeed = inputY;
            NormalizeDirs(ref xSpeed, ref ySpeed, TotalSpeed);
            return new GameObject.DummyObject(xSpeed, ySpeed);
        }

        public override void Step()
        {
            if (Dead && ++deadCounter > DeathTime)
                deadCounter = -1;

            float playerSpeed = MovePlayer();
            float xSpeed = inputX;
            float ySpeed = inputY;
            NormalizeDirs(ref xSpeed, ref ySpeed, playerSpeed);
            PlayerFiring(playerSpeed);

            MoveAndCollide(xSpeed, ySpeed, playerSpeed);
            CreateObjects(playerSpeed);

            CheckFireworks();
        }

        private float MovePlayer()
        {
            float playerSpeed = BasePlayerSpeed;
            if (Turbo && !Dead)
            {
                float turbo = TurboSpeed;
                fuel -= turbo;
                playerSpeed += turbo;
            }

            AddScore((decimal)playerSpeed * DistanceScore);

            return playerSpeed;
        }

        private void PlayerFiring(float playerSpeed)
        {
            //check that the player is firing and can fire
            --fireCounter;
            if (Fire && !Dead && fireCounter < 0 && ammo > 0)
            {
                if (--ammo < 1)
                    fireCounter = -1;
                else
                    fireCounter = Random.Round(GetCoolDown());
                Bullet.NewBullet(0, 0, inputX, inputY, playerSpeed, PlayerSize, Bullet.FriendlyStatus.Friend);
            }
        }

        private float GetCoolDown()
        {
            return (float)( FireTimeMult / Math.Pow(ammo + FireTimeAmmoAdd, FireTimePower) );
        }

        private void MoveAndCollide(float xSpeed, float ySpeed, float playerSpeed)
        {
            var objectSectors = MoveObjects(xSpeed, ySpeed, playerSpeed);
            CollideObjects(objectSectors);
        }

        private Dictionary<Point, List<GameObject>> MoveObjects(float xSpeed, float ySpeed, float playerSpeed)
        {
            var objectSectors = new Dictionary<Point, List<GameObject>>();

            foreach (GameObject obj in Random.Iterate(objects))
            {
                //move the object
                float playerDamage = obj.Step(xSpeed, ySpeed, playerSpeed);
                if (playerDamage > 0)
                    HitPlayer(playerDamage);

                //make sure the object is still in the game
                if (objects.Contains(obj))
                {
                    //add for collision detection
                    Point? p = GetSector(obj);
                    if (p.HasValue)
                    {
                        Point key = p.Value;
                        List<GameObject> sector;
                        if (!objectSectors.TryGetValue(key, out  sector))
                        {
                            sector = new List<GameObject>();
                            objectSectors.Add(key, sector);
                        }
                        sector.Add(obj);
                    }
                }
            }

            return objectSectors;
        }
        private void HitPlayer(float damage)
        {
            bool alreadyLost = GameOver();

            if (damage < PlayerLife)
                damage = Random.GaussianCapped(damage, PlayerDamageRandomness);

            if (Dead)
            {
                //if the player is dead, turn lost life into lost score
                AddScore((decimal)-damage * ScoreToDamageRatio);
            }
            else
            {
                int old = Lives;
                AddLife(-damage);
                //check if the player lost a whole life
                if (Lives < old)
                {
                    //set lives to a multiple of LivesMult and adjust score accordingly
                    float newLives = ( old - 1 ) * PlayerLife;
                    AddScore(( (decimal)life - (decimal)newLives ) * ScoreToDamageRatio);
                    life = newLives;
                    //the player died
                    deadCounter = 0;
                    fireCounter = -1;
                }
            }

            if (!alreadyLost && GameOver())
            {
                AddScore((decimal)ammo * RemainingAmmoScore);
                AddScore((decimal)fuel * RemainingFuelScore);
            }
        }
        private Point? GetSector(GameObject obj)
        {
            //no collisions for objects completely outside of the creation distance
            if (GetDistance(obj.X, obj.Y) - obj.Size > CreationDist)
                return null;

            return new Point((int)( obj.X / SectorSize ) + ( obj.X > 0 ? 1 : 0 ), (int)( obj.Y / SectorSize ) + ( obj.Y > 0 ? 1 : 0 ));
        }

        private void CollideObjects(Dictionary<Point, List<GameObject>> objectSectors)
        {
            HashSet<Point> done = new HashSet<Point>();
            foreach (var pair in Random.Iterate(objectSectors))
            {
                Point point = pair.Key;
                List<GameObject> curSector = pair.Value;

                for (int a = 0 ; a < curSector.Count ; ++a)
                {
                    GameObject obj = curSector[a];
                    //make sure the object is still in the game
                    if (objects.Contains(obj))
                        CollideObject(objectSectors, obj, point, a, done);
                }

                done.Add(point);
            }
        }
        private void CollideObject(Dictionary<Point, List<GameObject>> objectSectors, GameObject obj, Point point, int objIndex, HashSet<Point> done)
        {
            //some objects need to check extra sectors away
            int checkDist = 1;
            IChecksExtraSectors checksExtra = ( obj as IChecksExtraSectors );
            if (checksExtra != null)
            {
                checkDist = checksExtra.CheckSectors;
                if (checkDist < 2)
                    checkDist = 1;
            }

            foreach (Point p2 in Random.Iterate(point.X - checkDist, point.X + checkDist, point.Y - checkDist, point.Y + checkDist))
            {
                List<GameObject> value;
                if (objectSectors.TryGetValue(p2, out value) && ( !done.Contains(p2) ||
                        ( checkDist > 1 && ( Math.Abs(point.X - p2.X) > 1 || Math.Abs(point.Y - p2.Y) > 1 ) ) ))
                {
                    int start = 0;
                    //when checking the object's own sector, we only need to check objects with a higher index
                    if (p2 == point)
                        start = objIndex + 1;

                    for (int a = start ; a < value.Count ; ++a)
                    {
                        GameObject checkObj = value[a];
#if DEBUG
                        //if either object checks extra sectors, the sector size is effectively multiplied by that amount
                        int sectMult = checkDist;
                        IChecksExtraSectors checksExtra2;
                        if (( checksExtra2 = checkObj as IChecksExtraSectors ) != null)
                            sectMult = Math.Max(sectMult, checksExtra2.CheckSectors);
                        //this collision detection algorithm only works properly
                        //if the sum of the sizes of any two objects is less than the sector size
                        if (obj.Size + checkObj.Size > SectorSize * sectMult
                            //this case is only valid because fuel explosions do not collide with one another
                            && !( obj is FuelExplosion && checkObj is FuelExplosion ))
                            throw new Exception(string.Format("Sector size ({4}) is too small for:  {0} ({1}) and {2} ({3})",
                                    obj.GetType(), obj.Size, checkObj.GetType(), checkObj.Size, SectorSize));
#endif
                        //make sure the second object is still in the game
                        if (objects.Contains(checkObj))
                        {
                            obj.CheckCollision(checkObj);
                            //if the collision killed the main object, return early
                            if (!objects.Contains(obj))
                                return;
                        }
                    }
                }
            }
        }

        private void CreateObjects(float playerSpeed)
        {
            if (Random.Bool(playerSpeed * AlienCreationRate))
                Alien.NewAlien();
            if (Random.Bool(playerSpeed * AlienShipCreationRate / alienCount))
                AlienShip.NewAlienShip();
            if (Random.Bool(playerSpeed * AsteroidCreationRate))
                Asteroid.NewAsteroid();
            if (Random.Bool(playerSpeed * LifeDustCreationRate))
                LifeDust.NewLifeDust();
            if (Random.Bool(playerSpeed * PowerUpCreationRate))
                PowerUp.NewPowerUp();
        }

        private void CheckFireworks()
        {
            if (Fireworks)
            {
                if (Random.Bool(TotalSpeed * .039))
                    PowerUp.NewFirework();

                if (Random.Bool(GameSpeed * .039))
                    headingAngleDir = !headingAngleDir;
                headingAngle += Random.OE(GameSpeed * .039f) * ( headingAngleDir ? -1f : 1f );
                PointF p = GetPoint(headingAngle, MapSize * .39f);
                SetMouseCoordinates(Random.Round(p.X), Random.Round(p.Y));
            }
        }

        #endregion //game logic

        #region IChecksExtraSectors

        internal interface IChecksExtraSectors
        {
            int CheckSectors
            {
                get;
            }
        }

        #endregion //IChecksExtraSectors
    }
}
