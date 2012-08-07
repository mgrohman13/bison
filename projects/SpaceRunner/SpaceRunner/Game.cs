using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MattUtil;
using Point = MattUtil.Point;

namespace SpaceRunner
{
    public class Game : MattUtil.RealTimeGame.Game
    {
        public Game(MattUtil.RealTimeGame.GameTicker.EventDelegate Refresh)
            : base(GameTick, Framerate, Refresh)
        {
        }

        public void InitGame(int centerX, int centerY, bool fireworks, bool scoring)
        {
            this.Scoring = scoring;
            this.fireworks = fireworks;

            //reset game values
            RandomInput();
            life = StartLife * ( Fireworks ? 100 : 1 );
            ammo = StartAmmo;
            fuel = StartFuel;
            score = 0m;
            Turbo = Fireworks;
            Fire = Fireworks;
            deadCounter = -1;
            fireCounter = -1;

            alienCount = 1;
            Running = true;

            this.centerX = centerX;
            this.centerY = centerY;
            drawX = Random.Round(centerX - playerImage.Width / 2f);
            drawY = Random.Round(centerY - playerImage.Height / 2f);

            //clean up old objects
            DisposeObjects();

            //create starting objects
            StartObjects();

            if (Fireworks)
                headingAngle = Random.DoubleFull((float)Math.PI);

            //start game
            Started = Fireworks;
        }

        #region Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Forms.GameForm mainForm = new Forms.GameForm();

            Alien.NewAlien();
            AlienShip.NewAlienShip();
            Asteroid.NewAsteroid();
            PointF p = RandomEdgePoint();
            Bullet.NewBullet(p.X, p.Y, 0, 0, 0, 0, Bullet.FriendlyStatus.Enemy);
            p = RandomEdgePoint();
            Explosion.NewExplosion(p.X, p.Y, 0);
            p = RandomEdgePoint();
            FuelExplosion.NewFuelExplosion(p.X, p.Y);
            LifeDust.NewLifeDust();
            PowerUp.NewPowerUp();

#if TRACE
            MattUtil.RealTimeGame.ScoresForm.Scoring = false;
#endif

            Application.Run(mainForm);

