using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class ThrownSquirrel : Piece
    {
        int direction, movement, moveSpeed;

        public ThrownSquirrel(int x, int y, int dir)
            : base(x, y, '\'', ConsoleColor.Red)
        {
            this.direction = dir;
            movement = 0;
            moveSpeed = 1;
        }

        public override void Move()
        {
            if (movement-- < 0)
            {
                move(direction);
            }
        }

        public void move(int dir)
        {
            movement = moveSpeed;

            switch (dir)
            {
                case 0:
                    if (Y == 0)
                        Program.RemovePiece(this);
                    Y--;
                    break;

                case 1:
                    if (X == 0)
                        Program.RemovePiece(this);
                    X--;
                    break;

                case 2:
                    if (Y == Program.Height - 1)
                        Program.RemovePiece(this);
                    Y++;
                    break;

                case 3:
                    if (X == Program.Width - 1)
                        Program.RemovePiece(this);
                    X++;
                    break;
            }

            MoveHere();
        }

        private void MoveHere()
        {
            foreach (Piece p in Program.getPieces(X, Y))
                if (p is Squirrel)
                {
                    Program.AddPiece(new ThrownSquirrel(X, Y, Program.rand.Next(4)));
                    Program.RemovePiece(p);
                }
                else if (p is SquirrelEater)
                    HitEater((SquirrelEater)p);
        }

        public void HitEater(SquirrelEater se)
        {
            Program.player.AddScore(se.Score);
            Program.RemovePiece(se);
        }
    }
}