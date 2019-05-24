using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MattUtil;

namespace GalWar
{
    [Serializable]
    public class Wormhole
    {
        public readonly Game Game;

        public readonly int CreatedTurn;
        private PointS[] _points;

        internal Wormhole(IEnumerable<Tile> points, int createdTurn)
        {
            checked
            {
                this.Game = points.First().Game;
                this.CreatedTurn = createdTurn;
                this._points = Game.Random.Iterate(points).Select(t => Game.GetPointS(t)).ToArray();
            }
        }

        public IEnumerable<Tile> Tiles
        {
            get
            {
                checked
                {
                    return this._points.Select(point => Game.GetTile(point.X, point.Y));
                }
            }
        }

        internal bool Remove()
        {
            checked
            {
                int count = this._points.Length;
                if (count <= 2)
                    return true;

                var temp = new List<PointS>(this._points);
                temp.RemoveAt(Game.Random.Next(count));
                this._points = temp.ToArray();

                return false;
            }
        }
    }
}
