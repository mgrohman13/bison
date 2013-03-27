using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class Player : Piece
    {
        int hea, squ, movement, moveSpeed, direction, fireCount;
        double sco;

        public int Health
        {
            get
            {
                return health;
            }
        }
        public int Squirrels
        {
            get
            {
                return squirrels;
            }
        }
        public double Score
        {
            get
            {
                return score / 1000.0;
            }
        }

        private int health
        {
            get
            {
                return hea;
            }
            set
            {
                hea = value;
                Program.OutPutValid = false;
            }
        }
        private int squirrels
        {
            get
            {
                return squ;
            }
            set
            {
                squ = value;
                Program.OutPutValid = false;
            }
        }
        private double score
        {
            get
            {
                return sco;
            }
            set
            {
                sco = value;
                Program.OutPutValid = false;
            }
        }

        public Player(ConsoleColor color)
            : base(Convert.ToChar(2), color)
        {
            health = 6;
            squirrels = 3;
            movement = 0;
            moveSpeed = 9;
            direction = -1;
            score = 0;
            fireCount = 0;
        }

        public void MoveUp()
        {
            direction = 0;

            //Y--;
            //MoveHere();
        }
        public void MoveLeft()
        {
            direction = 1;

            //X--;
            //MoveHere();
        }
        public void MoveDown()
        {
            direction = 2;

            //Y++;
            //MoveHere();
        }
        public void MoveRight()
        {
            direction = 3;

            //X++;
            //MoveHere();
        }

        public void move(int dir)
        {
            //if (movement< 0)
            //{
            movement = moveSpeed;

            switch (dir)
            {
            case 0:
                Y--;
                break;

            case 1:
                X--;
                break;

            case 2:
                Y++;
                break;

            case 3:
                X++;
                break;
            }
            //}
        }

        public void fire(int dir)
        {
            if (fireCount < 0)
            {
                if (squirrels < 1)
                    return;

                squirrels--;
                Program.AddPiece(new ThrownSquirrel(X, Y, dir));

                fireCount = moveSpeed;
            }
        }

        public void Stop()
        {
            direction = -1;
        }

        //public void DropSquirrels()
        //{
        //    while (squirrels > 0)
        //    {
        //        squirrels--;
        //        Program.AddPiece(new Squirrel(X, Y));
        //    }
        //}

        public void AddScore(double score)
        {
            this.score += score;
        }

        public void MoveHere()
        {
            foreach (Piece p in Program.getPieces(X, Y))
                if (p is Squirrel)
                {
                    squirrels++;
                    if (Program.rand.OE(squirrels) > health * 10)
                    {
                        health++;
                        squirrels -= 10;
                    }
                    Program.RemovePiece(p);
                }
                else if (p is SquirrelEater)
                {
                    SquirrelEater se = (SquirrelEater)p;
                    score += se.Score;
                    health -= Program.rand.Round(se.Size);
                    Program.RemovePiece(p);
                }
        }

        public override void Move()
        {
            fireCount--;
            if (movement-- < 0)
            {
                move(direction);
            }
        }
    }
}