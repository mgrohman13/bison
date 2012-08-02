using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MattUtil;

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
            dead = false;
            deadCounter = 0;
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
            mainForm.NewGame();

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
            Mine.Dispose();
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

        //mathematical values
        public const float HalfPi = (float)( Math.PI / 2.0 );
        public const float TwoPi = (float)( Math.PI * 2.0 );
        public const float DegToRad = (float)( Math.PI / 180.0 );

        #endregion //consts
        #region game params
        //all object sizes are actually the radius of the object

        public const int MapRadius = 338;
        //distance from the edge of the map before objects are removed from the game
        public const float ExtraMapDistance = 65f;
        //distance from the edge of the map at which new objects are created
        public const float NewObjectDist = 39f;
        public const float GameSpeed = GameTick * .045f;
        //sectors for collision detection
        public const float SectorSize = 60f;

        public const float PlayerSize = 16.9f;
        public const float BasePlayerSpeed = GameSpeed * 3f;
        //time spent dead before getting the next life
        public const float DeathTime = 1f / GameSpeed * 65f;
        //time spent reloading (will be divided by ammo)
        public const float FireTimeMult = 1f / GameSpeed * 666f;

        //how many extra pixels each fuel point will take you
        public const float FuelMult = 169f;
        //percentage of fuel consumed each iteration when using turbo
        public const float FuelRate = GameSpeed * .052f;

        public const float StartFuel = FuelMult * 10f;
        //average fuel per power up
        public const float IncFuel = FuelMult * 3f;
        public const float IncFuelRandomness = .09f;

        public const float StartLife = PlayerLife * 3f;
        public const float PlayerLife = 13f;
        ////amount of life regenerated each iteration, up to the max for the curerent life
        //public const float PlayerLifeRegen = GameSpeed * 0.0039f;
        public const float PlayerDamageRandomness = .039f;

        public const int StartAmmo = 10;
        public const int IncAmmo = 3;

        //chances of objects being created each iteration (will be multiplied by players current speed)
        public const float LifeDustCreationRate = .0026f;
        public const float PowerUpCreationRate = .0026f;
        public const float AsteroidCreationRate = .078f;
        public const float AlienCreationRate = .013f;
        //will be divided by number of alien ships
        public const float AlienShipCreationRate = .000169f;

        public const float AlienSize = 13f;
        public const float AlienSpeed = GameSpeed * 2.6f;
        public const float AlienSpeedRandomness = .39f;
        //fuel power up
        public const float AlienSpeedInc = GameSpeed * 3f;
        //only when an alien picks up a first ammo power up
        public const float AlienFireRate = GameSpeed * .0234f;
        //subsequent ammo power ups
        public const float AlienFireRateInc = GameSpeed * .0078f;
        //randomness for power up values
        public const float AlienIncRandomness = .13f;
        //lower cap as a percentage of the average value
        public const float AlienIncCap = .39f;

        public const float AlienShipSize = 21f;
        public const float AlienShipLife = 130f;
        public const float AlienShipLifeInc = 91f;
        public const float AlienShipFireRate = GameSpeed * .0169f;
        public const float AlienShipFireRateInc = GameSpeed * .0091f;
        public const float AlienShipSpeedMult = 1.69f;
        public const float AlienShipSpeedMultInc = .39f;
        public const float AlienShipStatRandomness = .169f;
        //lower cap as a percentage of the average value
        public const float AlienShipStatCap = .13f;
        public const float AlienShipFriendlyBulletDamageMult = 7.8f;
        public const float AlienShipNeutralBulletDamageMult = 6.5f;
        public const float AlienShipFuelExplosionDamageMult = 5.2f;
        public const float AlienShipDamageRandomness = .09f;
        //average number of bullets in the explosion on death
        public const float AlienShipExplosionBullets = 3.9f;

        public const float AsteroidAverageSize = 16.9f;
        public const float AsteroidSizeRandomness = .09f;
        //lower cap as a percentage of the average value
        public const float AsteroidSizeCap = .5f;
        //damage to player and alien ship
        public const float AsteroidAreaToDamageRatio = 78f;
        //alien life is actually its speed in pixels, so this damage is in pixels
        public const float AsteroidAreaToAlienDamageRatio = 1f / GameSpeed * 169f;
        //drift speed for new asteroids
        public const float AsteroidInitialSpeed = GameSpeed * .13f;
        //when an asteroid breaks, the exponent for the chance (based on the breaking astroid size) of each new smaller piece to be created
        public const float AsteroidPieceChancePower = .6f;
        //average number of smaller asteroids created when a larger one breaks
        public const float AsteroidPieces = 2.6f;
        public const float AsteroidPiecesRandomness = .13f;
        //speed of new smaller asteroids when a larger one breaks
        public const float AsteroidPieceSpeed = GameSpeed * 1.69f;
        public const float AsteroidPieceSpeedRandomness = .21f;
        //chance (based on the difference in asteroid area) for the asteroids to both break
        public const float AsteroidCollisionChance = 520f;
        //asteroids smaller than this area are frequently destroyed uneventfully when colliding with other asteroids
        public const float AsteroidCollisionCriticalArea = 117f;

        public const float BulletSize = 2.5f;
        //speed added to the speed of the object firing the bullet
        public const float BulletSpeed = GameSpeed * 3.9f;
        //damage to player and alien ship (bullets always kill aliens and asteroids)
        public const float BulletDamage = 3.9f;
        //average speed of bullets from bullet explosions
        public const float BulletExplosionSpeed = GameSpeed * 5f;
        public const float BulletExplosionSpeedRandomness = .169f;
        //lower cap as a percentage of the average value
        public const float BulletExplosionSpeedLowerCap = .39f;
        //randomness on the angle (in degrees) when an alien shoots at the player
        public const float BulletRandomnessForAliens = 2.1f;

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
        public const float LifeDustAmtRandomness = .21f;
        //lower cap as a percentage of the average value
        public const float LifeDustAmtCap = .078f;
        //initial spacing between objects in a clump
        public const float LifeDustSpacing = 3f;
        //speed of the entire clump
        public const float LifeDustClumpSpeed = GameSpeed * .3f;
        //speed of each individual
        public const float LifeDustIndividualSpeed = GameSpeed * .03f;
        //speed change when hit by something
        public const float LifeDustAdjustSpeed = GameTick * .00013f;
        //chance of life dust getting hit by a bullet or fuel explosion
        public const float LifeDustHitChance = GameTick * .003f;
        //how many particles needed to fully heal, also the amount in a clump created when a life power up explodes
        public const float LifeDustAmtToHeal = 39f;

        public const float PowerUpSize = 9f;
        //these three chance value are only relative to one another
        public const int PowerUpAmmoChance = 5;
        public const int PowerUpFuelChance = 6;
        public const int PowerUpLifeChance = 2;
        //average number of bullets in the explosion when an ammo power up explodes
        public const float PowerUpAmmoExplosionBullets = 6f;

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
        //score an asteroid is worth for killing based on its size
        public const decimal AsteroidAreaScoreMult = 0m;

        #endregion //game params
        #region fields

        Font pausedFont = new Font("Arial", 12.75f, FontStyle.Bold);

        bool fireworks, headingAngleDir;
        float headingAngle;

        readonly Image playerImage = LoadImage("player.bmp", PlayerSize),
          noAmmoImage = LoadImage("noammo.bmp", PlayerSize);
        Dictionary<GameObject, object> objects = new Dictionary<GameObject, object>();
        float alienCount;

        int centerX, centerY, drawX, drawY;
        int inputX, inputY;

        decimal score;
        float life;
        int ammo;
        float fuel;
        bool dead;
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
                return dead;
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
                return (float)Math.Min(Math.Sqrt(fuel) * FuelRate, fuel);
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
			for (int sect = -5; sect < 11; ++sect)
			{
				float x = centerX + (sect * SectorSize);
				graphics.DrawLine(Pens.White, x, centerY - MapRadius, x, centerY + MapRadius);
				float y = centerY + (sect * SectorSize);
				graphics.DrawLine(Pens.White, centerX - MapRadius, y, centerX + MapRadius, y);
			}
			graphics.DrawEllipse(Pens.White, centerX - PlayerSize, centerY - PlayerSize, PlayerSize * 2, PlayerSize * 2);
