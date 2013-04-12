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
                move(direction);
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
        }

        public void HitEater(SquirrelEater se)
        {
            Program.RemovePiece(se);

            Program.player.AddScore(se.Score);
        }
        public void HitSquirrel(Squirrel s)
        {
            Program.RemovePiece(s);

            int dir;
            do
            {
                dir = Program.rand.Next(4);
            } while (dir == this.direction);
            Program.AddPiece(new ThrownSquirrel(X, Y, dir));
        }
    }
}