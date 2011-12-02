using System;
using System.Collections.Generic;
using System.Text;

namespace CGame
{
    static class Program
    {
        const bool modFlag = false;

        //arbitrary values
        const int ENEMYCHASEVALUE = 6;	//must be at least 4, higher means enemies more likely chase you
        const int EMENYSTARTCOUNT = 5;	//number of times to call createEnemies at the start

        //score constants
        const double GOTGOAL = 10; //score gained for reaching the goal
        const double SHOTGOAL = 3; //score gained for shooting the goal
        const double NEGATIVELIFE = 13; //score lost for having negative life

        const double NEXTRANDOMMULT = .78; //multiple of next life actual and calculated difference received as score
        const double GETLIFE = 1 / .3; //score required to receive a new life

        //characters
        const char NOTHING = (char)0;
        const char PLAYER = (char)1;
        const char GOAL = (char)3;
        static readonly char VERTICAL = Convert.ToChar(9474);
        static readonly char HORIZONTAL = Convert.ToChar(9472);
        static readonly char WALL = Convert.ToChar(9608);

        static MattUtil.MTRandom Random;

        //game parameters, constant throughout a single game
        static int _height;			    //playing area height
        static int _width;				//playing area width
        static int _totalSides; 		//used for side numbers
        static int _numWalls;
        static double _startEnemies;	//number of starting enemies
        static double _enemiesPerTurn;	//number of enemies that appear per turn
        static double _enemySpeed;		//average enemy speed
        static double _stillEnemySL;	//score lost when an enemy doesnt move
        static double _playerMoveSL;	//score lost when the player moves
        static double _bulletMoveSL;	//score lost when a bullet moves
        static double _bulletKillSL;	//score lost when a bullet kills and enemy
        static double _killFraction;	//fraction of an enemy killed when hit by a bullet when playing with the mod option

        //game variables per game
        static char[,] data;
        static HashSet<Point> enemies = new HashSet<Point>();
        static double newEnemies;
        static int pX, pY, gX, gY;
        static int lives, nextLife;
        static double score;

        static void initParams()
        {
            _height = 13;
            _width = 39;
            double mapMult = _height * _width;

            getValue(ref _height, 6, 18);
            getValue(ref _width, 13, 65);

            Console.WindowHeight = _height + 7;
            Console.WindowWidth = Math.Max(_width + 4, 41);
            Console.BufferWidth = Console.WindowWidth;

            _totalSides = ( _height + _width );
            double mapSize = ( _height * _width );
            mapMult = Math.Sqrt(mapSize / mapMult);

            _numWalls = Random.Round(mapSize * .13);
            getValue(ref _numWalls, Random.Round(mapSize * .06), Random.Round(mapSize * .39));

            _startEnemies = mapMult * 39.0;
            getValue(ref _startEnemies, mapMult * 30.0, mapMult * 52.0);

            _enemiesPerTurn = 1.69 * Math.Sqrt(mapMult * mapSize * .13 / _numWalls * Math.Sqrt(mapMult * 39.0 / _startEnemies));

            _enemySpeed = Math.Max(Math.Max(1, _enemiesPerTurn) * 1.69, mapMult * 3.9);
            getValue(ref _enemySpeed, Math.Max(1, _enemiesPerTurn) * 1.3, Math.Max(_enemySpeed * 1.3, 6.5));

            _stillEnemySL = 0; // ( 2.0 / _enemiesPerTurn / 260.0 );
            _playerMoveSL = ( 1.0 / mapMult / 260.0 );
            _bulletMoveSL = ( _playerMoveSL / 1.69 );
            _bulletKillSL = ( .03 / _enemiesPerTurn );

            _killFraction = .5;
            getValue(ref _killFraction, .39, .65);
        }

        static void getValue(ref int value, int min, int max)
        {
            int diff = max - min;
            value = min + Random.WeightedInt(diff, ( value - min ) / (float)diff);
        }

        static void getValue(ref double value, double min, double max)
        {
            double diff = max - min;
            value = min + Random.Weighted(diff, ( value - min ) / diff);
        }

