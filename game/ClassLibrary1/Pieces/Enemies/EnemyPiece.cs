﻿using MattUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using Tile = ClassLibrary1.Map.Map.Tile;

namespace ClassLibrary1.Pieces.Enemies
{
    [Serializable]
    public abstract class EnemyPiece : Piece
    {
        protected AIState _state;
        public AIState State => _state;// private - hide behind research

        internal EnemyPiece(Tile tile, AIState state)
            : base(tile.Map.Game.Enemy, tile)
        {
            this._state = state;
        }

        internal virtual AIState TurnState(double difficulty, Dictionary<Tile, double> playerAttacks, HashSet<Tile> moveTiles, HashSet<IKillable> killables,
            out List<Point> path)
        {
            path = null;
            return _state;
        }

        internal override void Die()
        {
            foreach (EnemyPiece piece in Side.Pieces.OfType<EnemyPiece>())
                piece.OnDeath(this);
            //Game.CountKill();
        }

        protected virtual void OnDeath(EnemyPiece enemyPiece)
        {
        }

        public enum AIState// internal protected
        {
            Retreat,
            Fight,
            Patrol,
            Rush,
        }
    }
}
