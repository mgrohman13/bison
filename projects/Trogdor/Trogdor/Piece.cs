using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Trogdor
{
    public enum Type
    {
        Hut,
        Ally,
        Enemy,
        Player
    }

    public class Piece
    {
        private readonly Type type;
        private double x, y, xVel, yVel, size;

        public double Size
        {
            get
            {
                return size;
            }
            private set
            {
                //Game.Invalidate(x, y, Math.Max(this.size, value));
                double old = Diameter;
                size = value;
                old -= this.Diameter;
                old /= 2.0;
                this.x += old;
                this.y += old;
            }
        }

        public double Diameter
        {
            get
            {
                return 2.0 * Math.Sqrt(Size / Math.PI);
            }
        }

        public Type Type
        {
            get
            {
                return type;
            }
        }

        internal Piece(Type type, double size)
        {
            this.type = type;
            this.size = size;
            double diameter = Diameter;
            this.x = Game.Random.DoubleHalf(Game.Width - diameter);
            this.y = Game.Random.DoubleHalf(Game.Height - diameter);
            xVel = 0;
            yVel = 0;

            switch (type)
            {
            case Type.Ally:
                Game.TotalAlly += size;
                break;
            case Type.Enemy:
                Game.TotalEnemy += size;
                break;
            case Type.Hut:
                Game.TotalHut += size;
                break;
            }

            Game.Pieces.Add(this);
        }

        private void CreateChild()
        {
            if (Game.Random.Bool(Game.CreateOther))
            {
                Piece child = new Piece(Type.Ally, Game.Random.DoubleHalf(this.Size));
                double diff = ( this.Diameter - child.Diameter ) / 2.0;
                child.x = this.x + diff;
                child.y = this.y + diff;
            }
        }

        private void Accelerate(double xVel, double yVel)
        {
            this.xVel += xVel;
            this.yVel += yVel;
        }

        private void Drift(float drift)
        {
            if (Game.Random.Bool(Game.DriftChance))
                Accelerate(Game.Random.Gaussian(drift), Game.Random.Gaussian(drift));
        }

        internal void Increment()
        {
            if (this.Size > 0 && Game.Random.Bool(Game.DeathChance))
            {
                double decay = Game.Random.OE(Game.DeathConst + this.Size / Game.HutSize);
                if (decay > this.Size)
                    decay = this.size;
                switch (this.Type)
                {
                case Type.Ally:
                    Game.DecayAlly += decay;
                    break;
                case Type.Enemy:
                    Game.DecayEnemy += decay;
                    break;
                case Type.Hut:
                    Game.DecayHut += decay;
                    break;
                case Type.Player:
                    Game.DecayPlayer += decay;
                    break;
                }
                this.Size -= decay;
            }

            if (this.Size > 0)
            {
                switch (this.Type)
                {
                case Type.Ally:
                case Type.Enemy:
                    Drift(Game.OtherDrift);

                    double offset = this.Diameter / 2.0;
                    Accelerate(Game.Random.DoubleFull(( Game.Width / 2.0 - this.x - offset ) * Game.OtherSpeed),
                            Game.Random.DoubleFull(( Game.Height / 2.0 - this.y - offset ) * Game.OtherSpeed));
                    break;

                case Type.Player:
                    Drift(Game.PlayerDrift);

                    if (Game.Down)
                        Accelerate(0, Game.PlayerSpeed);
                    if (Game.Up)
                        Accelerate(0, -Game.PlayerSpeed);
                    if (Game.Left)
                        Accelerate(-Game.PlayerSpeed, 0);
                    if (Game.Right)
                        Accelerate(Game.PlayerSpeed, 0);
                    break;
                }

                this.x += xVel;
                this.y += yVel;

                CheckSides();
            }

            if (this.Size > 0)
            {
                if (type == Type.Hut)
                    CreateChild();
            }
            else
            {
                if (this.type == Type.Player)
                    Game.EndGame();
                Game.Pieces.Remove(this);
            }
        }

        private void CheckSides()
        {
            bool hit = false;

            double offset = this.Diameter;
            double maxWidth = Game.Width - offset;
            double maxHeight = Game.Height - offset;
            if (x > maxWidth)
            {
                x = maxWidth;
                hit = true;
            }
            else if (x < 0)
            {
                x = 0;
                hit = true;
            }
            if (y > maxHeight)
            {
                y = maxHeight;
                hit = true;
            }
            else if (y < 0)
            {
                y = 0;
                hit = true;
            }

            if (hit)
            {
                if (this.type == Type.Player)
                    this.Size -= Math.Sqrt(xVel * xVel + yVel * yVel) * Game.HitDamage;

                yVel = 0;
                xVel = 0;
            }
        }

        private bool Hits(Piece piece)
        {
            double radius = this.Diameter / 2.0;
            double pieceRadius = piece.Diameter / 2.0;
            double diff = radius - pieceRadius;
            double x = this.x - piece.x + diff;
            double y = this.y - piece.y + diff;
            diff = radius + pieceRadius;
            return ( diff * diff > x * x + y * y );
        }

        internal void CheckCollisions()
        {
            foreach (Piece piece in Game.Random.Iterate(Game.Pieces))
                if (this.Hits(piece))
                {
                    switch (piece.type)
                    {
                    case Type.Ally:
                        double ad = piece.Size;
                        Game.TotalPlayer += ad;
                        Game.CollectAlly += ad;

                        this.Size += ad;
                        piece.Size = 0;
                        break;

                    case Type.Enemy:
                        double ed = Math.Min(this.Size, piece.Size);
                        Game.CollectEnemy += ed;

                        this.Size -= ed;
                        piece.Size -= ed;
                        break;

                    case Type.Hut:
                        double hd = Math.Min(this.Size, piece.Size);
                        Game.CollectHut += hd;

                        Game.Score += hd;
                        this.Size -= hd;
                        piece.Size -= hd;
                        break;
                    }
                }
        }

        public void Draw(Graphics graphics, int yOffset)
        {
            float diameter = (float)Diameter;
            graphics.FillEllipse(GetBrush(type), (float)x, (float)y + yOffset, diameter, diameter);
        }
        public void DrawBorder(Graphics graphics, int yOffset)
        {
            float diameter = (float)Diameter;
            graphics.DrawEllipse(Pens.Gray, (float)x, (float)y + yOffset, diameter, diameter);
        }

        public static Brush GetBrush(Type type)
        {
            switch (type)
            {
            case Type.Ally:
                return Brushes.Green;
            case Type.Enemy:
                return Brushes.Red;
            case Type.Hut:
                return Brushes.Black;
            case Type.Player:
                return Brushes.Blue;
            }
            throw new Exception();
        }
    }
}
