using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class SquirrelEater : Piece
    {
        double size, move, moveSpeed;

        static double StartSize
        {
            get
            {
                return 300.0 * Program.TimeMult;
            }
        }

        static char small = Convert.ToChar(246), large = Convert.ToChar(214);

        public double Size
        {
            get
            {
                return size / StartSize;
            }
        }

        public double Score
        {
            get
            {
                return size;
            }
        }

        public SquirrelEater()
            : base(small, ConsoleColor.Green)
        {
            InitStuff();
        }

        public SquirrelEater(int x, int y)
            : base(x, y, small, ConsoleColor.Green)
        {
            InitStuff();
        }

        private void InitStuff()
        {
            size = Program.rand.OE(StartSize);
            moveSpeed = 2100.0 / ( 13.0 * Program.TimeMult + Program.rand.OE(21.0 + 3.0 * Program.TimeMult) );
            move = moveSpeed;

            HideStuff(3.9 * Program.TimeMult, 13);
        }

        public override void Move()
        {
            move--;

            while (move < 0)
            {
                int dist = int.MaxValue;
                Piece target = null;

                foreach (Piece p in Program.rand.Iterate(Program.Pieces))
                    if (p is Squirrel || p is Player)
                    {
                        int newDist = GetDist(p);
                        if (newDist > 0 && newDist < dist)
                        {
                            dist = newDist;
                            target = p;
                        }
                    }

                Goto(target.X - X, target.Y - Y);
            }

            size -= 0.026 / Math.Sqrt(moveSpeed);
            if (size < 0)
            {
                Program.RemovePiece(this);
            }
            else if (Size < 2.1)
            {
                character = small;
            }
            else
            {
                character = large;
                HideStuff(( Size - 1.69 ) / 3.9, 1);
            }
        }

        private void Goto(int x, int y)
        {
            if (Program.rand.Bool(Math.Abs(x) / (double)( Math.Abs(x) + Math.Abs(y) )))
                X += Math.Sign(x);
            else
                Y += Math.Sign(y);

            move += moveSpeed;

            size--;
        }

        private int GetDist(Piece p)
        {
            return Math.Abs(X - p.X) + Math.Abs(Y - p.Y);
        }

        internal void Eat(Squirrel s)
        {
            Program.RemovePiece(s);

            size += Program.rand.OE(StartSize);
            while (Program.rand.OE(Size) > 3)
            {
                Program.AddPiece(new SquirrelEater(X, Y));
                size -= StartSize / 3;
            }
        }

        private void HideStuff(double amount, int times)
        {
            for (int a = 0 ; a < times ; a++)
            {
                int x = X, y = Y, amt = Program.rand.OEInt(amount);
                for (int i = 0 ; i < amt ; i++)
                {
                    Program.RandCoords(ref x, ref y);
                    Program.getTerrain(x, y).Visible = false;
                }
            }
        }
    }
}