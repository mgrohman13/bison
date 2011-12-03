using System;
using System.Collections.Generic;
using System.Text;

namespace CGame
{
    static class Program
    {
        //arbitrary values
        const int ChaseValue = 6;           //must be at least 4, higher means enemies more likely chase you
        const int StartLives = 2;
        const int StartEdgeDistance = 3;    //minimum distance from the map edge the player can start

        //score constants
        const double GoalScore = 10;            //score gained for reaching the goal
        const double ShotGoalScore = 3;         //score gained for shooting the goal
        const double NegativeLifeScore = 4;     //score lost for having negative life
        const double NextLifeScoreMult = .78;   //multiple of next life actual and calculated difference received as score
        const double BaseNewLife = 1 / .3;      //score required to receive a new life

        //characters
        const char Empty = (char)0;
        const char Player = (char)1;
        const char Goal = (char)3;
        const char BulletVertical = (char)9474;
        const char BulletHorizontal = (char)9472;
        const char Wall = (char)9608;

        static MattUtil.MTRandom Random;

        //game parameters, constant throughout a single game
        static int Height;			    //playing area height
        static int Width;			    //playing area width
        static int TotalSides; 		    //used for side numbers
        static int NumWalls;            //number of walls in the playing area
        static double StartEnemies;	    //number of starting enemies
        static double EnemiesPerTurn;   //number of enemies that appear per turn
        static double EnemySpeed;		//average enemy speed
        static double PlayerMoveSL;	    //score lost when the player moves
        static double BulletMoveSL;	    //score lost when a bullet moves
        static double BulletKillSL;	    //score lost when a bullet kills and enemy
        static double KillFraction;	    //fraction of an enemy killed when hit by a bullet when playing with the mod option

        //game variables per game
        static char[,] map;
        static HashSet<Point> enemies;
        static double newEnemies;
        static int playerX, playerY, goalX, goalY;
        static int lives, nextLife;
        static double score;

        static void SetGameParameters()
        {
            const int AvgHeight = 13;
            const int AvgWidth = 39;

            Height = AvgHeight;
            Width = AvgWidth;

            RandValue(ref Height, 7, 25);
            RandValue(ref Width, 13, 75);

            Console.WindowHeight = Height + 7;
            Console.BufferWidth = 80;
            Console.BufferWidth = Console.WindowWidth = Math.Max(Width + 4, 41);

            TotalSides = Height + Width;
            double mapSize = Height * Width;
            double mapMult = Math.Sqrt(mapSize / AvgHeight / AvgWidth);

            double avgWalls = mapSize * .13;
            NumWalls = Random.Round(avgWalls);
            RandValue(ref NumWalls, Random.Round(mapSize * .06), Random.Round(mapSize * .39));

            double avgEnemies = mapMult * 39.0;
            StartEnemies = avgEnemies;
            RandValue(ref StartEnemies, .13, mapMult * 52.0);

            EnemiesPerTurn = 1.69 * Math.Sqrt(mapMult * avgWalls / NumWalls * Math.Sqrt(avgEnemies / StartEnemies));

            EnemySpeed = Math.Min(Math.Max(mapMult * 3.0, Math.Max(EnemiesPerTurn, 1) * 1.69), 6.5);
            RandValue(ref EnemySpeed, Math.Min(Math.Max(EnemiesPerTurn, 1) * 1.3, EnemySpeed / 1.3),
                    Math.Min(Math.Max(EnemySpeed * 2.1, 3.9), 7.8));

            PlayerMoveSL = ( 1.0 / mapMult / 260.0 );
            BulletMoveSL = ( PlayerMoveSL / 1.69 );
            BulletKillSL = ( .03 / EnemiesPerTurn );

            KillFraction = .5;
            //getValue(ref KillFraction, .39, .65);
        }

        static void RandValue(ref int value, int min, int max)
        {
            int diff = max - min;
            value = min + Random.WeightedInt(diff, ( value - min ) / (float)diff);
        }

        static void RandValue(ref double value, double min, double max)
        {
            double diff = max - min;
            value = min + Random.Weighted(diff, ( value - min ) / diff);
        }

        static void Main(string[] args)
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();

