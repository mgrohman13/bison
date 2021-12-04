using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MattUtil;
using ClassLibrary1.Pieces;
using ClassLibrary1.Pieces.Enemies;
using ClassLibrary1.Pieces.Players;
using Tile = ClassLibrary1.Map.Tile;

namespace ClassLibrary1
{
    [Serializable]
    public class Enemy : Side
    {
        internal IReadOnlyCollection<Piece> Pieces => _pieces;
        public IEnumerable<Piece> VisiblePieces => _pieces.Where(p => p.Tile.Visible);

        internal Enemy(Game game)
            : base(game, -Consts.EnemyEnergy, 0)
        {
        }

        internal void PlayTurn(double difficulty, Func<MechBlueprint> Blueprint)
        {
            foreach (Piece piece in Game.Rand.Iterate(Pieces))
                PlayTurn(piece);

            double e, m;
            e = m = 0;
            foreach (Piece piece in Game.Rand.Iterate(Pieces))
                piece.GetUpkeep(ref e, ref m);
            this._energy -= e + m;

            this.EndTurn();

            this._energy += this.Mass + difficulty * Consts.EnemyEnergy;
            this._mass = 0;
            bool flag = false;
            while (true)
            {
                MechBlueprint blueprint = Blueprint();
                Mech.Cost(out double energy, out double mass, blueprint, difficulty);
                energy += mass;
                if (this.Energy > energy)
                {
                    if (flag)
                    { }
                    flag = true;
                    this._energy -= energy;
                    Alien.NewAlien(Game.Map.GetEnemyTile(), blueprint.Killable, blueprint.Attacks, blueprint.Movable);
                }
                else break;
            }
        }
        private void PlayTurn(Piece piece)
        {
            IMovable movable = piece as IMovable;
            IEnumerable<Tile> moveTiles = new Tile[] { piece.Tile };
            if (movable != null)
                moveTiles = piece.Tile.GetTilesInRange(movable.MoveCur).Where(t => t.Piece == null);
            IEnumerable<Tile> attackTiles = Enumerable.Empty<Tile>();
            if (piece is IAttacker attacker)
            {
                foreach (double range in attacker.Attacks.Select(a => a.Range).OrderBy(r => r))
                    foreach (Tile tile in Game.Rand.Iterate(moveTiles))
                    {
                        IEnumerable<Tile> targets = tile.GetTilesInRange(range).Where(t => t.Piece is IKillable && t.Piece.IsPlayer);
                        if (targets.Any())
                        {
                            if (movable != null)
                                movable.EnemyMove(tile);
                            attacker.EnemyFire((IKillable)Game.Rand.SelectValue(targets).Piece);
                            return;
                        }
                    }
            }
            if (movable != null && moveTiles.Any())
                movable.EnemyMove(Game.Rand.SelectValue(moveTiles, t =>
                {
                    double dist = t.GetDistance(Game.Player.Core.Tile);
                    return Game.Rand.Round(ushort.MaxValue / dist / dist);
                }));
        }
    }
}
