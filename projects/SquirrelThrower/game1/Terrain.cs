using System;
using System.Collections.Generic;
using System.Text;

namespace game1
{
    abstract class Terrain
    {
        public Terrain(ConsoleColor color, int X, int Y)
        {
            this.color = color;
            x = X;
            y = Y;

            this.foreGround = false;
        }

        public Terrain(ConsoleColor color, int X, int Y, char character, ConsoleColor foreColor)
        {
            this.color = color;
            x = X;
            y = Y;

            this.foreGround = true;
            this.character = character;
            this.foreColor = foreColor;
        }

        bool visible = false;

        protected char character;
        protected bool foreGround;
        readonly int x, y;
        protected ConsoleColor color, foreColor;

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                    Program.ForceInvalidate(x, y);
                visible = value;
            }
        }

        public char Character
        {
            get
            {
                return character;
            }
        }

        public bool ForeGround
        {
            get
            {
                return foreGround;
            }
        }

        public ConsoleColor Color
        {
            get
            {
                return color;
            }
        }

        public ConsoleColor ForeColor
        {
            get
            {
                return foreColor;
            }
        }
    }
}