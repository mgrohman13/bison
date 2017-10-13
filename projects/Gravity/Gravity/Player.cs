﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Player : Piece
    {
        private float shield;

        public Player(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, System.Drawing.Color.Blue)
        {
            this.shield = 2.5f;
        }

        public float Shield
        {
            get
            {
                return shield;
            }
        }

        internal override float GetGravity(Type type)
        {
            if (type == typeof(Center))
                return 1;
            if (type == typeof(Enemy))
                return 1 / 4f;
            if (type == typeof(Player))
                return 1;
            if (type == typeof(PowerUp))
                return 1;
            if (type == typeof(Target))
                return 4;
            throw new Exception();
        }

        internal override void Interact(Piece piece)
        {
            base.Interact(piece);

            if (( x - piece.X ) * ( x - piece.X ) + ( y - piece.Y ) * ( y - piece.Y ) < ( ( this.size + piece.Size ) / 2f ) * ( ( this.size + piece.Size ) / 2f ))
                if (piece is PowerUp)
                {
                    game.AddScore(piece.Size / Game.avgSize);
                    game.Remove(piece);
                }
                else if (piece is Enemy)
                {
                    shield -= ( (Enemy)piece ).GetDmg();
                    game.Remove(piece);
                    game.NewEnemy();
                }
        }

        internal override void Step(float count)
        {
            base.Step(count);

            const float decay = .9999f;
            this.xDir *= decay;
            this.yDir *= decay;

            //double distSqr =;
            //double gameSize =;
            //gameSize *= gameSize;
            //if (distSqr > gameSize)
            //    shield -= (float)Game.rand.OE(shield * .000025 * Math.Pow(distSqr / gameSize, .2));
            //else
            //    shield += (float)Game.rand.OE(Math.Sqrt(game.Difficulty) * .002f / ( shield + 1 ));

            const float gameSizeSqr = Game.gameSize * Game.gameSize;
            float val = (float)Math.Pow(( ( x * x + y * y ) * 4f + gameSizeSqr ) / ( gameSizeSqr * 3f ), .25f) - 1f;
            float cur = (float)Math.Sqrt(shield + 1f);
            val = Game.rand.OE((float)( .005f * Math.Log(Math.Abs(val) + 1f) * ( val < 0 ? Math.Sqrt(game.Difficulty) / cur : -cur / 25f ) ));
            Console.WriteLine(val);
            shield += val;

            if (shield >= 0)
                this.color = GetShieldColor();

            this.size = (float)( 25f * Math.Pow(shield / 3f, .3f) );
        }

        public System.Drawing.Color GetShieldColor()
        {
            int pct = Game.rand.Round(255f * GetShieldPct());
            return System.Drawing.Color.FromArgb(0, pct, 255);
        }

        public float GetShieldPct()
        {
            return GetShieldPct(shield);
        }

        public static float GetShieldPct(float shield)
        {
            return (float)Math.Pow(shield / ( shield + 5f ), .65);
        }

        internal bool CheckCourse(float x2, float y2, float diameter)
        {
            int iter = Game.Random.Round(500 / ( 1000f / 39f )) + 1;
            float x1 = this.x - size / 2f, y1 = this.y - size / 2f, d1 = this.size, d2 = diameter, e1 = 0, e2 = 0;
            for (int a = 0 ; a < iter ; ++a)
            {
                float f1 = ( d1 - this.size ) / 2f;
                float f2 = ( d2 - diameter ) / 2f;
                if (CheckCourse(x1 - f1, y1 - f1, d1, x2 - f2, y2 - f2, d2))
                    return true;
                x1 += xDir;
                y1 += yDir;
                e1 += Game.gravity;
                e2 += Game.gravity;
                d1 += e1;
                d2 += e2;
            }
            return false;
        }
        private static bool CheckCourse(float x1, float y1, float d1, float x2, float y2, float d2)
        {
            float radius = d1 / 2f;
            float pieceRadius = d2 / 2f;
            float diff = radius - pieceRadius;
            float x = x1 - x2 + diff;
            float y = y1 - y2 + diff;
            diff = radius + pieceRadius;
            return ( diff * diff > x * x + y * y );
        }
    }
}