            Forms.GameForm.Game.Dispose();
            mainForm.Dispose();
        }
        void Dispose()
        {
            playerImage.Dispose();
            noAmmoImage.Dispose();
            pausedFont.Dispose();

            DisposeObjects();

            //dispose   images
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
        public const int GameTick = 6;
        //minimum miliseconds between graphics refresh
        public const int Framerate = 21;

        //directory info
        const string PicLocation = "..\\..\\..\\pics\\";
        public override string ScoreFile
        {
            get
            {
                return PicLocation + "pics.dat";
            }
        }

        //image information, should reflect actual images
        public const int NumExplosions = 5;
        public const int NumExplosionImages = 15;
        public const int NumAsteroidImages = 8;
        public const float AsteroidMaxImageSizeHalf = 27f;
        public const int NumFuelExplosionImages = 6;
        public const float FuelExplosionImageSizeHalf = 65f;
        public const int NumLifeDustImages = 6;
        public const int NumFireworks = 13;

        //mathematical values
        public const float HalfPi = (float)( Math.PI / 2.0 );
        public const float TwoPi = (float)( Math.PI * 2.0 );
        public const float DegToRad = (float)( Math.PI / 180.0 );

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

        public const float MapSize = 338f;
        //distance from the edge of the map that objects are removed from the game
        public const float RemovalDist = MapSize * 1.69f;
        //distance from the edge of the map at which new objects are created
        public const float CreationDist = MapSize + 39f;
#if TRACE
        public const float GameSpeed = GameTick * 0.021f;
#else
        public const float GameSpeed = (float)( GameTick * Math.PI * .0139 );
#endif
        //sectors for collision detection
        public const float SectorSize = ( AsteroidMaxSize + FuelExplosionSize ) / 2f;

        public const float PlayerSize = 17f;
        public const float BasePlayerSpeed = GameSpeed * 3f;
        //time spent dead before getting the next life
        public const float DeathTime = 1f / GameSpeed * 65f;
        //time spent reloading (will be divided by ammo^FireTimePower)
        public const double FireTimeMult = 1.0 / GameSpeed * 5200.0;
        public const double FireTimePower = 1.69;
        //how many extra pixels each fuel point will take you
        public const float FuelMult = 169f;
        //percentage of fuel consumed each iteration when using turbo
        public const double FuelRate = GameSpeed * .052;
        //exponent of fuel consumption
        public const double FuelPower = .52;

        public const float StartFuel = FuelMult * 10f;
        //average fuel per power up
        public const float IncFuel = FuelMult * 3f;
        public const float IncFuelRandomness = .104f;

#if TRACE
        public const float StartLife = PlayerLife * 30f;
#else
        public const float StartLife = PlayerLife * 3f;
#endif
        public const float PlayerLife = 13f;
        public const float PlayerDamageRandomness = .065f;

        public const int StartAmmo = 10;
        public const int IncAmmo = 3;

        //chances of objects being created each iteration (will be multiplied by players current speed)
        public const float LifeDustCreationRate = (float)( Math.E * .0013 );
        public const float PowerUpCreationRate = (float)( Math.E / 1300 );
        public const float AsteroidCreationRate = .078f;
        public const float AlienCreationRate = .013f;
        //will be divided by number of alien ships
        public const float AlienShipCreationRate = .00013f;

        public const float AlienSize = 13f;
        public const float AlienSpeed = GameSpeed * 2.6f;
        public const float AlienSpeedRandomness = .39f;
        //fuel power up
        public const float AlienSpeedInc = GameSpeed * 3f;
        //only when an alien picks up a first ammo power up
        public const float AlienFireRate = (float)( GameSpeed * Math.PI / 130 );
        //subsequent ammo power ups
        public const float AlienFireRateInc = GameSpeed * .0117f;
        //randomness for power up values
        public const float AlienIncRandomness = .13f;
        //lower cap as a percentage of the average value
        public const float AlienIncCap = .39f;
        public const float AlienConstSpeedReduceWeight = .65f;

        public const float ExplosionSize = Game.AlienSize * 1.21f;

        public const float AlienShipSize = 21f;
        public const float AlienShipLife = 169f;
        public const float AlienShipLifeInc = AlienShipLife;
        public const float AlienShipFireRate = GameSpeed * .0169f;
        public const float AlienShipFireRateInc = GameSpeed * .0078f;
        public const float AlienShipSpeedMult = 1.69f;
        public const float AlienShipSpeedMultInc = .39f;
        public const float AlienShipStatRandomness = .169f;
        //lower cap as a percentage of the average value
        public const float AlienShipStatCap = .13f;
        public const float AlienShipFriendlyBulletDamageMult = 9.1f;
        public const float AlienShipNeutralBulletDamageMult = 5.2f;
        public const float AlienShipFuelExplosionDamageMult = 7.8f;
        public const float AlienShipDamageRandomness = .078f;
        public const float AlienShipDamageOEPct = .26f;
        //average number of bullets in the explosion on death
        public const float AlienShipExplosionBullets = 6.5f;

        public const float AsteroidAverageSize = 16.9f;
        public const float AsteroidSizeRandomness = .091f;
        public const float AsteroidMaxSize = Game.AsteroidAverageSize * ( 2f - Game.AsteroidSizeCap );
        //lower cap as a percentage of the average value
        public const float AsteroidSizeCap = .52f;
        //damage to player and alien ship
        public const float AsteroidAreaToDamageRatio = (float)( Math.PI * PlayerSize * PlayerSize / PlayerLife );
        //alien life is actually its speed in pixels, so this damage is in pixels
        public const float AsteroidAreaToAlienDamageRatio = 1f / GameSpeed * 169f;
        //drift speed for new asteroids
        public const float AsteroidInitialSpeed = GameSpeed * .13f;
        //when an asteroid breaks, the exponent for the chance (based on the breaking astroid size) of each new smaller piece to be created
        public const float AsteroidPieceChancePower = .6f;
        //average number of smaller asteroids created when a larger one breaks
        public const float AsteroidPieces = 2.6f;
        public const float AsteroidPiecesRandomness = .117f;
        //speed of new smaller asteroids when a larger one breaks
        public const float AsteroidPieceSpeed = GameSpeed * 1.69f;
        public const float AsteroidPieceSpeedRandomness = .21f;
        //chance (based on the difference in asteroid area) for the asteroids to both break
        public const float AsteroidCollisionChance = 520f;
        //asteroids smaller than this area are frequently destroyed uneventfully when colliding with other asteroids
        public const float AsteroidCollisionCriticalArea = 117f;
        public const float AsteroidRotateConst = Game.GameSpeed * .39f;
        public const float AsteroidRotateMult = Game.GameSpeed / Game.AsteroidPieceSpeed * .65f;

        public const float BulletSize = 2.5f;
        //speed added to the speed of the object firing the bullet
        public const float BulletSpeed = (float)( GameSpeed * Math.PI );
        //damage to player and alien ship (bullets always kill aliens and asteroids)
        public const float BulletDamage = 3.9f;
        //average speed of bullets from bullet explosions
        public const float BulletExplosionSpeed = BulletSpeed * 1.3f;
        public const float BulletExplosionSpeedRandomness = .52f;
        //randomness on the angle (in degrees) when an alien shoots at the player
        public const float BulletRandomnessForAliens = 3f;

        public const float FuelExplosionSize = 91f;
        //number of iterations a fuel explosion lasts
        public const float FuelExplosionSteps = 1f / GameSpeed * 65f;
        //an object at the center of the explosion is cosidered to be this distance from it for damage purposes
        public const float FuelExplosionDamageStartDist = FuelExplosionSize * .3f;
        //damage done each iteration to the player or an alien ship inside the explosion
        public const float FuelExplosionDamage = 1f / FuelExplosionSteps * 30f;

        public const float LifeDustSize = 1.75f;
        //average amount in new clumps
        public const float LifeDustClumpAmt = 13f;
        public const float LifeDustClumpOEPct = .13f;
        public const float LifeDustAmtRandomness = .3f;
        //initial spacing between objects in a clump
        public const float LifeDustSpacing = LifeDustSize * 2;
        //speed of the entire clump
        public const float LifeDustClumpSpeed = GameSpeed * .3f;
        //speed of each individual
        public const float LifeDustIndividualSpeed = GameSpeed * .013f;
        //speed change when hit by something
        public const float LifeDustAdjustSpeed = GameSpeed * 0.0052f;
        //chance of life dust getting hit by a bullet or fuel explosion
        public const float LifeDustHitChance = GameSpeed * 0.039f;
        //how many particles needed to fully heal, also the amount in a clump created when a life power up explodes
        public const float LifeDustAmtToHeal = 52f;

        public const float PowerUpSize = 9f;
        //these three chance value are only relative to one another
        public const int PowerUpAmmoChance = 5;
        public const int PowerUpFuelChance = 6;
        public const int PowerUpLifeChance = 2;
        //average number of bullets in the explosion when an ammo power up explodes
        public const float PowerUpAmmoExplosionBullets = 13f;
        public const float PowerUpRotate = Game.GameSpeed * .78f;

        //amount each damage point while dead reduces your score
        public const decimal ScoreToDamageRatio = 1m / (decimal)PlayerLife * 10m;
        //score added per pixel traveled
        public const decimal SpeedScoreMult = .001m;
        //score an alien is worth for killing based on its speed
        public const decimal AlienSpeedScoreMult = 1m / (decimal)AlienSpeed * 1m;
        //extra score an alien that shoots is worth for killing
        public const decimal AlienFireRateScoreMult = 1m / (decimal)AlienFireRate * 1m;
        //score an alien ship is worth for injuring based on its stats compared to average
        public const decimal AlienShipScoreMult = 100m;
        //score an alien ship is worth for killing based on its stats compared to average
        public const decimal AlienShipDeathScoreMult = 10m;

        #endregion //game params
        #region fields

        Font pausedFont = new Font("Arial", 12.75f, FontStyle.Bold);

        bool fireworks, headingAngleDir;
        float headingAngle;

        readonly Image playerImage = LoadImage("player.bmp", PlayerSize),
          noAmmoImage = LoadImage("noammo.bmp", PlayerSize);
        HashSet<GameObject> objects = new HashSet<GameObject>();
        float alienCount;

        int centerX, centerY, drawX, drawY;
        int inputX, inputY;

        decimal score;
        float life;
        int ammo;
        float fuel;
        int deadCounter;
        int fireCounter;

        public bool Turbo;
        public bool Fire;

        #endregion //fields
        #region properties

        public bool Fireworks
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

        public int Lives
        {
            get
            {
                return (int)Math.Ceiling(life / PlayerLife);
            }
        }
        public float CurrentLifePart
        {
            get
            {
                float retVal;
                return ( retVal = life % PlayerLife ) > 0 ? retVal : PlayerLife;
            }
        }
        public int Ammo
        {
            get
            {
                return ammo;
            }
        }
        public float Fuel
        {
            get
            {
                return fuel / FuelMult;
            }
        }

        public bool Dead
        {
            get
            {
                return ( deadCounter > -1 );
            }
        }

        public override bool GameOver()
        {
            return life <= 0;
        }

        public float TurboSpeed
        {
            get
            {
                return (float)Math.Min(Math.Pow(fuel, FuelPower) * FuelRate, fuel);
            }
        }
        public float TotalSpeed
        {
            get
            {
                return BasePlayerSpeed + ( Turbo ? TurboSpeed : 0 );
            }
        }

        #endregion //properties
        #region public functional methods

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
                DrawHealthBar(graphics, Dead ? Brushes.Lime : Brushes.White, centerX, centerY, PlayerSize,
                        Dead ? deadCounter / DeathTime : CurrentLifePart / PlayerLife);
                DrawFireBar(graphics);
            }
            DrawObjects(graphics);
            if (Paused && !GameOver())
            {
                graphics.ResetTransform();
                const string PausedText = "PAUSED";
                float sect = centerX - graphics.MeasureString(PausedText, pausedFont).Width / 2f;
                graphics.DrawString(PausedText, pausedFont, Brushes.White, sect, centerY + PlayerSize);
            }
        }

        void DrawObjects(Graphics graphics)
        {
            if (GameOver() || !Paused)
            {
                GameObject[] array;
                int count;
                lock (gameTicker)
                {
                    count = objects.Count;
                    array = new GameObject[count];
                    objects.CopyTo(array, 0);
                }
                Array.Sort<GameObject>(array, delegate(GameObject p1, GameObject p2)
                {
                    //draw explosions, then fuel explosions, then bullets, on top of everything else
                    int retVal = 0;
                    if (p1 is Bullet)
                        ++retVal;
                    else if (p1 is Explosion)
                        retVal += 3;
                    else if (p1 is FuelExplosion)
                        retVal += 2;
                    if (p2 is Bullet)
                        --retVal;
                    else if (p2 is Explosion)
                        retVal -= 3;
                    else if (p2 is FuelExplosion)
                        retVal -= 2;
                    return retVal;
                });
                //draw objects
                for (int i = 0 ; i < count ; ++i)
                {
                    GameObject obj = array[i];
                    lock (gameTicker)
                    {
                        if (objects.Contains(obj))
                            obj.Draw(graphics, centerX, centerY);
                    }
                }
            }
        }

        void DrawPlayer(Graphics graphics)
        {
            const float DeadBlinkDiv = 300f / GameTick;
            const float DeadBlinkWindow = DeadBlinkDiv / 2.1f;
            //not drawing when deadCounter is within a certain range causes the player to blink when dead
            if (!Dead || deadCounter % DeadBlinkDiv > DeadBlinkWindow || GameOver() || Paused || !Started)
            {
                //draw player
                Rotate(graphics, inputX, inputY, centerX, centerY);
                //choose between regular image and no ammo image
                graphics.DrawImageUnscaled(( ( ( ammo > 0 && fireCounter < 0 ) || GameOver() || Fireworks ) ? playerImage : noAmmoImage ), drawX, drawY);
            }
        }

        public void DrawFireBar(Graphics graphics)
        {
            Rectangle bounds = GetBarRect(centerX, centerY, PlayerSize);
            bounds.Height += 2;
            bounds.Y += bounds.Height;
            if (fireCounter > -1 || ammo < 1)
                DrawBar(graphics, Pens.White, Brushes.Cyan, Brushes.Black, bounds,
                        ammo > 0 ? (float)( Math.Max(fireCounter, 0) * Math.Pow(ammo, FireTimePower) / FireTimeMult ) : 1);
        }

        public static void DrawHealthBar(Graphics graphics, GameObject obj, float pct)
        {
            DrawHealthBar(graphics, Brushes.White, Forms.GameForm.Game.centerX + obj.X, Forms.GameForm.Game.centerY + obj.Y, obj.Size, pct);
        }
        public static void DrawHealthBar(Graphics graphics, Brush brush, float x, float y, float size, float pct)
        {
            DrawBar(graphics, Pens.White, brush, Brushes.Black, GetBarRect(x, y, size), pct);
        }
        public static void DrawBar(Graphics graphics, Pen Border, Brush full, Brush empty, Rectangle rect, float pct)
        {
            graphics.ResetTransform();

            //draw frame
            graphics.DrawRectangle(Border, rect.X, rect.Y, rect.Width, rect.Height);
            //black out inside
            graphics.FillRectangle(empty, rect.X + 1, rect.Y + 1, rect.Width - 1, rect.Height - 1);
            //fill percent
            graphics.FillRectangle(full, rect.X + 1, rect.Y + 1, (int)( .5 + pct * rect.Width ) - 1, rect.Height - 1);
        }

        static Rectangle GetBarRect(float x, float y, float size)
        {
            return new Rectangle(Round(x - size), Round(y + size), (int)( .5 + size * 2f ), 3);
        }

        public void AddObject(GameObject obj)
        {
            objects.Add(obj);
            if (obj is AlienShip)
                ++alienCount;
        }
        public void RemoveObject(GameObject obj)
        {
            objects.Remove(obj);
            IDisposable disposable;
            if (( disposable = obj as IDisposable ) != null)
                disposable.Dispose();
            if (obj is AlienShip)
                --alienCount;
        }

        public void SetMouseCoordinates(int x, int y)
        {
            const float MapSizeSqr = MapSize * MapSize;
            if (!GameOver() && Running && !( GetDistanceSqr(x, y) > MapSizeSqr ))
            {
                //start the game if it hasnt already
                if (!Started)
                    Start();

                //unpause
                if (Paused)
                {
                    Paused = false;
                    //this fixes an exploit where you could use pause to instantly turn and fire any direction you wanted
                    //setting the input variables has to be done after this for the fix to work
                    lock (gameTicker)
                    {
                    }
                    System.Threading.Thread.Sleep(0);
                    lock (gameTicker)
                    {
                    }
                }

                if (x == 0 && y == 0)
                {
                    //if you put the mouse right on the center, move a random direction
                    RandomInput();
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

        public void AddScore(decimal amt)
        {
            score += amt;
        }
        public void AddAmmo()
        {
            ammo += IncAmmo;
            fireCounter = -1;
        }
        private void AddLife(float amt)
        {
            life += amt;
        }
        public void AddLife(float amt, bool allowNew)
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
        public void AddFuel()
        {
            fuel += Random.GaussianCapped(IncFuel, IncFuelRandomness);
        }

        #endregion //public functional methods
        #region public abstraction methods

        void DisposeObjects()
        {
            IDisposable disposable;
            foreach (GameObject obj in objects)
                if (( disposable = obj as IDisposable ) != null)
                    disposable.Dispose();
            objects.Clear();
        }
        void RandomInput()
        {
            float fx, fy;
            GetRandomDirection(out fx, out fy, MapSize);
            inputX = Random.Round(fx);
            inputY = Random.Round(fy);
        }

        public static void GetRandomDirection(out float xDir, out float yDir, float dist)
        {
            float angle = Random.DoubleFull((float)Math.PI);
            xDir = (float)( Math.Cos(angle) * dist );
            yDir = (float)( Math.Sin(angle) * dist );
        }

        public static Image LoadImage(string name, float size)
        {
            return LoadImage(name, Color.Magenta, size);
        }
        public static Image LoadImage(string name, Color color, float size)
        {
            int sizeInt = Random.Round(size * 2f);
            Bitmap image = new Bitmap(PicLocation + name);
            image.MakeTransparent(color);
            //resize image
            Image retVal = new Bitmap(image, new Size(sizeInt, sizeInt));
            image.Dispose();
            return retVal;
        }

        public static void Rotate(Graphics graphics, float angle, float centerX, float centerY)
        {
            graphics.ResetTransform();
            graphics.TranslateTransform(centerX, centerY);
            graphics.RotateTransform(angle);
            graphics.TranslateTransform(-centerX, -centerY);
        }
        public static void Rotate(Graphics graphics, float xSpeed, float ySpeed, float centerX, float centerY)
        {
            Rotate(graphics, GetAngle(xSpeed, ySpeed) + 90f, centerX, centerY);
        }

        public static void ShootAtPlayer(float speed, float x, float y, float size)
        {
            float dirX = -x, dirY = -y;

            //half the time, adjust for the player's movement
            if (Random.Bool())
                AdjustForPlayerSpeed(ref dirX, ref dirY, speed, x, y, size);

            //randomize the angle slightly
            float bulletAngle = GetAngle(dirX, dirY) + Random.Gaussian(BulletRandomnessForAliens);
            bulletAngle *= DegToRad;
            dirX = (float)Math.Cos(bulletAngle);
            dirY = (float)Math.Sin(bulletAngle);

            Bullet.NewBullet(x, y, dirX, dirY, speed, size, Bullet.FriendlyStatus.Enemy);
        }

        public static void AdjustForPlayerSpeed(ref float dirX, ref float dirY, float speed, float x, float y, float spacing)
        {
            //angle between enemy and player movement vectors
            float angle = ( GetAngle(dirX, dirY) - GetAngle(Forms.GameForm.Game.inputX, Forms.GameForm.Game.inputY) ) * DegToRad;
            //distance from player
            float dist = GetDistance(x, y) - spacing - PlayerSize;
            float xDist = (float)( Math.Cos(angle) * dist );
            float yDist = (float)( Math.Sin(angle) * dist );
            //ratio of speed between the bullet and the player
            float speedRatio = ( speed + BulletSpeed ) / Forms.GameForm.Game.TotalSpeed;
            //check that a zero will not be in the denominator
            if (( speedRatio *= speedRatio ) != 1)
            {
                //all occurances of yDist in the formula are squared, so do it now
                yDist *= yDist;
                //check that the value inside the square root is positive
                float sqrtInner = ( xDist * xDist + yDist ) * speedRatio - yDist;
                if (sqrtInner >= 0)
                {
                    //lead is the extra distance the player will travel until the bullet hits
                    float lead = (float)( ( Math.Sqrt(sqrtInner) + xDist ) / ( speedRatio - 1.0 ) );
                    //add the buffer to the direction
                    float totalInput = GetDistance(Forms.GameForm.Game.inputX, Forms.GameForm.Game.inputY);
                    dirX += lead * Forms.GameForm.Game.inputX / totalInput;
                    dirY += lead * Forms.GameForm.Game.inputY / totalInput;
                }
            }
        }

        public static void NormalizeDirs(ref float xDir, ref float yDir, float speed)
        {
            float distance;
            if (( distance = GetDistance(xDir, yDir) ) > 0)
            {
                float mult;
                xDir *= ( mult = speed / distance );
                yDir *= mult;
            }
#if DEBUG
            else if (speed > 0)
                throw new Exception();
#endif
            else
            {
                xDir = 0;
                yDir = 0;
            }
        }
        public static float GetAngle(float xSpeed, float ySpeed)
        {
            float arctan;
            if (xSpeed == 0)
                arctan = HalfPi;
            else
                arctan = (float)Math.Atan(ySpeed / xSpeed);

            return arctan / DegToRad + ( xSpeed == 0 ? ( ySpeed < 0 ? 180 : 0 ) : ( xSpeed < 0 ? 180 : 0 ) );
        }

        public static float GetDistanceSqr(float x, float y)
        {
            return ( x * x + y * y );
        }
        public static float GetDistanceSqr(float x1, float y1, float x2, float y2)
        {
            return GetDistanceSqr(x1 - x2, y1 - y2);
        }
        public static float GetDistance(float x, float y)
        {
            return (float)Math.Sqrt(GetDistanceSqr(x, y));
        }
        public static float GetDistance(float x1, float y1, float x2, float y2)
        {
            return GetDistance(x1 - x2, y1 - y2);
        }

        public static PointF RandomEdgePoint()
        {
            float angle;
            return new PointF((float)( Math.Cos(angle = Random.DoubleFull((float)Math.PI)) * CreationDist ),
                (float)( Math.Sin(angle) * CreationDist ));
        }

        public PointF RandomStartPoint(float size, bool fullMap)
        {
            float angle = Random.DoubleFull((float)Math.PI);
            float maxDist;
            if (fullMap)
                maxDist = RemovalDist;
            else
                maxDist = MapSize;
            float padding = 3f * ( PlayerSize + size );
            maxDist -= padding + size;
            float dist = padding + Random.DoubleHalf(maxDist);
            PointF retVal = new PointF((float)( Math.Cos(angle) * dist ), (float)( Math.Sin(angle) * dist ));
            bool valid = true;
            foreach (GameObject obj in objects)
                if (GetDistanceSqr(retVal.X, retVal.Y, obj.X, obj.Y) < ( size + obj.Size ) * ( size + obj.Size ))
                {
                    valid = false;
                    break;
                }
            if (valid)
                return retVal;
            return RandomStartPoint(size, fullMap);
        }

        public static int Round(float value)
        {
            return (int)Math.Round(value);
        }

        #endregion //public abstraction methods
        #region game logic

        void StartObjects()
        {
            //1 power up, 3 aliens, random number of asteroids

            PointF start = RandomStartPoint(PowerUpSize, false);
            PowerUp.NewPowerUp(start.X, start.Y);

            for (int i = 0 ; i < 3 ; i++)
            {
                start = RandomStartPoint(AlienSize, false);
                Alien.NewAlien(start.X, start.Y);
            }

            int startAsteroids = Game.Random.GaussianOEInt(13f, .39f, .26f, 1);
            for (int i = 0 ; i < startAsteroids ; i++)
            {
                start = RandomStartPoint(AsteroidMaxSize, true);
                Asteroid.NewAsteroid(start.X, start.Y);
            }
        }


        void CheckFireworks()
        {
            if (Fireworks)
            {
                if (Random.Bool(TotalSpeed * .039))
                    PowerUp.NewFirework();

                if (Random.Bool(GameSpeed * .039))
                    headingAngleDir = !headingAngleDir;
                headingAngle += Random.OE(GameSpeed * .039f) * ( headingAngleDir ? -1f : 1f );
                PointF p = new PointF((float)( Math.Cos(headingAngle) * MapSize * .39 ), (float)( Math.Sin(headingAngle) * MapSize * .39 ));
                SetMouseCoordinates(Random.Round(p.X), Random.Round(p.Y));
            }
        }

        public override void Step()
        {
            if (Dead && ++deadCounter > DeathTime)
                deadCounter = -1;

            float speed = MovePlayer();
            //direction
            float xSpeed = inputX;
            float ySpeed = inputY;
            //normalize for speed
            NormalizeDirs(ref xSpeed, ref ySpeed, speed);

            PlayerFiring(speed);
            //RegenLife();

            //create new objects
            CreateObjects(speed);

            //move objects and detect collisions
            MoveObjects(xSpeed, ySpeed);

            CheckFireworks();
        }

        //  void RegenLife()
        //{
        //    //regen life
        //    float maxLifeRegen = PlayerLife - CurrentLifePart;
        //    if (PlayerLifeRegen > maxLifeRegen)
        //    {
        //        life += maxLifeRegen;
        //        float extra = PlayerLifeRegen - maxLifeRegen;
        //        //add to score when not regenerating life
        //        AddScore((decimal)extra * ScoreToDamageRatio);
        //    }
        //    else
        //    {
        //        life += PlayerLifeRegen;
        //    }
        //}

        float MovePlayer()
        {
            //check turbo
            float speed = BasePlayerSpeed;
            if (Turbo && !Dead)
            {
                float turbo = TurboSpeed;
                fuel -= turbo;
                speed += turbo;
            }

            //add traveling score
            AddScore((decimal)speed * Game.SpeedScoreMult);
            return speed;
        }

        void PlayerFiring(float speed)
        {
            //check that the player is firing and can fire
            --fireCounter;
            if (Fire && !Dead && fireCounter < 0 && ammo > 0)
            {
                if (--ammo < 1)
                    fireCounter = -1;
                else
                    fireCounter = Random.Round(FireTimeMult / Math.Pow(ammo, FireTimePower));
                Bullet.NewBullet(0, 0, inputX, inputY, speed, PlayerSize, Bullet.FriendlyStatus.Friend);
            }
        }

        void CreateObjects(float appearChance)
        {
            //if (Fireworks)
            //    appearChance /= 3f;

            //in case appearChance > 1 we dont want to overflow Random.Bool
            for ( ; appearChance > 1 ; --appearChance)
                ActualCreate(1);
            ActualCreate(appearChance);
        }
        void ActualCreate(float appearChance)
        {
            //this relies on each objects creation rate being less than 1
            if (Random.Bool(appearChance * LifeDustCreationRate))
                LifeDust.NewLifeDust();
            if (Random.Bool(appearChance * PowerUpCreationRate))
                PowerUp.NewPowerUp();
            if (Random.Bool(appearChance * AsteroidCreationRate))
                Asteroid.NewAsteroid();
            if (Random.Bool(appearChance * AlienCreationRate))
                Alien.NewAlien();
            if (Random.Bool(appearChance * AlienShipCreationRate / alienCount))
                AlienShip.NewAlienShip();
        }

        void MoveObjects(float xSpeed, float ySpeed)
        {

            //Mine.indexer = 0;

            int minX, maxX, minY, maxY;
            Dictionary<Point, List<GameObject>> objectSectors = GetObjectSectors(out minX, out maxX, out minY, out maxY);
            List<GameObject> objs;
            if (objectSectors.TryGetValue(new Point(int.MinValue, int.MinValue), out objs))
                foreach (GameObject obj in objs)
                    obj.Step(xSpeed, ySpeed);

            bool xFirst = Random.Bool(), xDir = Random.Bool(), yDir = Random.Bool();

            int x, y;
            x = xDir ? minX : maxX;
            y = yDir ? minY : maxY;

            while (xFirst ? ( xDir ? x <= maxX : x >= minX ) : ( yDir ? y <= maxY : y >= minY ))
            {
                if (xFirst)
                    y = yDir ? minY : maxY;
                else
                    x = xDir ? minX : maxX;
                while (xFirst ? ( yDir ? y <= maxY : y >= minY ) : ( xDir ? x <= maxX : x >= minX ))
                {
                    List<GameObject> curSector;
                    if (objectSectors.TryGetValue(new Point(x, y), out curSector))
                    {

                        //Mine.NewMine((x - 1) * SectorSize, (y - 1) * SectorSize, true);

                        int count = curSector.Count;
                        for (int i = 0 ; i < count ; ++i)
                        {
                            GameObject obj = curSector[i];

                            //make sure the object is still in the game
                            if (objects.Contains(obj))
                            {
                                //move the object
                                float damage = obj.Step(xSpeed, ySpeed);
                                //check for damage to the player
                                if (damage > 0)
                                    HitPlayer(damage);

                                //make sure moving the object did not kill it
                                if (objects.Contains(obj))
                                {
                                    //detect collisions with other objects
                                    CollisionDetection(objectSectors, xFirst, xDir, yDir, obj, x, y, i);
                                }
                            }
                        }
                    }
                    if (xFirst)
                        y += yDir ? 1 : -1;
                    else
                        x += xDir ? 1 : -1;
                }
                if (xFirst)
                    x += xDir ? 1 : -1;
                else
                    y += yDir ? 1 : -1;
            }
        }

        Dictionary<Point, List<GameObject>> GetObjectSectors(out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = int.MaxValue;
            maxX = int.MinValue;
            minY = int.MaxValue;
            maxY = int.MinValue;
            Dictionary<Point, List<GameObject>> objectSectors = new Dictionary<Point, List<GameObject>>();

            //List<GameObject> mines = new List<GameObject>();

            foreach (GameObject obj in Game.Random.Iterate(objects))
            {

                //if (obj is Mine)
                //{
                //    mines.Add(obj);
                //    continue;
                //}

                Point? s = GetSector(obj);
                Point p;
                if (s.HasValue)
                    p = s.Value;
                else
                    p = new Point(int.MinValue, int.MinValue);

                List<GameObject> sector;
                if (!objectSectors.TryGetValue(p, out  sector))
                {
                    sector = new List<GameObject>();
                    objectSectors.Add(p, sector);
                }
                sector.Add(obj);

                if (s.HasValue)
                {
                    minX = Math.Min(minX, p.X);
                    maxX = Math.Max(maxX, p.X);
                    minY = Math.Min(minY, p.Y);
                    maxY = Math.Max(maxY, p.Y);
                }
            }

            //foreach (GameObject mine in mines)
            //    objects.Remove(mine);

            return objectSectors;
        }
        Point? GetSector(GameObject obj)
        {
            //no collisions for objects completely outside of the creation distance
            if (GetDistance(obj.X, obj.Y) - obj.Size > CreationDist)
                return null;
            return new Point((int)( obj.X / SectorSize ) + ( obj.X > 0 ? 1 : 0 ), (int)( obj.Y / SectorSize ) + ( obj.Y > 0 ? 1 : 0 ));
            //if (p.X < -5 || p.X > 6 || p.Y < -5 || p.Y > 6)
            //{ }
            //return p;
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
                AddScore(ammo);
                AddScore((decimal)fuel / (decimal)FuelMult);
            }
        }

        void CollisionDetection(Dictionary<Point, List<GameObject>> objectSectors, bool xFirst, bool xDir, bool yDir, GameObject obj, int objX, int objY, int objIndex)
        {
            //some objects need to check extra sectors away
            int checkDist;
            IChecksExtraSectors checksExtra;
            if (( checksExtra = obj as IChecksExtraSectors ) != null)
            {
                checkDist = checksExtra.DistanceChecked;
                if (checkDist < 2)
                    checkDist = 1;
            }
            else
            {
                checkDist = 1;
            }

            int addX, addY;
            addX = xDir ? -checkDist : checkDist;
            addY = yDir ? -checkDist : checkDist;

            while (xFirst ? ( yDir ? addY <= checkDist : addY >= -checkDist ) : ( xDir ? addX <= checkDist : addX >= -checkDist ))
            {
                if (xFirst)
                    addX = xDir ? -checkDist : checkDist;
                else
                    addY = yDir ? -checkDist : checkDist;

                while (xFirst ? ( xDir ? addX <= checkDist : addX >= -checkDist ) : ( yDir ? addY <= checkDist : addY >= -checkDist ))
                {
                    //skip the four adjacent sectors that have already checked this sector
                    if (!( xFirst ? ( ( addX == ( xDir ? -1 : 1 ) && addY > -2 && addY < 2 ) || ( addX == 0 && addY == ( yDir ? -1 : 1 ) ) ) :
                        ( ( addY == ( yDir ? -1 : 1 ) && addX > -2 && addX < 2 ) || ( addY == 0 && addX == ( xDir ? -1 : 1 ) ) ) ))
                    {
                        //Mine.NewMine((objX + addX - 1) * SectorSize, (objY + addY - 1) * SectorSize, false);

                        List<GameObject> curSector;
                        if (objectSectors.TryGetValue(new Point(objX + addX, objY + addY), out curSector))
                        {
                            //when checking the objects sector, we only need to check objects with a higher index
                            int i;
                            if (addX == 0 && addY == 0)
                                i = objIndex + 1;
                            else
                                i = 0;
                            int count = curSector.Count;
                            for ( ; i < count ; ++i)
                            {
                                GameObject obj2 = curSector[i];

#if DEBUG //this code just checks if the collision detection is working properly
                                //if either object checks extra sectors, the sector size is effectively multiplied by that amount
                                int sectMult = checkDist;
                                IChecksExtraSectors checksExtra2;
                                if (( checksExtra2 = obj2 as IChecksExtraSectors ) != null)
                                    sectMult = Math.Max(sectMult, checksExtra2.DistanceChecked);
                                //this collision detection algorithm only works properly
                                //if the sum of the sizes of any two objects is less than the sector size
                                if (obj.Size + obj2.Size > SectorSize * sectMult
                                    //this case is only valid because fuel explosions do not collide with one another
                                    && !( obj is FuelExplosion && obj2 is FuelExplosion ))
                                    //SectorSize needs to be increased
                                    throw new Exception(string.Format("Sector size ({4}) needs to be increased.  {0} ({1}) and {2} ({3})",
                                        obj.GetType(), obj.Size, obj2.GetType(), obj2.Size, SectorSize));
#endif

                                //make sure the second object is still in the game
                                if (objects.Contains(obj2))
                                {
                                    obj.CheckCollision(obj2);
                                    //if the collision killed the main object, return early
                                    if (!objects.Contains(obj))
                                        return;
                                }
                            }
                        }
                    }
                    if (xFirst)
                        addX += xDir ? 1 : -1;
                    else
                        addY += yDir ? 1 : -1;
                }
                if (xFirst)
                    addY += yDir ? 1 : -1;
                else
                    addX += xDir ? 1 : -1;
            }
        }

        #endregion //game logic

        #region extra definitions
        public interface IChecksExtraSectors
        {
            int DistanceChecked
            {
                get;
            }
        }
        #endregion //extra definitions
    }
}