            enemies = new HashSet<Point>();
            while (true)
            {
                NewGame();

                //game loop;
                bool quit = false;
                while (lives > 0)
                {
                    if (-1 == goalX)
                        PlaceGoal();

                    DrawMap();

                    char[,] dataOld = (char[,])map.Clone();
                    HashSet<Point> enemiesOld = new HashSet<Point>(enemies);
                    int pXOld = playerX, pYOld = playerY, gXOld = goalX, gYOld = goalY;
                    int livesOld = lives;
                    double scoreOld = score;

input:
                    if (ProcessInput())
                    {
                        quit = true;
                        break;
                    }

                    Dictionary<Point, double> killers = GetKillers();
                    double chance = 1, total = 0;
                    foreach (double value in killers.Values)
                    {
                        chance *= ( 1 - value );
                        total += value;
                    }
                    if (livesOld > lives)
                        chance = 1;
                    else
                        chance = ( 1 - chance );
                    if (chance > .005)
                    {
                        if (ConfirmChance(total, chance, livesOld))
                        {
                            map = (char[,])dataOld.Clone();
                            enemies = new HashSet<Point>(enemiesOld);
                            playerX = pXOld;
                            playerY = pYOld;
                            goalX = gXOld;
                            goalY = gYOld;
                            lives = livesOld;
                            score = scoreOld;
                            goto input;
                        }

                        DamagePlayer(killers);
                    }

                    CheckLives(livesOld);

                    MoveEnemies();

                    newEnemies += Random.OE(EnemiesPerTurn);
                    CreateEnemies();
                }

                DrawMap();
                score += lives * NegativeLifeScore;
                Console.Write("\nGame Over!   Score:{0}\n", Random.Round(score));

                string input = Console.ReadLine().ToLower();
                if (!quit)
                    if (input.Length > 0)
                        switch (input[0])
                        {
                        case 'e':
                        case 'q':
                            if (input.Length == 1 || 'x' == input[1] || 'u' == input[1])
                                quit = true;
                            break;
                        }
                if (quit)
                    break;
            }

            Random.Dispose();
        }

        static void NewGame()
        {
            SetGameParameters();

            //reset all varaibles
            map = new char[Width, Height];
            enemies.Clear();
            newEnemies = StartEnemies;
            playerX = -1;
            playerY = -1;
            goalX = -1;
            goalY = -1;
            lives = StartLives;
            nextLife = -1;
            IncNextLife(BaseNewLife);
            score = BaseNewLife;

            //lay out map
            LayoutWalls();
            CreateEnemies();

            //place player
            Point p = RandomPoint(Player, StartEdgeDistance);
            playerX = p.x;
            playerY = p.y;
        }

        static void LayoutWalls()
        {
            for (int a = 0 ; a < NumWalls ; ++a)
                RandomPoint(Wall);

            Point? failed;
            while (( failed = TestWalls() ).HasValue)
            {
                int x = failed.Value.x;
                int y = failed.Value.y;
                do
                {
                    switch (Random.Next(4))
                    {
                    case 0:
                        if (( x + 1 ) < Width)
                            ++x;
                        break;
                    case 1:
                        if (x > 0)
                            --x;
                        break;
                    case 2:
                        if (( y + 1 ) < Height)
                            ++y;
                        break;
                    case 3:
                        if (y > 0)
                            --y;
                        break;
                    default:
                        throw new Exception();
                    }
                } while (PointIs(x, y, Empty));

                SetPoint(x, y, Empty);
                RandomPoint(Wall);
            }
        }

        static Point? TestWalls()
        {
            bool[,] reached = new bool[Width, Height];
            TestPoint(RandomPoint(), reached);

            foreach (int coord in Random.Iterate(Width * Height))
            {
                int x = coord / Height;
                int y = coord % Height;
                if (!reached[x, y] && !PointIs(x, y, Wall))
                    return new Point(x, y);
            }

            return null;
        }

        static void TestPoint(Point point, bool[,] reached)
        {
            int x = point.x, y = point.y;
            if (reached[x, y] || y < 0 || y >= Height || x < 0 || x >= Width)
                return;

            reached[x, y] = true;

            if (PointIs(x, y, Empty))
            {
                TestPoint(new Point(x, y + 1), reached);
                TestPoint(new Point(x, y - 1), reached);
                TestPoint(new Point(x + 1, y), reached);
                TestPoint(new Point(x - 1, y), reached);
            }
        }

        static void PlaceGoal()
        {
            Point p = RandomPoint(Goal);
            goalX = p.x;
            goalY = p.y;
        }

