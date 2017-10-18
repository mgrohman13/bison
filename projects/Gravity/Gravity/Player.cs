using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Player : Piece
    {
        private const int shieldTime = 1500;

        private float shield;

        private float shieldPrev;
        private int shieldTick;

        public Player(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, System.Drawing.Color.Blue)
        {
            this.shield = 2.5f;

            ResetShieldTick();
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
            if (typeof(Center).IsAssignableFrom(type))
                return 1;
            if (typeof(Enemy).IsAssignableFrom(type))
                return 1 / 4f;
            if (typeof(Player).IsAssignableFrom(type))
                return 1;
            if (typeof(PowerUp).IsAssignableFrom(type))
                return 1;
            if (typeof(Target).IsAssignableFrom(type))
                return 4;
            throw new Exception();
        }

        internal override void Interact(Piece piece)
        {
            base.Interact(piece);

            if (( x - piece.X ) * ( x - piece.X ) + ( y - piece.Y ) * ( y - piece.Y ) < ( ( this.size + piece.Size ) / 2f ) * ( ( this.size + piece.Size ) / 2f ))
                if (piece is PowerUp)
                {
                    game.AddScore(piece.Size / Game.avgSize, piece);
                    game.Remove(piece);
                }
                else if (piece is Enemy)
                {
                    float dmg = ( (Enemy)piece ).GetDmg();
                    shield -= dmg;
                    game.Remove(piece);
                    game.NewEnemy();

                    ShowDamage(piece, dmg);
                }
        }

        private void ShowDamage(Piece piece, float dmg)
        {
            game.AddText(piece, Game.rand.Round(dmg * -100).ToString(), dmg, true, false, false);
            this.shieldPrev -= dmg;
        }
        private void ShowShield()
        {
            if (--shieldTick < 0)
            {
                float diff = this.shield - this.shieldPrev;
                bool pos = ( diff > 0 );
                game.AddText(this, ( pos ? "+" : "" ) + Game.rand.Round(diff * 100).ToString(), diff, !pos, false, pos);
                ResetShieldTick();
            }
        }
        private void ResetShieldTick()
        {
            this.shieldPrev = shield;
            this.shieldTick = shieldTime;
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


            if (shield >= 0)
            {
                const float gameSizeSqr = Game.gameSize * Game.gameSize;
                float val = (float)Math.Pow(( ( x * x + y * y ) * 4f + gameSizeSqr ) / ( gameSizeSqr * 3f ), .25f) - 1f;
                float cur = (float)Math.Sqrt(shield + 1f);
                val = (float)( .005f * Math.Log(Math.Abs(val) + 1f) * ( val < 0 ? Math.Sqrt(game.Difficulty) / cur : -cur / 25f ) );
                if (Game.rand.Bool(25 / 1000f))
                    Console.WriteLine(string.Format("{0:0.000}\t\t{1:+#.00E+0;-#.00E+0}", shield, val));
                val = Game.rand.OE(val);
                shield += val;
                if (shield >= 0)
                    this.color = GetShieldColor();

                ShowShield();
            }

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