#endif

            DrawPlayer(graphics);
            if (!Paused)
            {
                DrawHealthBar(graphics, dead ? Brushes.Lime : Brushes.White, centerX, centerY, PlayerSize, dead ? deadCounter / DeathTime : CurrentLifePart / PlayerLife);
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
                    objects.Keys.CopyTo(array, 0);
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
                        if (objects.ContainsKey(obj))
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
            if (!dead || deadCounter % DeadBlinkDiv > DeadBlinkWindow || GameOver() || Paused || !Started)
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
                    ammo > 0 ? Math.Max(fireCounter, 0) * ( ammo + 1 ) / FireTimeMult : 1);
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
            objects.Add(obj, null);
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
            const int MapSizeSqr = MapRadius * MapRadius;
            if (!( GetDistanceSqr(x, y) > MapSizeSqr ) && !GameOver() && Running)
            {
                //start the game if it hasnt already
                Started = true;

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
        public void AddLife(float amt)
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
                    life += maxLifeRegen;
                    amt -= maxLifeRegen;
                    //add to score when not regenerating life
                    AddScore((decimal)amt * ScoreToDamageRatio);
                }
                else
                {
                    life += amt;
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
            foreach (GameObject obj in objects.Keys)
                if (( disposable = obj as IDisposable ) != null)
                    disposable.Dispose();
            objects.Clear();
        }
        void RandomInput()
        {
            const float dist = MapRadius / 2f;
            float angle = Random.DoubleFull((float)Math.PI);
            inputX = Random.Round(Math.Cos(angle) * dist);
            inputY = Random.Round(Math.Sin(angle) * dist);
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
            float dirX = -x, dirY = -y, spacing = size + BulletSize + speed + Forms.GameForm.Game.TotalSpeed;

            //half the time, adjust for the player's movement
            if (Random.Bool())
                AdjustForPlayerSpeed(ref dirX, ref dirY, speed, x, y, spacing);

            //randomize the angle slightly
            float bulletAngle = GetAngle(dirX, dirY) + Random.Gaussian(BulletRandomnessForAliens);
            bulletAngle *= DegToRad;
            dirX = (float)Math.Cos(bulletAngle);
            dirY = (float)Math.Sin(bulletAngle);

            Bullet.NewBullet(x, y, dirX, dirY, speed, spacing, Bullet.FriendlyStatus.Enemy);
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
                float sqrtInner;
                if (( sqrtInner = ( xDist * xDist + yDist ) * speedRatio - yDist ) >= 0)
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

        public static void NormalizeDirs(ref float xDir, ref float yDir, float Speed)
        {
            float distance;
            if (( distance = GetDistance(xDir, yDir) ) > 0)
            {
                float mult;
                xDir *= ( mult = Speed / distance );
                yDir *= mult;
            }
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
            const float CreationDist = ( MapRadius + NewObjectDist );

            float angle;
            return new PointF((float)( Math.Cos(angle = Random.DoubleFull((float)Math.PI)) * CreationDist ),
                (float)( Math.Sin(angle) * CreationDist ));
        }

        public PointF RandomStartPoint(float size, bool fullMap)
        {
            float angle = Random.DoubleFull((float)Math.PI);
            float maxDist = MapRadius - PlayerSize - size - size;
            if (fullMap)
                maxDist += ExtraMapDistance;
            float dist = PlayerSize + size + Random.DoubleHalf(maxDist);
            PointF retVal = new PointF((float)( Math.Cos(angle) * dist ), (float)( Math.Sin(angle) * dist ));
            bool valid = true;
            foreach (GameObject obj in objects.Keys)
            {
                if (GetDistanceSqr(retVal.X, retVal.Y, obj.X, obj.Y) < ( size + obj.Size ) * ( size + obj.Size ))
                {
                    valid = false;
                    break;
                }
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

            int startAsteroids = Game.Random.OEInt(6.66f);
            for (int i = 0 ; i < startAsteroids ; i++)
            {
                start = RandomStartPoint(Asteroid.AsteroidMaxSize, true);
                Asteroid.NewAsteroid(start.X, start.Y);
            }
        }


        void CheckFireworks()
        {
            if (Fireworks)
            {
                if (Random.Bool(BasePlayerSpeed * .039))
                    PowerUp.NewFirework();

                if (Random.Bool(GameSpeed * .039))
                    headingAngleDir = !headingAngleDir;
                headingAngle += Random.OE(GameSpeed * .039f) * ( headingAngleDir ? -1f : 1f );
                PointF p = new PointF((float)( Math.Cos(headingAngle) * MapRadius * .39 ), (float)( Math.Sin(headingAngle) * MapRadius * .39 ));
                SetMouseCoordinates(Random.Round(p.X), Random.Round(p.Y));
            }
        }

        public override void Step()
        {
            if (++deadCounter > DeathTime)
                dead = false;

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
            if (Turbo && !dead)
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
            if (Fire && !dead && fireCounter < 0 && ammo > 0)
            {
                fireCounter = Random.Round(FireTimeMult / ammo);
                if (--ammo < 1)
                    fireCounter = -1;
                Bullet.NewBullet(0, 0, inputX, inputY, speed, PlayerSize + BulletSize + speed, Bullet.FriendlyStatus.Friend);
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
                            if (objects.ContainsKey(obj))
                            {
                                //move the object
                                float damage = obj.Step(xSpeed, ySpeed);
                                //check for damage to the player
                                if (damage > 0)
                                    HitPlayer(damage);

                                //make sure moving the object did not kill it
                                if (objects.ContainsKey(obj))
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

            foreach (GameObject obj in objects.Keys)
            {

                //if (obj is Mine)
                //{
                //    mines.Add(obj);
                //    continue;
                //}

                List<GameObject> sector;
                Point p = GetSector(obj);
                if (!objectSectors.TryGetValue(p, out  sector))
                {
                    sector = new List<GameObject>();
                    objectSectors.Add(p, sector);
                }
                sector.Insert(Random.Next(sector.Count + 1), obj);

                minX = Math.Min(minX, p.X);
                maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
            }

            //foreach (GameObject mine in mines)
            //    objects.Remove(mine);

            return objectSectors;
        }
        Point GetSector(GameObject obj)
        {
            return new Point((int)( obj.X / SectorSize ) + ( obj.X > 0 ? 1 : 0 ), (int)( obj.Y / SectorSize ) + ( obj.Y > 0 ? 1 : 0 ));
            //if (p.X < -5 || p.X > 6 || p.Y < -5 || p.Y > 6)
            //{ }
            //return p;
        }

        private void HitPlayer(float damage)
        {
            if (damage < PlayerLife)
                damage = Random.GaussianCapped(damage, PlayerDamageRandomness);

            if (dead)
            {
                //if the player is dead, turn lost life into lost score
                AddScore((decimal)-damage * ScoreToDamageRatio);
            }
            else
            {
                int old = Lives;
                life -= damage;
                //check if the player lost a whole life
                if (Lives < old)
                {
                    //set lives to a multiple of LivesMult and adjust score accordingly
                    float newLives = ( old - 1 ) * PlayerLife;
                    AddScore((decimal)( life - newLives ) * ScoreToDamageRatio);
                    life = newLives;
                    //the player died
                    dead = true;
                    deadCounter = 0;
                }
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
                                if (objects.ContainsKey(obj2))
                                {
                                    obj.CheckCollision(obj2);
                                    //if the collision killed the main object, return early
                                    if (!objects.ContainsKey(obj))
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

        struct Point
        {
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            int x, y;
            public int X
            {
                get
                {
                    return x;
                }
                set
                {
                    x = value;
                }
            }
            public int Y
            {
                get
                {
                    return y;
                }
                set
                {
                    y = value;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is Point)
                {
                    Point p2 = (Point)obj;
                    return x == p2.x && y == p2.y;
                }
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                //the whole reason for using this struct as opposed to System.Drawing.Point
                //is to ensure no collisions on GetHashCode for valid Point values
                const int XMult = (int)( 1.3f + ( MapRadius + ExtraMapDistance ) / SectorSize ) * 2;
                return XMult * x + y;
            }
            public override string ToString()
            {
                return string.Format("{0},{1}", x, y);
            }
        }
        #endregion //extra definitions
    }
}