        static void DrawMap()
        {
            //top numbers
            Console.Write("\n   ");
            for (int x = 0 ; x < Width ; ++x)
                Console.Write(( (int)( x / 10 ) ).ToString());
            Console.Write("\n   ");
            for (int x = 0 ; x < Width ; ++x)
                Console.Write(( (int)( x % 10 ) ).ToString());
            Console.Write("\n\n");

            for (int x = Width ; x < TotalSides ; ++x)
            {
                //side numbers
                Console.Write("{0} ", x);

                //map
                for (int y = 0 ; y < Width ; ++y)
                {
                    char c = GetPoint(y, x - Width);
                    Console.Write(c);
                    //clear the bullet path, but after it has been drawn
                    if (BulletHorizontal == c || BulletVertical == c)
                        SetPoint(y, x - Width, Empty);
                }

                Console.Write("\n");
            }

            //show player info
            Console.Write("\nyou: ({0},{1})\tgoal: ({2},{3})\nscore:{4} \tlives:{5} \tnext:{6}\n",
                    playerX, Width + playerY, goalX, Width + goalY, (int)score, lives - 1, nextLife);
        }

        static bool ProcessInput()
        {
            string input = Console.ReadLine().ToLower();

            //check if it's a number to move the player
            int num;
            if (int.TryParse(input, out num))
            {
                MovePlayer(num);
            }
            else if (input.Length > 0)
            {
                //mod is a dummy variable since were not actually moving the player
                int mod = 0, other = 0;
                bool xFlag = false, dirFlag = false, fireBullet = false;
                switch (input[0])
                {
                case 'w':
                    mod = playerY;
                    other = playerX;
                    xFlag = false;
                    dirFlag = false;
                    fireBullet = true;
                    break;
                case 's':
                    mod = playerY;
                    other = playerX;
                    xFlag = false;
                    dirFlag = true;
                    fireBullet = true;
                    break;
                case 'a':
                    mod = playerX;
                    other = playerY;
                    xFlag = true;
                    dirFlag = false;
                    fireBullet = true;
                    break;
                case 'd':
                    mod = playerX;
                    other = playerY;
                    xFlag = true;
                    dirFlag = true;
                    fireBullet = true;
                    break;
                case 'e':
                case 'q':
                    if (input.Length > 1 && ( 'x' == input[1] || 'u' == input[1] ))
                        //returning true signifies the game loop should exit prematurely
                        return true;
                    break;
                }
                if (fireBullet)
                    MovePlayer(ref mod, other, xFlag, dirFlag ? ( xFlag ? Width : Height ) - 1 : 0,
                            BulletMoveSL, BulletCollisions);
                else
                    //no decent input was given, so recurse
                    return ProcessInput();
            }

            return false;
        }

        static int MovePlayer(int num)
        {
            SetPoint(playerX, playerY, Empty);

            if (num < 0)
                num = 0;
            else if (num >= TotalSides)
                num = TotalSides - 1;
            if (num < Width)
                MovePlayer(ref playerX, playerY, true, num, PlayerMoveSL, PlayerCollisions);
            else
                MovePlayer(ref playerY, playerX, false, num - Width, PlayerMoveSL, PlayerCollisions);

            SetPoint(playerX, playerY, Player);
            return num;
        }

        static void MovePlayer(ref int mod, int other, bool xFlag, int num, double scoreLoss, CollisionHandler Collisions)
        {
            bool dirFlag = ( num > mod );
            while (num != mod)
            {
                //move towards the destination
                score -= scoreLoss;
                if (dirFlag)
                    ++mod;
                else
                    --mod;

                //check for collisions
                switch (Collisions(xFlag ? mod : other, xFlag ? other : mod))
                {
                case Collision.ROLLBACK:
                    //rollback the last move
                    score += scoreLoss;
                    if (dirFlag)
                        --mod;
                    else
                        ++mod;
                    return;
                case Collision.STOP:
                    return;
                }
            }
        }

        static Collision BulletCollisions(int x, int y)
        {
            int enemy;
            if (PointIs(x, y, Empty))
            {
                //show the bullet path
                SetPoint(x, y, ( y == playerY ? BulletHorizontal : BulletVertical ));
                return Collision.MOVE;
            }
            else if (( enemy = GetEnemy(x, y) ) > 0)
            {
                score -= BulletKillSL * enemy;
                SetPoint(x, y, Empty);
            }
            else if (PointIs(x, y, Goal))
            {
                GetGoal(ShotGoalScore);
            }

            return Collision.STOP;
        }

        static Collision PlayerCollisions(int x, int y)
        {
            if (PointIs(x, y, Wall))
            {
                return Collision.ROLLBACK;
            }
            else if (GetEnemy(x, y) > 0)
            {
                --lives;
                SetPoint(x, y, Empty);
            }
            else if (PointIs(x, y, Goal))
            {
                GetGoal(GoalScore);
            }

            return Collision.MOVE;
        }

        private static void GetGoal(double addScore)
        {
            score += addScore;
            SetPoint(goalX, goalY, Empty);
            goalX = -1;
            goalY = -1;
        }

