using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class Player : Piece
    {
        private int _hp, _squirrels, movement, moveSpeed, direction, fireCount;
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
                return score / 300.0;
            }
        }

        private int health
        {
            get
            {
                return _hp;
            }
            set
            {
                _hp = value;
                Program.OutPutValid = false;
            }
        }
        private int squirrels
        {
            get
            {
                return _squirrels;
            }
            set
            {
                _squirrels = value;
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
            health = 5;
            squirrels = 5;
            movement = 0;
            moveSpeed = 9;
            direction = -1;
            score = 0;
            fireCount = -1;
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

        public override void Move()
        {
            fireCount--;
            if (movement-- < 0)
                move(direction);
        }

        internal void GetSquirrel(Squirrel s)
        {
            Program.RemovePiece(s);

            squirrels++;
            if (squirrels > 9 && Program.rand.Bool(squirrels / ( squirrels + health * health * 3.9 )))
            {
                health++;
                squirrels -= 5;
            }
        }
        internal void HitEater(SquirrelEater squirrelEater)
        {
            Program.RemovePiece(squirrelEater);

            AddScore(squirrelEater.Score);
            health--;
        }
    }
}