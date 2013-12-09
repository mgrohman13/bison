using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using MattUtil;
using MattUtil.RealTimeGame;
using Point = MattUtil.Point;

namespace SpaceRunner
{
    internal class Game : MattUtil.RealTimeGame.Game, IDisposable
    {
        #region StaticInit

        internal static Game StaticInit()
        {
            Game game = new Game(null, Game.Random.Round(MapSize), Game.Random.Round(MapSize), false, false);
            game.Running = false;
            game.Started = false;
            game.Dispose();

            InitializeImages(game);

            return game;
        }
        //InitializeImages serves 2 purposes:
        // -call the static constructor for each object type to load all images
        // -show something interesting on the screen when the game is first launched
        private static void InitializeImages(Game game)
        {
            PointF p = game.RandomEdgePoint();
            FuelExplosion.NewFuelExplosion(game, p.X, p.Y);
            AlienShip.NewAlienShip(game);

            p = game.RandomStartPoint(0);
            LifeDust.NewLifeDust(game, p.X, p.Y, 6);
            int amt = Game.Random.GaussianOEInt(4f, 1f, .1f, 1);
            while (--amt > -1)
            {
                p = game.RandomStartPoint(AsteroidMaxSize);
                Asteroid.NewAsteroid(game, p.X, p.Y);
            }
            amt = Game.Random.GaussianCappedInt(2f, 1f);
            while (--amt > -1)
            {
                p = game.RandomStartPoint(AlienSize);
                Alien.NewAlien(game, p.X, p.Y);
            }
            p = game.RandomStartPoint(PowerUpSize);
            PowerUp.NewPowerUp(game, p.X, p.Y);

            p = game.RandomStartPoint(-BulletSize);
            Bullet.BulletExplosion(game, p.X, p.Y, 1);
            p = game.RandomStartPoint(-ExplosionSize);
            Explosion.NewExplosion(game, new GameObject.DummyObject(p.X, p.Y, 0, 0));

            amt = Game.Random.OEInt(8f);
            while (--amt > -1)
            {
                game.MoveAndCollide(0, 0);

                game.score = 0;
                game.life = StartLife;
                game.deadCounter = -1;
            }
        }
        internal static void StaticDispose()
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
        new internal const float GameTick = 1000 / 65f;

        private const string PicLocation = "..\\..\\..\\pics\\";
        public override string ScoreFile
        {
            get
            {
                return PicLocation + "pics.dat";
            }
        }

        //should reflect actual information about the images in PicLocation
        internal const int NumAsteroidImages = 8;

        //mathematical values
        internal const float QuarterPi = (float)( Math.PI / 4.0 );
        internal const float TwoPi = (float)( Math.PI * 2.0 );
        internal const float RadToDeg = (float)( 180.0 / Math.PI );
        internal static readonly float SqrtTwo = (float)( Math.Sqrt(2.0) );

        #endregion //consts

        #region game params

        // object	radius	diameter   area
        //life dust	 2.1      4.2	    13.9    (avg)
        //bullet	 3.1      6.3	    31.0
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
        internal const float RemovalDist = MapSize * MapSize * 1.3f;
        //sectors for collision detection
        internal const float SectorSize = ( AsteroidMaxSize + FuelExplosionSize ) / 2;

        internal const float GameSpeed = (float)( GameTick * Math.PI * .013 );

        internal const float PlayerSize = 17f;
        internal const float BasePlayerSpeed = GameSpeed * 3f;
        //time spent dead before getting the next life
        internal const float DeathTime = 1 / GameSpeed * 65f;
        internal const float TurnSpeed = (float)( Math.PI * .021 );

        internal const float StartLife = PlayerLife * 3f;
        internal const float PlayerLife = 13f;
        internal const float PlayerDamageRandomness = .065f;

        internal const float StartFuel = FuelMult * 15;
        //fuel per power up
        internal const float IncFuel = FuelMult * 3;
        //how many extra pixels each fuel point will take you
        internal const float FuelMult = 130f;
        //percentage of fuel consumed each iteration when using turbo
        internal static readonly float FuelRate = GameSpeed * .13f;
        //exponent of fuel consumption
        internal const float FuelPower = .39f;

        internal const int StartAmmo = 10;
        internal const int IncAmmo = 3;
        //time spent reloading, will be divided by (ammo+FireTimeAmmoAdd)^FireTimePower
        internal const float FireTimeMult = 1 / GameSpeed * 3900f;
        internal const float FireTimePower = 1.69f;
        internal const float FireTimeAmmoAdd = 1.69f;

        private const float DeadBlinkDiv = 300f / GameTick;
        private const float DeadBlinkWindow = DeadBlinkDiv / 2.1f;