        static void Main(string[] args)
        {
            Random = new MattUtil.MTRandom();
            Random.StartTick();

            while (true)
            {
                initParams();
                newGame();

                //game loop;
                bool quit = false;
                while (lives > 0)
                {
                    if (-1 == gX)
                        placeGoal();

                    drawMap();

                    char[,] dataOld = (char[,])data.Clone();
                    HashSet<Point> enemiesOld = new HashSet<Point>(enemies);
                    int pXOld = pX, pYOld = pY, gXOld = gX, gYOld = gY;
                    int livesOld = lives;
                    double scoreOld = score;

input:
                    if (playerInput())
                    {
                        quit = true;
                        break;
                    }

                    Dictionary<Point, double> killers = getKillers();
                    double chance = 1, total = 0;
                    foreach (KeyValuePair<Point, double> pair in killers)
                    {
                        chance *= ( 1 - pair.Value );
                        total += pair.Value;
                    }
                    chance = ( 1 - chance );
                    if (livesOld > lives)
                        chance = 1;
                    if (chance > .005)
                    {
                        --Console.CursorTop;
                        --Console.WindowTop;
                        double scoreLoss = ( total - chance + livesOld - lives ) * NEGATIVELIFE;
                        Console.Write("Hit chance {0:0}%{1}, continue? (y/n)", chance * 100,
                            scoreLoss > .05 ? string.Format(" (-{0:0.0})", scoreLoss) : "");
                        ConsoleKey k = Console.ReadKey().Key;
                        Console.CursorLeft = 0;
                        StringBuilder b = new StringBuilder();
                        b.Append(' ', Console.BufferWidth - 1);
                        Console.Write(b.ToString());
                        Console.CursorLeft = 0;
                        if (k != ConsoleKey.Y)
                        {
                            data = (char[,])dataOld.Clone();
                            enemies = new HashSet<Point>(enemiesOld);
                            pX = pXOld;
                            pY = pYOld;
                            gX = gXOld;
                            gY = gYOld;
                            lives = livesOld;
                            score = scoreOld;
                            goto input;
                        }

                        foreach (KeyValuePair<Point, double> pair in killers)
                            if (Random.Bool(pair.Value))
                            {
                                --lives;
                                data[pair.Key.x, pair.Key.y] = NOTHING;
                                enemies.Remove(pair.Key);
                            }
                    }

                    if (lives < --livesOld)
                    {
                        score += ( lives - livesOld ) * NEGATIVELIFE;
                        lives = livesOld;
                    }
                    while (score >= nextLife)
                    {
                        IncNextLife(nextLife);
                        ++lives;
                    }

                    moveEnemies();
                    newEnemies += _enemiesPerTurn;
                    createEnemies();
                }

                drawMap();
                score += lives * NEGATIVELIFE;
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

        private static void IncNextLife(double nextLife)
        {
            const double m1 = 8.0 / GETLIFE;
            const double m2 = GETLIFE / 2.0;
            double avg = nextLife + ( Math.Sqrt(nextLife * m1 + 1) + 1 ) * m2;
            nextLife += Random.GaussianCapped((float)avg - nextLife, .09, GETLIFE * .78);
            Program.nextLife = Random.Round(nextLife);
            score += ( Program.nextLife - avg ) * NEXTRANDOMMULT;
        }

        static void newGame()
        {
            //reset variables
            lives = 2;
            if (modFlag)
                lives *= 6;
            score = GETLIFE;
            IncNextLife(GETLIFE);
            gX = -1;
            gY = -1;
            int i, j;
            data = new char[_width, _height];
            enemies.Clear();

            for ( ; _numWalls > 0 ; --_numWalls)
                placeWall();

            Point? failed;
            while (( failed = testWalls() ).HasValue)
            {
                i = failed.Value.x;
                j = failed.Value.y;

                do
                {
                    switch (Random.Next(4))
                    {
                    case 0:
                        if (( i + 1 ) < _width)
                            ++i;
                        break;
                    case 1:
                        if (i > 0)
                            --i;
                        break;
                    case 2:
                        if (( j + 1 ) < _height)
                            ++j;
                        break;
                    case 3:
                        if (j > 0)
                            --j;
                        break;
                    default:
                        throw new Exception();
                    }
                } while (NOTHING == data[i, j]);
                data[i, j] = NOTHING;

                placeWall();
            }

            //create start enemies
            newEnemies = _startEnemies;
            for (i = 0 ; i < EMENYSTARTCOUNT ; ++i)
                createEnemies();

            //place player
            do
            {
                pX = 2 + Random.Next(_width - 4);
                pY = 2 + Random.Next(_height - 4);
            } while (NOTHING != data[pX, pY]);
            data[pX, pY] = PLAYER;
        }

        static void placeWall()
        {
            int i, j;
            do
            {
                i = Random.Next(_width);
                j = Random.Next(_height);
            } while (WALL == data[i, j]);
            data[i, j] = WALL;
        }

        static Point? testWalls()
        {
            int sX, sY;
            do
            {
                sX = Random.Next(_width);
                sY = Random.Next(_height);
            } while (WALL == data[sX, sY]);
            bool[,] reached = new bool[_width, _height];
            testHex(sX, sY, reached);

            foreach (int a in Random.Iterate(_width))
                foreach (int b in Random.Iterate(_height))
                    if (!reached[a, b] && WALL != data[a, b])
                        return new Point(a, b);

            return null;
        }

        static void testHex(int x, int y, bool[,] reached)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height || reached[x, y])
                return;

            reached[x, y] = true;

            if (NOTHING == data[x, y])
            {
                testHex(x, y + 1, reached);
                testHex(x, y - 1, reached);
                testHex(x + 1, y, reached);
                testHex(x - 1, y, reached);
            }
        }

