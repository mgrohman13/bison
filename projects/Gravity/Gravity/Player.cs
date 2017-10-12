using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gravity
{
    class Player : Piece
    {
        public Player(Game game, float x, float y, float size, float density) : base(game, x, y, size, density, System.Drawing.Color.Blue)
        {
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

            if ((x - piece.X) * (x - piece.X) + (y - piece.Y) * (y - piece.Y) < ((this.size + piece.Size) / 2f) * ((this.size + piece.Size) / 2f))
                if (piece is PowerUp)
                {
                    game.AddScore(piece.Mass / getMass(Game.scoreSize, piece.Density));
                    game.Remove(piece);
                }
                else if (piece is Enemy)
                {
                    game.AddScore(-piece.Mass / getMass(Game.avgSize, 1));
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
        }
    }
}