        static Dictionary<Point, double> GetKillers()
        {
            Dictionary<Point, double> retVal = new Dictionary<Point, double>();
            foreach (Point p in enemies)
            {
                double chance = GetKillChance(p, GetEnemy(p), p.x, p.y);
                if (chance > 0)
                    retVal.Add(p, Math.Sqrt(chance));
            }
            return retVal;
        }

        static double GetKillChance(Point orig, int speed, int x, int y)
        {
            const double m1 = 2 * ChaseValue;
            const double m3 = ChaseValue - 4;
            const double m2 = m3 / 2;

            double chance = 0;
            double tot = m1;
            if (Math.Abs(x - playerX) + Math.Abs(y - playerY) <= speed)
                for (int rand = 0 ; rand < 5 ; ++rand)
                    for (int rBool = 0 ; rBool < ( rand > 3 ? 2 : 1 ) ; ++rBool)
                    {
                        double add = 0;
                        int xDif, yDif, flag;
                        Point move = GetEnemyMove(new Point(x, y), out xDif, out yDif, out flag, rand, rBool == 0);
                        if (new Point(playerX, playerY).Equals(move))
                            add = 1;
                        else if (speed > 1)
                            if (move.x > -1 && move.x < Width && move.y > -1 && move.y < Height
                                    && ( orig.Equals(move) || PointIs(move.x, move.y, Empty) ))
                                add = GetKillChance(orig, speed - 1, move.x, move.y);
                            else
                                tot -= ( rand > 3 ? m2 : 1 );
                        chance += add * ( rand > 3 ? m3 : 2 );
                    }
            return chance / tot;
        }

        static bool ConfirmChance(double total, double chance, int livesOld)
        {
            --Console.CursorTop;
            --Console.WindowTop;

            double scoreLoss = ( total - chance + livesOld - lives ) * NegativeLifeScore;
            Console.Write("Hit chance {0:0}%{1}, continue? (y/n)", chance * 100,
                    scoreLoss > .05 ? string.Format(" (-{0:0.0})", scoreLoss) : "");

            ConsoleKey k = Console.ReadKey().Key;

            Console.CursorLeft = 0;
            StringBuilder b = new StringBuilder();
            b.Append(' ', Console.BufferWidth - 1);
            Console.Write(b.ToString());
            Console.CursorLeft = 0;

            return ( k != ConsoleKey.Y );
        }

        static void DamagePlayer(Dictionary<Point, double> killers)
        {
            foreach (KeyValuePair<Point, double> pair in killers)
                if (Random.Bool(pair.Value))
                {
                    --lives;
                    SetPoint(pair.Key.x, pair.Key.y, Empty);
                }
        }

        static void CheckLives(int livesOld)
        {
            if (lives < --livesOld)
            {
                score += ( lives - livesOld ) * NegativeLifeScore;
                lives = livesOld;
            }

            while (score >= nextLife)
            {
                IncNextLife(nextLife);
                ++lives;
            }
        }

        static void IncNextLife(double value)
        {
            const double m1 = 8.0 / BaseNewLife;
            const double m2 = BaseNewLife / 2.0;
            double avg = value + ( Math.Sqrt(value * m1 + 1) + 1 ) * m2;
            value += Random.GaussianCapped((float)avg - value, .09, BaseNewLife * .78);
            nextLife = Random.Round(value);
            score += ( nextLife - avg ) * NextLifeScoreMult;
        }

        static void MoveEnemies()
        {
            foreach (Point p in Random.Iterate(enemies))
            {
                int speed = GetEnemy(p);
                if (speed > 0)
                    for ( ; speed > 0 ; --speed)
                        MoveEnemy(p);
                else
                    throw new Exception();
            }
        }

        static void MoveEnemy(Point p)
        {
            int xDif, yDif, flag;
            Point move = GetEnemyMove(p, out xDif, out yDif, out flag, Random.Next(ChaseValue), Random.Bool());
            int x = move.x, y = move.y;

actualmove:
            //check the edges of the playing area
            if (!( x < 0 || y < 0 || x >= Width || y >= Height ))
                if (PointIs(x, y, Empty))
                {
                    SetPoint(x, y, GetPoint(p));
                    SetPoint(p, Empty);
                }
                else if (flag > 0)
                {
                    //if flag was set, the enemy was moving towards the player, 
                    //so try to move the other direction that is still towards the player
                    if (1 == flag)
                    {
                        //rollback the previous choice
                        if (xDif > 0)
                            --x;
                        else
                            ++x;
                        //try the other direction
                        if (yDif > 0 || ( 0 == yDif && Random.Bool() ))
                            ++y;
                        else
                            --y;
                    }
                    else
                    {
                        //rollback the previous choice
                        if (yDif > 0)
                            --y;
                        else
                            ++y;
                        //try the other direction
                        if (xDif > 0 || ( 0 == xDif && Random.Bool() ))
                            ++x;
                        else
                            --x;
                    }
                    //remove the flag so we dont loop endlessly
                    flag = 0;
                    goto actualmove;
                }
        }

