using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    class Squirrel : Piece
    {
        double move, speed;

        public Squirrel()
            : base(',', ConsoleColor.DarkRed)
        {
            InitStuff();
        }

        public Squirrel(int x, int y)
            : base(x, y, ',', ConsoleColor.DarkRed)
        {
            InitStuff();
        }

        private void InitStuff()
        {
            move = 5.0 + Program.rand.OE(169);
            speed = 666.0 / ( 2.6 + Program.rand.OE(26) );
        }

        public override void Move()
        {
            const double staticAdd = 2.1;

            if (--move < 4)
                move = 4;

            switch (Program.rand.Next(Program.rand.Round(move)))
            {
            case 0:
                X++;
                move += staticAdd + Program.rand.OE(speed);
                break;

            case 1:
                X--;
                move += staticAdd + Program.rand.OE(speed);
                break;

            case 2:
                Y++;
                move += staticAdd + Program.rand.OE(speed);
                break;

            case 3:
                Y--;
                move += staticAdd + Program.rand.OE(speed);
                break;

            case 4:
                move += staticAdd;
                break;
            }

            bool grass = ( Program.getTerrain(X, Y) is Grass );
            const double chance = 0.0052;
            if (Program.rand.Bool(grass ? chance : chance * chance))
            {
                Program.AddPiece(new Squirrel(X, Y));
                if (grass)
                    Program.setTerrain(X, Y, new Dirt(X, Y, Program.getTerrain(X, Y).Visible));
            }
        }
    }
}