        static bool playerInput()
        {
            string input = Console.ReadLine().ToLower();

            //check if its a number to move the player
            int num;
            if (int.TryParse(input, out num))
            {
                data[pX, pY] = NOTHING;

                if (num < 0)
                    num = 0;
                else if (num >= _totalSides)
                    num = _totalSides - 1;

                //gather variables needed for movePlayer
                bool flag = ( num < _width );
                int mod;
                int other;
                if (flag)
                {
                    mod = pX;
                    other = pY;
                }
                else
                {
                    num -= _width;
                    mod = pY;
                    other = pX;
                }

                movePlayer(ref mod, other, flag, num, _playerMoveSL, playerCollisions);
                int old;
                if (flag)
                {
                    old = pX;
                    pX = mod;
                }
                else
                {
                    old = pY;
                    pY = mod;
                }
                char oldChar = data[pX, pY];
                data[pX, pY] = PLAYER;
            }
            else if (input.Length > 0)
            {
                //mod is a dummy variable since were not actually moving the player
                int mod = 0, other = 0;
                bool xFlag = false, dirFlag = false, fireBullet = false;
                switch (input[0])
                {
                case 'w':
                    mod = pY;
                    other = pX;
                    xFlag = false;
                    dirFlag = false;
                    fireBullet = true;
                    break;
                case 's':
                    mod = pY;
                    other = pX;
                    xFlag = false;
                    dirFlag = true;
                    fireBullet = true;
                    break;
                case 'a':
                    mod = pX;
                    other = pY;
                    xFlag = true;
                    dirFlag = false;
                    fireBullet = true;
                    break;
                case 'd':
                    mod = pX;
                    other = pY;
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
                    movePlayer(ref mod, other, xFlag, dirFlag ? ( xFlag ? _width : _height ) - 1 : 0,
                            modFlag ? 0 : _bulletMoveSL, bulletCollisions);
                else
                    //no decent input was given, so recurse
                    return playerInput();
            }

            return false;
        }

        static void drawMap()
        {
            int i, j;

            //top numbers
            Console.Write("\n   ");
            for (i = 0 ; i < _width ; ++i)
            {
                Console.Write(( (int)( i / 10 ) ).ToString());
            }
            Console.Write("\n   ");
            for (i = 0 ; i < _width ; ++i)
            {
                Console.Write(( (int)( i % 10 ) ).ToString());
            }
            Console.Write("\n\n");
            for (i = _width ; i < _totalSides ; ++i)
            {
                //side numbers
                Console.Write("{0} ", i);

                //map
                for (j = 0 ; j < _width ; ++j)
                {
                    char c = data[j, i - _width];
                    Console.Write(c);
                    //clear the bullet path, but after it is drawn
                    if (HORIZONTAL == c || VERTICAL == c)
                        data[j, i - _width] = NOTHING;
                }
                Console.Write("\n");
            }

            //show player info
            Console.Write("\nyou: ({0},{1})\tgoal: ({2},{3})\nscore:{4} \tlives:{5} \tnext:{6}\n",
                    pX, _width + pY, gX, _width + gY, (int)score, lives, nextLife);
        }

        static void createEnemies()
        {
            while (newEnemies > 0)
                newEnemy(1 + Random.WeightedInt(8, ( _enemySpeed - 1.0 ) / 8.0));
        }

        static void newEnemy(int curenemy)
        {
            //find a spot on the edge of the playing area
            int eX, eY;
            do
            {
                switch (Random.Next(4))
                {
                case 0:
                    eX = 0;
                    eY = Random.Next(_height);
                    break;
                case 1:
                    eX = _width - 1;
                    eY = Random.Next(_height);
                    break;
                case 2:
                    eY = 0;
                    eX = Random.Next(_width);
                    break;
                case 3:
                    eY = _height - 1;
                    eX = Random.Next(_width);
                    break;
                default:
                    throw new Exception();
                }
            } while (NOTHING != data[eX, eY]);

            enemies.Add(new Point(eX, eY));

            //place the enemy
            data[eX, eY] = curenemy.ToString()[0];
            //subtract from the number of enemies to be placed
            newEnemies -= curenemy;
        }

        static void moveEnemies()
        {
            foreach (Point p in Random.Iterate(enemies))
            {
                int eX = p.x, eY = p.y;
                int speed = getEnemy(p);
                if (speed > 0)
                {
                    for ( ; speed > 0 ; --speed)
                        if (moveEnemy(p))
                        {
                            score -= _stillEnemySL * speed;
                            break;
                        }
                }
                else
                {
                    enemies.Remove(p);
                }
            }
        }

        private static int getEnemy(Point p)
        {
            int speed = data[p.x, p.y] - '0';
            if (speed > 9)
                speed = -1;
            return speed;
        }

        private static Dictionary<Point, double> getKillers()
        {
            Dictionary<Point, double> retVal = new Dictionary<Point, double>();
            foreach (Point p in enemies)
            {
                double chance = doKillChance(p, getEnemy(p), p.x, p.y);
                if (chance > 0)
                    retVal.Add(p, Math.Sqrt(chance));
            }
            return retVal;
        }

        private static double doKillChance(Point orig, int speed, int x, int y)
        {
            const double mult = ENEMYCHASEVALUE - 4;
            double chance = 0;
            double tot = 2 * ENEMYCHASEVALUE;
            if (speed > 0 && Math.Abs(x - pX) + Math.Abs(y - pY) <= speed)
                for (int rand = 0 ; rand < 5 ; ++rand)
                    for (int b = 0 ; b < ( rand > 3 ? 2 : 1 ) ; ++b)
                    {
                        double add = 0;
                        int xDif, yDif, flag;
                        Point move = getMove(new Point(x, y), out xDif, out yDif, out flag, rand, b == 0);
                        if (new Point(pX, pY).Equals(move))
                            add = 1;
                        else if (speed > 1)
                            if (move.x > -1 && move.x < _width && move.y > -1 && move.y < _height
                                    && ( orig.Equals(move) || NOTHING == data[move.x, move.y] ))
                                add = doKillChance(orig, speed - 1, move.x, move.y);
                            else
                                tot -= ( rand > 3 ? mult / 2.0 : 1 );
                        chance += add * ( rand > 3 ? mult : 2 );
                    }
            return chance / tot;
        }

        private static Point getMove(Point p, out int xDif, out int yDif, out int flag, int rand, bool rBool)
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
                xDif = pX - eX;
                yDif = pY - eY;
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

        static bool moveEnemy(Point p)
        {
            int oldX = p.x, oldY = p.y, xDif, yDif, flag;
            Point move = getMove(p, out xDif, out yDif, out flag, Random.Next(ENEMYCHASEVALUE), Random.Bool());
            int eX = move.x, eY = move.y;

actualmove:
            //Note that the following logic defaults to being unable to move,
            //so any square type not specifically included cannot be moved over by enemies.
            bool nomove = false;
            //check the edges of the playing area
            if (!( eX < 0 || eY < 0 || eX >= _width || eY >= _height ))
                if (PLAYER == data[eX, eY])
                {
                    nomove = true;
                }
                else if (NOTHING == data[eX, eY] ||
                        ( modFlag && ( VERTICAL == data[eX, eY] || HORIZONTAL == data[eX, eY] ) ))
                {
                    data[eX, eY] = data[oldX, oldY];
                    data[oldX, oldY] = NOTHING;
                    enemies.Remove(p);
                    enemies.Add(new Point(eX, eY));
                }
                else if (flag > 0)
                {
                    //if flag was set, the enemy was moving towards the player, 
                    //so try to move the other direction that is still towards the player
                    if (1 == flag)
                    {
                        //rollback the previous choice
                        if (xDif > 0)
                            --eX;
                        else
                            ++eX;
                        //try the other direction
                        if (yDif > 0 || ( 0 == yDif && Random.Bool() ))
                            ++eY;
                        else
                            --eY;
                    }
                    else
                    {
                        //rollback the previous choice
                        if (yDif > 0)
                            --eY;
                        else
                            ++eY;
                        //try the other direction
                        if (xDif > 0 || ( 0 == xDif && Random.Bool() ))
                            ++eX;
                        else
                            --eX;
                    }
                    //remove the flag so we dont loop endlessly
                    flag = 0;
                    goto actualmove;
                }
                else
                {
                    nomove = true;
                }
            else
                nomove = true;

            if (nomove)
                //half the time, try again
                if (Random.Bool())
                    return moveEnemy(p);
                else
                    score -= _stillEnemySL;

            return false;
        }

        delegate int CollisionsDelegate(int x, int y);

        static void movePlayer(ref int mod, int other, bool xFlag, int num, double scoreLoss, CollisionsDelegate collisions)
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
                int retval = collisions(xFlag ? mod : other, xFlag ? other : mod);
                if (retval > 0)
                {
                    if (2 == retval)
                    {
                        //rollback the last move
                        score += scoreLoss;
                        if (dirFlag)
                            --mod;
                        else
                            ++mod;
                    }
                    //quit moving
                    break;
                }
            }
        }