        //chances of objects being created each iteration (will be multiplied by player's current speed)
        internal const float LifeDustCreationRate = (float)( Math.E * .0013 );
        internal const float PowerUpCreationRate = .0021f;
        internal const float AsteroidCreationRate = .078f;
        internal const float AlienCreationRate = .013f;
        internal static readonly float AlienShipCreationRate = (float)( Math.Pow(60.0 * 1000.0 / GameTick, -AlienShipCreationTickPower) * .00013 );//.000078 );
        internal const float AlienShipCreationTickPower = 0f;//.13f;
        internal const float AlienShipCreationCountPower = 1f;//1.69f;

        internal const float AlienSize = 13f;
        internal const float AlienSpeed = GameSpeed * 2.6f;
        internal const float AlienSpeedRandomness = .39f;
        //fuel power up/life dust
        internal const float AlienSpeedInc = GameSpeed * 3f;
        //only when an alien picks up a first ammo power up
        internal const float AlienFireRate = (float)( GameSpeed * Math.PI / 130.0 );
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
        internal const float AlienShipSpeedMultInc = .65f;
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
        internal static readonly float AsteroidAreaToDamageRatio = GetArea(PlayerSize) / PlayerLife / 1.3f;
        //alien life is actually its speed in pixels, so this damage is in pixels
        internal const float AsteroidAreaToAlienDamageRatio = 1 / GameSpeed * 169f;
        //standard deviation for new asteroid drift speed
        internal const float AsteroidInitialSpeed = GameSpeed * .13f;
        //when an asteroid breaks, the exponent for the chance (based on the breaking astroid size) of each new smaller piece to be created
        internal const float AsteroidPieceChancePower = .6f;
        //average number of smaller asteroids created when a larger one breaks
        internal const float AsteroidPieces = 2.6f;
        internal const float AsteroidPiecesRandomness = .117f;
        //speed of new smaller asteroids when a larger one breaks
        internal const float AsteroidPieceSpeed = GameSpeed * 1.69f;
        internal const float AsteroidPieceSpeedRandomness = .26f;
        //chance (based on the difference in asteroid area) for the asteroids to both break
        internal const float AsteroidCollisionChance = 520f;
        //asteroids smaller than this area are frequently destroyed uneventfully when colliding with other asteroids
        internal const float AsteroidCollisionCriticalArea = 117f;
        internal const float AsteroidRotateConst = GameSpeed * .0065f;
        internal const float AsteroidRotateMult = GameSpeed / AsteroidPieceSpeed * .0169f;

        internal const float BulletSize = (float)( Math.PI );
        //speed added to the speed of the object firing the bullet
        internal const float BulletSpeed = (float)( GameSpeed * Math.PI );
        //damage to player and alien ship (bullets always kill aliens and asteroids)
        internal const float BulletDamage = 3.9f;
        //average speed of bullets from bullet explosions
        internal const float BulletExplosionSpeed = GameSpeed * 6.5f;
        internal const float BulletExplosionSpeedRandomness = (float)( Math.E * .13 );
        //standard deviation on the angle (in radians) when an alien shoots at the player
        internal const float AlienFiringInaccuracy = .052f;
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
        internal const float ExplosionRotate = GameSpeed * .0052f;
        //speed the explosion shockwave is considered to be traveling
        internal const float ExplosionSpeed = ( FuelExplosionSize - PowerUpSize ) / FuelExplosionTime;

        internal const float ExplosionSize = AlienSize * 1.21f;
        internal const float ExplosionTime = 1 / GameSpeed * 39f;
        internal const float ExplosionAppearanceRandomness = .104f;
        internal static readonly float ExplosionSpeedMult = (float)( Math.Pow(1.0 - .052, GameSpeed) );

        internal const float LifeDustSize = 2.1f;
        internal const float LifeDustSizeRandomness = .21f;
        //average amount in new clumps
        internal const float LifeDustClumpAmt = 13f;
        internal const float LifeDustClumpOEPct = .13f;
        internal const float LifeDustAmtRandomness = .3f;
        //initial spacing between objects in a clump
        internal const float LifeDustSpacing = LifeDustSize * 1.69f;
        //speed of the entire clump
        internal const float LifeDustClumpSpeed = GameSpeed * .3f;
        //speed of each individual
        internal const float LifeDustIndividualSpeed = GameSpeed * .03f;
        //exponent to the speed picked up from collisions with other objects
        internal const float LifeDustObjSpeedPower = GameSpeed / ( GameSpeed + .169f );
        //chance of life dust getting hit by a bullet or fuel explosion
        internal const float LifeDustHitChance = GameTick * .0065f;
        //how many particles needed to fully heal, also the amount in a clump created when a life power up explodes
        internal const float LifeDustAmtToHeal = 52f;

