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
    public class Enemy : ISide
    {
        private readonly ISide side;

        internal Enemy(Game game)
        {
            this.side = new Side(game);
        }

        #region ISide

        public Game Game => side.Game;
        IReadOnlyCollection<Piece> ISide.Pieces => side.Pieces;
        internal IReadOnlyCollection<Piece> Pieces => side.Pieces;

        void ISide.AddPiece(Piece piece)
        {
            AddPiece(piece);
        }
        internal void AddPiece(Piece piece)
        {
            side.AddPiece(piece);
        }
        void ISide.RemovePiece(Piece piece)
        {
            RemovePiece(piece);
        }
        internal void RemovePiece(Piece piece)
        {
            side.RemovePiece(piece);
        }

        internal void PlayTurn()
        {
            foreach (Piece piece in Game.Rand.Iterate(Pieces))
                PlayTurn(piece);

            this.EndTurn();
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

        void ISide.EndTurn()
        {
            EndTurn();
        }
        internal void EndTurn()
        {
            side.EndTurn();
        }

        #endregion ISide
    }
}
