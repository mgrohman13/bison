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
            moveSpeed = 1690.0 / ( 13.0 * Program.TimeMult + Program.rand.OE(21.0 + 3.0 * Program.TimeMult) );
            move = moveSpeed;

            HideStuff(3.9 * Program.TimeMult, 39);
        }

        public override void Move()
        {
            move--;

            while (move < 0)
            {
                int dist = int.MaxValue;
                Piece target = null;

                foreach (Piece p in Program.Pieces)
                    if (p is Squirrel || p is Player)
                    {
                        int newDist = GetDist(p);
                        if (newDist < dist)
                        {
                            dist = newDist;
                            target = p;
                        }
                    }

                Goto(target.X - X, target.Y - Y);
            }

            foreach (Piece p in Program.getPieces(X, Y))
                if (p is Squirrel)
                    Eat((Squirrel)p);
                else if (p is ThrownSquirrel)
                    ( (ThrownSquirrel)p ).HitEater(this);

            size -= .003;
            if (size < 0)
            {
                Program.RemovePiece(this);
                Program.player.AddScore(-StartSize / 3.0);
            }
            else if (Size < 2.1)
                character = small;
            else if (Size > 2.1)
            {
                character = large;
                HideStuff(( Size - 1.3 ) / 3.9, 1);
            }
        }

        private void Goto(int x, int y)
        {
            if (x == 0)
                Y += Math.Sign(y);
            else if (y == 0)
                X += Math.Sign(x);
            else
                switch (Program.rand.Next(2))
                {
                case 0:
                    X += Math.Sign(x);
                    break;

                case 1:
                    Y += Math.Sign(y);
                    break;
                }

            move += moveSpeed;

            size--;
        }

        private int GetDist(Piece p)
        {
            return Math.Abs(X - p.X) + Math.Abs(Y - p.Y);
        }

        private void Eat(Squirrel s)
        {
            Program.RemovePiece(s);
            size += Program.rand.OE(StartSize);

            while (Program.rand.OE(Size) > 3)
            {
                Program.AddPiece(new SquirrelEater(X, Y));
                size -= StartSize / 3.0;
            }
        }

        private void HideStuff(double amount, int times)
        {
            for (int a = 0 ; a < times ; a++)
            {
                int x = X, y = Y, amt = Program.rand.OEInt(amount);
                for (int i = 0 ; i < amt ; i++)
                {
                    while (Program.rand.Next(3) == 0)
                    {
                        switch (Program.rand.Next(4))
                        {
                        case 0:
                            x++;
                            if (x >= Program.Width)
                                x = Program.Width - 1;
                            break;

                        case 1:
                            x--;
                            if (x < 0)
                                x = 0;
                            break;

                        case 2:
                            y++;
                            if (y >= Program.Height)
                                y = Program.Height - 1;
                            break;

                        case 3:
                            y--;
                            if (y < 0)
                                y = 0;
                            break;
                        }
                    }

                    Program.getTerrain(x, y).Visible = false;
                }
            }
        }
    }
}