        static Point GetEnemyMove(Point p, out int xDif, out int yDif, out int flag, int rand, bool rBool)
        {
            int eX = p.x;
            int eY = p.y;
            int oldX = eX;
            int oldY = eY;

            xDif = 0;
            yDif = 0;

            flag = 0;

            switch (rand)
            {
            //completely random direction
            case 0:
                --eX;
                break;
            case 1:
                ++eX;
                break;
            case 2:
                --eY;
                break;
            case 3:
                ++eY;
                break;

            //towards the player
            default:
                xDif = playerX - eX;
                yDif = playerY - eY;
                if (( Math.Abs(xDif) > Math.Abs(yDif) ) || ( ( Math.Abs(xDif) == Math.Abs(yDif) ) && rBool ))
                {
                    flag = 1;
                    if (xDif > 0)
                        ++eX;
                    else
                        --eX;
                }
                else
                {
                    flag = 2;
                    if (yDif > 0)
                        ++eY;
                    else
                        --eY;
                }
                break;
            }

            return new Point(eX, eY);
        }

        static int GetEnemy(Point p)
        {
            return GetEnemy(p.x, p.y);
        }

        static int GetEnemy(int x, int y)
        {
            int speed = GetPoint(x, y) - '0';
            if (speed > 9)
                speed = -1;
            return speed;
        }

        static void CreateEnemies()
        {
            while (newEnemies > 0)
                NewEnemy(1 + Random.WeightedInt(8, ( EnemySpeed - 1 ) / 8.0));
        }

        static void NewEnemy(int speed)
        {
            //find a spot on the edge of the playing area
            int eX, eY;
            do
            {
                switch (Random.Next(4))
                {
                case 0:
                    eX = 0;
                    eY = Random.Next(Height);
                    break;
                case 1:
                    eX = Width - 1;
                    eY = Random.Next(Height);
                    break;
                case 2:
                    eY = 0;
                    eX = Random.Next(Width);
                    break;
                case 3:
                    eY = Height - 1;
                    eX = Random.Next(Width);
                    break;
                default:
                    throw new Exception();
                }
            } while (!PointIs(eX, eY, Empty));

            //place the enemy
            SetPoint(eX, eY, speed.ToString()[0]);
            //subtract from the number of enemies to be placed
            newEnemies -= speed;
        }

        static Point RandomPoint()
        {
            return RandomPoint(Empty);
        }

        static Point RandomPoint(char type)
        {
            return RandomPoint(type, 0);
        }

        static Point RandomPoint(char type, int edgeDistance)
        {
            int x, y;
            do
            {
                int twice = edgeDistance * 2;
                x = edgeDistance + Random.Next(Width - twice);
                y = edgeDistance + Random.Next(Height - twice);
            } while (!PointIs(x, y, Empty));
            SetPoint(x, y, type);
            return new Point(x, y);
        }

        static bool PointIs(Point point, char type)
        {
            return PointIs(point.x, point.y, type);
        }

        static bool PointIs(int x, int y, char type)
        {
            return ( GetPoint(x, y) == type );
        }

        static char GetPoint(Point point)
        {
            return GetPoint(point.x, point.y);
        }

        static char GetPoint(int x, int y)
        {
            return map[x, y];
        }

        static void SetPoint(Point point, char type)
        {
            map[point.x, point.y] = type;

            if (GetEnemy(point) > 0)
                enemies.Add(point);
            else
                enemies.Remove(point);
        }

        static void SetPoint(int x, int y, char type)
        {
            SetPoint(new Point(x, y), type);
        }

        delegate Collision CollisionHandler(int x, int y);

        enum Collision
        {
            STOP,
            MOVE,
            ROLLBACK,
        }

        struct Point
        {
            public int x;
            public int y;
            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public override bool Equals(object obj)
            {
                return ( obj is Point && GetHashCode() == obj.GetHashCode() );
            }
            public override int GetHashCode()
            {
                return x * Height + y;
            }
            public override string ToString()
            {
                return string.Format("({0},{1})", x, y);
            }
        }
    }
}