        internal const float PowerUpSize = 9f;
        //these three chance values are only relative to one another
        internal const int PowerUpAmmoChance = 5;
        internal const int PowerUpFuelChance = 6;
        internal const int PowerUpLifeChance = 2;
        //average number of bullets in the explosion when an ammo power up explodes
        internal const float PowerUpAmmoExplosionBullets = 13f;
        internal const float PowerUpRotate = GameSpeed * .03f;

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
        internal const decimal RemainingAmmoScore = ScoreMult * .5m;
        internal const decimal RemainingFuelScore = ScoreMult / (decimal)FuelMult * .3m;

        #endregion //game params

        #region fields

        private static readonly Font Font;
        private static readonly Image PlayerImage, NoAmmoImage, TurboImage, NoAmmoTurboImage;

        static Game()
        {
            Font = new Font("Arial", 12.75f, FontStyle.Bold);
            PlayerImage = LoadImage("player.bmp", PlayerSize);
            NoAmmoImage = LoadImage("noammo.bmp", PlayerSize);
            TurboImage = LoadImage("player fuel.bmp", PlayerSize);
            NoAmmoTurboImage = LoadImage("noammo fuel.bmp", PlayerSize);
        }

        private int tickCount;
        private readonly bool isReplay;
        private readonly Replay replay;
        private MTRandom gameRand;

        private readonly HashSet<GameObject> objects = new HashSet<GameObject>();

        private readonly int centerX, centerY;
        private float moveAngle, inputAngle;
        private decimal score;
        private float life, fuel;
        private int ammo;
        private int deadCounter, fireCounter, alienCount;
        private bool turbo;
        private float? fire;

        #endregion //fields

        #region properties

        internal MTRandom GameRand
        {
            get
            {
                if (gameRand == null)
                    return Game.Random;
                return gameRand;
            }
        }

        public override decimal Score
        {
            get
            {
                return score;
            }
        }

