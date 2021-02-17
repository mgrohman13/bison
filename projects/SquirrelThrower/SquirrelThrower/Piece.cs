using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    abstract class Piece
    {
        protected Piece(char character, ConsoleColor foreColor)
        {
            InitStuff(Program.RandX(), Program.RandY(), character, foreColor, ConsoleColor.White, false);
        }

        protected Piece(char character, ConsoleColor foreColor, ConsoleColor backColor)
        {
            InitStuff(Program.RandX(), Program.RandY(), character, foreColor, backColor, true);
        }

        protected Piece(int x, int y, char character, ConsoleColor foreColor)
        {
            InitStuff(x, y, character, foreColor, ConsoleColor.White, false);
        }

        protected Piece(int x, int y, char character, ConsoleColor foreColor, ConsoleColor backColor)
        {
            InitStuff(x, y, character, foreColor, backColor, true);
        }

        private void InitStuff(int x, int y, char character, ConsoleColor foreColor, ConsoleColor backColor, bool backGround)
        {
            this.x = x;
            this.y = y;
            this.character = character;
            this.foreColor = foreColor;
            this.backColor = backColor;
            this.backGround = backGround;
        }

        protected bool backGround;
        int x, y;
        protected char character;
        protected ConsoleColor foreColor, backColor;

        public void SetXY(int X, int Y)
        {
            if (X < 0 || X >= Program.Width || Y < 0 || Y >= Program.Height)
                return;

            Program.MovePiece(this, X, Y);
            x = X;
            y = Y;
        }

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                SetXY(value, y);
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
                SetXY(x, value);
            }
        }

        public void Draw(ConsoleColor backColor)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = foreColor;
            if (backGround)
                Console.BackgroundColor = this.backColor;
            else
                Console.BackgroundColor = backColor;
            Console.Write(character);
        }

        public abstract void Move();

        internal void MovePiece()
        {
            Move();

            foreach (Piece p in Program.rand.Iterate(Program.getPieces(X, Y)))
            {
                Collision(this, p);
                Collision(p, this);
            }
        }

        private void Collision(Piece p1, Piece p2)
        {
            if (p1 is Player)
            {
                Player player = ( (Player)p1 );
                if (p2 is Squirrel)
                    player.GetSquirrel((Squirrel)p2);
                else if (p2 is SquirrelEater)
                    player.HitEater((SquirrelEater)p2);
            }
            else if (p1 is ThrownSquirrel)
            {
                ThrownSquirrel ts = ( (ThrownSquirrel)p1 );
                if (p2 is Squirrel)
                    ts.HitSquirrel((Squirrel)p2);
                else if (p2 is SquirrelEater)
                    ts.HitEater((SquirrelEater)p2);
            }
            else if (p1 is SquirrelEater)
            {
                SquirrelEater se = ( (SquirrelEater)p1 );
                if (p2 is Squirrel)
                    se.Eat((Squirrel)p2);
            }
        }
    }
}