        static int playerCollisions(int x, int y)
        {
            int num = ( data[x, y] - '0' );
            if (num > 0 && num < 10)
            {
                if (modFlag)
                    lives -= num;
                else
                    --lives;
                data[x, y] = NOTHING;
                return 0;
            }
            if (data[x, y] == WALL)
            {
                return 2;
            }
            else if (data[x, y] == GOAL)
            {
                score += GOTGOAL;
                data[x, y] = NOTHING;
                gX = -1;
                gY = -1;
                //goal doesnt stop movement
                return 0;
            }

            return 0;
        }

        static int bulletCollisions(int x, int y)
        {
            char c = data[x, y];
            int num = ( c - '0' );
            if (num > 0 && num < 10)
            {
                if (modFlag)
                {
                    int amtKilled = Random.GaussianInt(num * _killFraction, .13);
                    Console.Write("{0}", amtKilled);
                    score -= _bulletKillSL * amtKilled;
                    num -= amtKilled;
                    if (num > 0)
                        data[x, y] = num.ToString()[0];
                    else
                        data[x, y] = NOTHING;
                    return 0;
                }
                else
                {
                    score -= _bulletKillSL * num;
                    data[x, y] = NOTHING;
                    return 1;
                }
            }
            else
            {
                if (c == WALL)
                {
                    //no need to rollback the move since the *mod pointer isnt actually changing anything
                    return 1;
                }
                else if (c == GOAL)
                {
                    score += SHOTGOAL;
                    data[x, y] = NOTHING;
                    gX = -1;
                    gY = -1;
                    return 1;
                }
            }

            //show the bullet path
            data[x, y] = ( y == pY ? HORIZONTAL : VERTICAL );
            return 0;
        }

        static void placeGoal()
        {
            //find an empty square that does not have the same x or y coordinate as the player
            int nX, nY;
            do
            {
                nX = Random.Next(_width);
                nY = Random.Next(_height);
            } while (pX == nX || pY == nY || NOTHING != data[nX, nY]);
            data[nX, nY] = GOAL;
            gX = nX;
            gY = nY;
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
                return x * _height + y;
            }
            public override string ToString()
            {
                return string.Format("({0},{1})", x, y);
            }
        }
    }
}