        internal int TickCount
        {
            get
            {
                return tickCount;
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

        internal int FuelInt
        {
            get
            {
                return (int)Math.Ceiling(fuel / FuelMult);
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
                return (float)( Math.Pow(fuel, FuelPower) * FuelRate );
            }
        }
        internal float TotalSpeed
        {
            get
            {
                return BasePlayerSpeed + ( Turbo ? TurboSpeed : 0 );
            }
        }

        internal bool Turbo
        {
            get
            {
                return ( turbo && !Dead && fuel > 0 );
            }
            set
            {
                if (!IsReplay)
                    lock (gameTicker)
                        turbo = value;
            }
        }

        internal Replay Replay
        {
            get
            {
                return this.replay;
            }
        }

        internal bool IsReplay
        {
            get
            {
                return isReplay;
            }
        }

        #endregion //properties

        #region internal functional methods

        public override void Draw(System.Drawing.Graphics graphics)
        {
#if TRACE
            graphics.ResetTransform();
            graphics.DrawEllipse(Pens.White, centerX - MapSize, centerY - MapSize, MapSize * 2 - 1, MapSize * 2 - 1);
            int drawSectors = (int)Math.Ceiling(CreationDist / SectorSize);
            for (int sect = -drawSectors ; sect <= drawSectors ; ++sect)
            {
                float x = centerX + ( sect * SectorSize );
                graphics.DrawLine(Pens.White, x, centerY - CreationDist, x, centerY + CreationDist);
                float y = centerY + ( sect * SectorSize );
                graphics.DrawLine(Pens.White, centerX - CreationDist, y, centerX + CreationDist, y);
            }
            graphics.DrawEllipse(Pens.White, centerX - PlayerSize, centerY - PlayerSize, PlayerSize * 2 - 1, PlayerSize * 2 - 1);
#endif
            DrawPlayer(graphics);
            bool pauseDraw = ( Paused && !IsReplay );
            if (!pauseDraw && !GameOver())
            {
                DrawFireBar(graphics);
                DrawHealthBar(graphics);
            }

            if (!( pauseDraw && !GameOver() ))
                DrawObjects(graphics);
            if (Paused && !GameOver())
                DrawPaused(graphics);
        }

        private void DrawPlayer(Graphics graphics)
        {
            bool pauseDraw = ( Paused && !IsReplay );
            //not drawing when deadCounter is within a certain range causes the player to blink when dead
            if (pauseDraw || !Dead || GameOver() || !Started || ( deadCounter % DeadBlinkDiv > DeadBlinkWindow ))
            {
                bool turbo = ( !pauseDraw && Turbo && !GameOver() );
                bool canFire = ( pauseDraw || CanFire() || Dead || GameOver() );
                Image image = ( canFire ? ( turbo ? TurboImage : PlayerImage ) : ( turbo ? NoAmmoTurboImage : NoAmmoImage ) );
                GameObject.DrawImage(graphics, image, centerX, centerY, 0, 0, 0, PlayerSize, AdjustImageAngle(moveAngle));
            }
        }
        internal static float GetAngleImageAdjusted(float xSpeed, float ySpeed)
        {
            return AdjustImageAngle(GetAngle(xSpeed, ySpeed));
        }
        internal static float AdjustImageAngle(float angle)
        {
            return angle + QuarterPi;
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
            graphics.DrawString(PausedText, Font, Brushes.White, drawX, centerY + PlayerSize + ( IsReplay ? 9 : 0 ));
        }

        private void DrawObjects(Graphics graphics)
        {
            IEnumerable<GameObject> gameObjects;
            lock (gameTicker)
                gameObjects = this.objects.ToList();

            foreach (GameObject obj in Random.Iterate(gameObjects).OrderBy(GetDrawPriority))
                lock (gameTicker)
                    if (this.objects.Contains(obj))
                        obj.Draw(graphics, centerX, centerY);
        }

        private static int GetDrawPriority(GameObject obj)
        {
#if DEBUG
            if (Game.GetDistance(obj.X, obj.Y) - obj.Size > Game.MapSize)
                return 0;
#endif
            //z-index: higher values are drawn on top of lower values
            if (obj is Alien)
                return 4 * ushort.MaxValue;
            if (obj is AlienShip)
                return 5 * ushort.MaxValue - GetDrawPriority(obj.Y / ( MapSize + AlienShipSize ));
            if (obj is Asteroid)
                return 1 * ushort.MaxValue;
            if (obj is Bullet)
                return 6 * ushort.MaxValue;
            if (obj is Explosion)
                return 8 * ushort.MaxValue;
            if (obj is FuelExplosion)
                return 7 * ushort.MaxValue;
            if (obj is LifeDust)
                return 3 * ushort.MaxValue - GetDrawPriority(LifeDust.GetSizePct(obj) / 4);
            if (obj is PowerUp)
                return 2 * ushort.MaxValue;
#if DEBUG
            throw new Exception();
#else
            return 0;
#endif
        }
        private static int GetDrawPriority(float priority)
        {
#if DEBUG
            if (priority < -1 || priority > 1)
                throw new Exception();
#endif
            return Round(( ushort.MaxValue / 2f - 1 ) * priority);
        }

        internal void DrawHealthBar(Graphics graphics, GameObject obj, float pct)
        {
            DrawHealthBar(graphics, Pens.White, Brushes.White, this.centerX + obj.X, this.centerY + obj.Y, obj.Size, pct);
        }
        private static void DrawHealthBar(Graphics graphics, Pen border, Brush fill, float x, float y, float size, float pct)
        {
            DrawBar(graphics, border, fill, GetBarRect(x, y, size), pct);
        }
        private static void DrawBar(Graphics graphics, Pen border, Brush fill, Rectangle rect, float pct)
        {
            Rectangle fillRect = new Rectangle(rect.X + 1, rect.Y + 1, Round(pct * ( rect.Width - 1 )), rect.Height - 1);

            graphics.ResetTransform();
            graphics.DrawRectangle(border, rect);
            graphics.FillRectangle(fill, fillRect);
        }
        private static Rectangle GetBarRect(float x, float y, float size)
        {
            return new Rectangle(Round(x - size), Round(y + size), Round(size * 2 - 1), 3);
        }

        internal void AddObject(GameObject obj)
        {
            this.objects.Add(obj);

            if (obj is AlienShip)
                ++alienCount;
        }
        internal void RemoveObject(GameObject obj)
        {
            this.objects.Remove(obj);

            IDisposable disposable;
            if (( disposable = obj as IDisposable ) != null)
                disposable.Dispose();
            if (obj is AlienShip)
                --alienCount;
        }

        internal void Fire(int x, int y)
        {
            if (!IsReplay)
                lock (gameTicker)
                    if (x != 0 || y != 0 && CanFire())
                        fire = GetAngle(x, y);
        }

        internal void SetMouseCoordinates(int x, int y)
        {
            if (!IsReplay)
                lock (gameTicker)
                    if (!GameOver() && Running && !( GetDistanceSqr(x, y) > MapSize * MapSize ))
                    {
                        if (!Started)
                            Start();
                        else if (Paused)
                            Paused = false;
                        else if (x == 0 && y == 0)
                            this.inputAngle = GetImageAngle();
                        else
                            this.inputAngle = GetAngle(x, y);
                    }
                    else if (Started && !IsReplay)
                    {
                        //pause the game if the mouse is outside the playing area
                        Paused = true;
                    }
        }

        internal void SetReplaySpeed(float speedMult)
        {
            if (IsReplay)
                base.GameTick = GameTick / speedMult;
#if DEBUG
            else
                throw new Exception();
#endif
        }
        internal static Game SetReplayPosition(Game game, int position, GameTicker.EventDelegate Refresh)
        {
            if (position < game.tickCount)
            {
                game.Dispose();
                game = new Game(Refresh, game.centerX, game.centerY, game.replay);
            }
            if (position > game.tickCount)
                game.SetReplayPosition(position, Refresh);
            return game;
        }
        private void SetReplayPosition(int position, GameTicker.EventDelegate Refresh)
        {
#if DEBUG
            if (isReplay && position > tickCount && position <= replay.Length)
            {
#endif
                Paused = true;
                SleepTick();
                Refresh();
                SleepTick();
                lock (gameTicker)
                    while (tickCount < position)
                        this.Step();
                SleepTick();
                Paused = false;
#if DEBUG
            }
            else
                throw new Exception();
#endif
        }

        private static void SleepTick()
        {
            Thread.Sleep(Game.Random.Round(GameTick * 1.3f) + 91);
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
            fuel += IncFuel;
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
                    amt = GameRand.GaussianCapped(amt, PlayerDamageRandomness);
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

        internal float RandDmgToAlien(float amt)
        {
            return GameRand.GaussianOE(amt, AlienDamageRandomness, AlienDamageOEPct);
        }

        public void Dispose()
        {
            Running = false;
            SleepTick();

            GameObject[] array;
            lock (gameTicker)
            {
                array = new GameObject[this.objects.Count];
                this.objects.CopyTo(array, 0);
                this.objects.Clear();
            }
            foreach (GameObject obj in array)
            {
                IDisposable disposable = obj as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }

            SleepTick();
        }

        private void GetMoveDirs(out float moveX, out float moveY)
        {
            GetDirs(out moveX, out moveY, moveAngle, TotalSpeed);
        }

        internal static float GetRingSpacing(int numPieces, float size)
        {
            return (float)( size * ( numPieces < 3.0 ? 1.0 : 1.0 / Math.Sin(Math.PI / numPieces) ) );
        }

        internal void GetRandomDirection(out float xDir, out float yDir, float dist)
        {
            GetRandomDirection(GameRand, out xDir, out yDir, dist);
        }
        private static void GetRandomDirection(MTRandom rand, out float xDir, out float yDir, float dist)
        {
            float angle = GetRandomAngle(rand);
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
        internal float GetRandomAngle()
        {
            return GetRandomAngle(GameRand);
        }
        internal static float GetImageAngle()
        {
            return GetRandomAngle(Game.Random);
        }
        private static float GetRandomAngle(MTRandom rand)
        {
            return ( rand.NextFloat() * TwoPi );
        }

        internal static Image LoadImageRotated(Bitmap image, float size)
        {
            float twoSize = 2 * size;
            int newSize = (int)Math.Ceiling(twoSize * SqrtTwo);

            Image retVal = new Bitmap(newSize, newSize);
            Graphics graphics = Graphics.FromImage(retVal);
            Image temp = SetTransparentBackground(image);

            const int NumRotateFlipTypes = 8;
            RotateFlipType rotateFlipType = (RotateFlipType)Game.Random.Next(NumRotateFlipTypes);
#if DEBUG
            if (Enum.IsDefined(typeof(RotateFlipType), NumRotateFlipTypes) || !Enum.IsDefined(typeof(RotateFlipType), rotateFlipType))
                throw new Exception();
#endif
            temp.RotateFlip(rotateFlipType);

            float trans = newSize / 2f;
            float scale = twoSize / temp.Width;

            graphics.TranslateTransform(trans, trans);
            graphics.RotateTransform(GetImageAngle() * RadToDeg);
            graphics.TranslateTransform(-size, -size);
            graphics.ScaleTransform(scale, scale);

            graphics.DrawImage(temp, 0f, 0f);

            graphics.Dispose();
            temp.Dispose();

            return retVal;
        }
        internal static Image LoadImage(Bitmap image, float size)
        {
            return LoadImage(image, size, true);
        }
        internal static Image LoadImage(Bitmap image, float size, bool disposeOriginal)
        {
            return ResizeImage(SetTransparentBackground(image), size, disposeOriginal);
        }
        internal static Image LoadImage(string name)
        {
            return SetTransparentBackground(new Bitmap(PicLocation + name));
        }
        internal static Image SetTransparentBackground(Bitmap image)
        {
            image.MakeTransparent(Color.Magenta);
            return image;
        }
        internal static Image LoadImage(string name, float size)
        {
            return ResizeImage(LoadImage(name), size);
        }
        internal static Image ResizeImage(Image image, float size)
        {
            return ResizeImage(image, size, true);
        }
        internal static Image ResizeImage(Image image, float size, bool disposeOriginal)
        {
            int actualSize = Game.Random.Round(size * 2f);
            if (actualSize < 1)
                actualSize = 1;
            Image retVal;
            lock (image)
                retVal = new Bitmap(image, new Size(actualSize, actualSize));
            if (disposeOriginal)
                image.Dispose();
            return retVal;
        }

        internal void ShootAtPlayer(float fireRate, ref int coolDown, float speed, float x, float y, float size)
        {
            if (--coolDown < 0 && GameRand.Bool(fireRate))
            {
                coolDown = GameRand.Round(1f + 3f * BulletSize / ( speed + BulletSpeed ));

                float xDir = -x, yDir = -y;
                //half the time, adjust for the player's movement
                if (GameRand.Bool())
                    this.AdjustForPlayerSpeed(ref xDir, ref yDir, speed + BulletSpeed, size + BulletSize);

                float bulletAngle = GetAngle(xDir, yDir);
                //randomize the angle slightly
                bulletAngle += GameRand.Gaussian(AlienFiringInaccuracy);

                GetDirs(out xDir, out yDir, bulletAngle);
                Bullet.NewBullet(this, x, y, xDir, yDir, speed, size, Bullet.FriendlyStatus.Enemy);
            }
        }

        internal void AdjustForPlayerSpeed(ref float xDir, ref float yDir, float speed, float spacing)
        {
            //angle between bullet and player movement vectors
            float angle = ( GetAngle(xDir, yDir) - this.moveAngle );
            //distance from player
            float distance = GetDistance(xDir, yDir) - spacing;
            float xDist, yDist;
            GetDirs(out xDist, out yDist, angle, distance);
            //ratio of speed between the bullet and the player
            float speedRatio = speed / this.TotalSpeed;
            speedRatio *= speedRatio;
            //make sure a zero will not be in the denominator
            if (speedRatio == 1.0)
                speedRatio = AddBit(speedRatio, GameRand.Bool());
            yDist *= yDist;
            float sqrt = ( xDist * xDist + yDist ) * speedRatio - yDist;
            //handle negative square root
            if (sqrt < 0)
                sqrt = (float)( -Math.Sqrt(-sqrt) );
            else
                sqrt = (float)( Math.Sqrt(sqrt) );
            //lead is the extra distance the player will travel until the bullet hits
            float lead = ( sqrt + xDist ) / ( speedRatio - 1f );
            //negative lead means the player is traveling away too quickly
            if (lead > 0)
            {
                //add the lead to the firing direction
                float moveX, moveY;
                GetMoveDirs(out moveX, out moveY);
                NormalizeDirs(ref moveX, ref moveY, lead);
                xDir += moveX;
                yDir += moveY;
            }
        }
        internal static float AddBit(float value, bool negative = false)
        {
            float mult = 1f + 1.501f / ( 1L << ( MTRandom.FLOAT_BITS - 1 ) );
            if (negative)
                mult = 1 / mult;
            return value * mult;
        }

        internal void NormalizeDirs(ref float xDir, ref float yDir, float speed)
        {
            float distance = GetDistance(xDir, yDir);
            if (distance > 0)
            {
                float mult = speed / distance;
                xDir *= mult;
                yDir *= mult;
            }
            else
            {
                GetRandomDirection(out xDir, out yDir, speed);
            }
        }
        internal static float GetAngle(float xSpeed, float ySpeed)
        {
            return (float)( Math.Atan2(ySpeed, xSpeed) );
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
            return (float)( Math.Sqrt(GetDistanceSqr(x, y)) );
        }
        internal static float GetDistance(float x1, float y1, float x2, float y2)
        {
            return GetDistance(x1 - x2, y1 - y2);
        }

        internal PointF RandomEdgePoint()
        {
            return GetPoint(GetRandomAngle(), CreationDist);
        }
        private PointF RandomStartPoint(float size)
        {
            float padding = 3.9f * ( PlayerSize + AlienSize );
            PointF retVal = GetPoint(GetRandomAngle(), padding + GameRand.DoubleHalf(MapSize - padding));

            foreach (GameObject obj in this.objects)
                if (GetDistanceSqr(retVal.X, retVal.Y, obj.X, obj.Y) < ( size + obj.Size ) * ( size + obj.Size ))
                    return RandomStartPoint(size);

            return retVal;
        }
        internal static PointF GetPoint(float angle, float dist)
        {
            float x, y;
            GetDirs(out x, out y, angle, dist);
            return new PointF(x, y);
        }

        //redefines the exponent operation such that it behaves for all inputs 
        //very closely to its real behavior when base is greater than 1
        internal static float VectorExponent(float expBase, float expPower)
        {
            return (float)( Math.Sign(expBase) * ( Math.Pow(Math.Abs(expBase) + 1.0, expPower) - 1.0 ) );
        }

        internal static int Round(float value)
        {
            return (int)Math.Round(value);
        }

        internal static float GetArea(float size)
        {
            return (float)( Math.PI * size * size );
        }

        #endregion //internal abstraction methods

        #region game logic

        internal Game(GameTicker.EventDelegate Refresh, int centerX, int centerY, Replay replay)
            : this(Refresh, replay.Seed, centerX, centerY, false, replay, true)
        {
            Start();
        }

        internal Game(GameTicker.EventDelegate Refresh, int centerX, int centerY, bool scoring, bool allowReplay)
            : this(Refresh, NewSeed(allowReplay), centerX, centerY, scoring, allowReplay)
        {
        }

        private Game(GameTicker.EventDelegate Refresh, uint[] seed, int centerX, int centerY, bool scoring, bool allowReplay)
            : this(Refresh, seed, centerX, centerY, scoring, allowReplay ? new Replay(seed) : null, false)
        {
        }

        private Game(GameTicker.EventDelegate Refresh, uint[] seed, int centerX, int centerY, bool scoring, Replay replay, bool isReplay)
            : base(GameTick, Refresh)
        {
            SpaceRunner.Images.Generator.Generate();

            if (seed == null)
                this.gameRand = null;
            else
                this.gameRand = new MTRandom(seed);

            this.tickCount = 0;
            this.isReplay = isReplay;
            this.replay = replay;
            this.Scoring = scoring;

            this.objects.Clear();
            this.centerX = centerX;
            this.centerY = centerY;

            this.inputAngle = GetRandomAngle();

            this.score = 0;

            this.life = StartLife;
            this.fuel = StartFuel;
            this.ammo = StartAmmo;

            this.deadCounter = -1;
            this.fireCounter = -1;

            this.alienCount = 1;

            this.Turbo = false;
            this.fire = null;

            CreateStartObjects();

            Running = true;
            Started = false;
        }

        private static uint[] NewSeed(bool allowReplay)
        {
            const float AvgSeedSize = 13;

            if (allowReplay)
            {
                const int max = MTRandom.MAX_SEED_SIZE - 1;
                uint[] seed = MTRandom.GenerateSeed((ushort)( Game.Random.WeightedInt(max, ( AvgSeedSize - 1f ) / max) + 1 ));
                for (int idx = 0 ; idx < seed.Length ; ++idx)
                    seed[idx] += Game.Random.NextUInt();
                return seed;
            }

            return null;
        }

        //1 power up, 3 aliens, random number of asteroids
        private void CreateStartObjects()
        {
            PointF start;
            int startAsteroids = GameRand.GaussianOEInt(7.8f, .39f, .26f, 1);
            for (int idx = 0 ; idx < startAsteroids ; idx++)
            {
                start = RandomStartPoint(AsteroidMaxSize);
                Asteroid.NewAsteroid(this, start.X, start.Y);
            }

            start = RandomStartPoint(PowerUpSize);
            PowerUp.NewPowerUp(this, start.X, start.Y);

            const int startAliens = 3;
            for (int idx = 0 ; idx < startAliens ; idx++)
            {
                start = RandomStartPoint(AlienSize);
                Alien.NewAlien(this, start.X, start.Y);
            }
        }

        internal GameObject GetPlayerObject()
        {
            float moveX, moveY;
            GetMoveDirs(out moveX, out moveY);
            return new GameObject.DummyObject(moveX, moveY);
        }

        public override void Step()
        {
            ++tickCount;
            if (replay != null)
            {
                if (IsReplay)
                    replay.Play(tickCount, ref inputAngle, ref turbo, ref fire);
                else
                    replay.Record(tickCount, inputAngle, turbo, fire);
            }

            if (Dead && ++deadCounter > DeathTime)
                deadCounter = -1;
            --fireCounter;

            TurnPlayer();
            MovePlayer();
            PlayerFiring();

            float moveX, moveY;
            GetMoveDirs(out moveX, out moveY);
            MoveAndCollide(moveX, moveY);

            CreateObjects();
        }

        private void TurnPlayer()
        {
            float input;
            if (IsFiring())
                input = fire.Value;
            else
                input = inputAngle;

            float diff = NormalizeAngle(input - moveAngle);
            float turnSpeed = GetTurnSpeed();
            if (diff > turnSpeed)
                diff = turnSpeed;
            else if (diff < -turnSpeed)
                diff = -turnSpeed;

            moveAngle = NormalizeAngle(moveAngle + diff);
        }

        private float GetTurnSpeed()
        {
            return TurnSpeed * TotalSpeed;
        }

        private static float NormalizeAngle(float diff)
        {
            if (diff > Math.PI)
                diff -= TwoPi;
            else if (diff < -Math.PI)
                diff += TwoPi;
            return diff;
        }

        private void MovePlayer()
        {
            if (Turbo)
                fuel -= GameRand.OE(TurboSpeed);

            AddScore((decimal)TotalSpeed * DistanceScore);
        }

        private void PlayerFiring()
        {
            if (IsFiring())
            {
                if (Math.Abs(fire.Value - moveAngle) < GetTurnSpeed())
                {
                    if (--ammo < 1)
                        fireCounter = -1;
                    else
                        fireCounter = GameRand.Round(GetCoolDown());
                    float dirX, dirY;
                    GetDirs(out dirX, out dirY, fire.Value);
                    Bullet.NewBullet(this, 0, 0, dirX, dirY, TotalSpeed, PlayerSize, Bullet.FriendlyStatus.Friend);
                    fire = null;
                }
            }
            else if (fire.HasValue)
            {
                fire = null;
            }
        }

        private bool IsFiring()
        {
            return ( fire.HasValue && CanFire() );
        }
        private bool CanFire()
        {
            return ( !Dead && fireCounter < 0 && ammo > 0 );
        }

        private float GetCoolDown()
        {
            return (float)( FireTimeMult / Math.Pow(ammo + FireTimeAmmoAdd, FireTimePower) );
        }

        private void MoveAndCollide(float xSpeed, float ySpeed)
        {
            var objectSectors = MoveObjects(xSpeed, ySpeed);
            CollideObjects(objectSectors);
        }

        private Dictionary<Point, List<GameObject>> MoveObjects(float xSpeed, float ySpeed)
        {
            var objectSectors = new Dictionary<Point, List<GameObject>>();

            foreach (GameObject obj in GameRand.Iterate(this.objects))
            {
                //move the object
                obj.Step(xSpeed, ySpeed);

                //make sure the object is still in the game
                if (this.objects.Contains(obj))
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
        internal void HitPlayer(float damage, bool randomize = true)
        {
            if (randomize)
                damage = GameRand.GaussianCapped(damage, PlayerDamageRandomness);

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
            foreach (var pair in GameRand.Iterate(objectSectors))
            {
                Point point = pair.Key;
                List<GameObject> curSector = pair.Value;

                for (int idx = 0 ; idx < curSector.Count ; ++idx)
                {
                    GameObject obj = curSector[idx];
                    //make sure the object is still in the game
                    if (this.objects.Contains(obj))
                        CollideObject(objectSectors, obj, point, idx, done);
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

            foreach (Point p2 in GameRand.Iterate(point.X - checkDist, point.X + checkDist, point.Y - checkDist, point.Y + checkDist))
            {
                List<GameObject> value;
                if (objectSectors.TryGetValue(p2, out value) && ( !done.Contains(p2) ||
                        ( checkDist > 1 && ( Math.Abs(point.X - p2.X) > 1 || Math.Abs(point.Y - p2.Y) > 1 ) ) ))
                {
                    int start = 0;
                    //when checking the object's own sector, we only need to check objects with a higher index
                    if (p2 == point)
                        start = objIndex + 1;

                    for (int idx = start ; idx < value.Count ; ++idx)
                    {
                        GameObject checkObj = value[idx];
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
                        if (this.objects.Contains(checkObj))
                            if (obj.CheckCollision(checkObj))
                                //if the collision killed the main object, return early
                                if (!this.objects.Contains(obj))
                                    return;
                    }
                }
            }
        }

        private void CreateObjects()
        {
            if (GameRand.Bool(TotalSpeed * AlienCreationRate))
                Alien.NewAlien(this);
            if (GameRand.Bool((float)( Math.Pow(tickCount, AlienShipCreationTickPower) * AlienShipCreationRate
                    * TotalSpeed / Math.Pow(alienCount, AlienShipCreationCountPower) )))
                AlienShip.NewAlienShip(this);
            if (GameRand.Bool(TotalSpeed * AsteroidCreationRate))
                Asteroid.NewAsteroid(this);
            if (GameRand.Bool(TotalSpeed * LifeDustCreationRate))
                LifeDust.NewLifeDust(this);
            if (GameRand.Bool(TotalSpeed * PowerUpCreationRate))
                PowerUp.NewPowerUp(this);
        }

        protected override void OnEnd()
        {
            AddScore((decimal)ammo * RemainingAmmoScore);
            AddScore((decimal)fuel * RemainingFuelScore);

            if (replay != null)
                replay.EndRecord(this.tickCount);
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
