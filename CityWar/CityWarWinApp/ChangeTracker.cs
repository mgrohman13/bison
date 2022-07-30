using CityWar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityWarWinApp
{
    class ChangeTracker
    {
        Dictionary<Tile, int> relics = new();
        internal void StartBattle()
        {
            relics = GetRelics();
        }
        internal IEnumerable<Tile> EndBattle()
        {
            var after = GetRelics();
            foreach (Tile t in after.Keys.ToArray())
            {
                relics.TryGetValue(t, out int a);
                if (after[t] <= a)
                    after.Remove(t);
            }
            relics = new();
            return after.Keys;
        }
        private Dictionary<Tile, int> GetRelics()
        {
            return Map.Game.GetPlayers().SelectMany(p => p.GetPieces()).OfType<Relic>().GroupBy(r => r.Tile).ToDictionary(g => g.Key, g => g.Count());
        }

        Dictionary<Tile, Tileinfo> tileInfo = new();
        private class Tileinfo
        {
            Terrain terrain;
            bool wizPts, cityTime;
            HashSet<Piece> pieces;
            public Tileinfo(Tile tile)
            {
                this.terrain = tile.Terrain;
                this.wizPts = tile.WizardPoints > -1;
                this.cityTime = tile.CityTime > -1;
                this.pieces = tile.GetAllPieces().ToHashSet();
            }
            public bool Equals(Tileinfo other)
            {
                return terrain == other.terrain && wizPts == other.wizPts && cityTime == other.cityTime && pieces.SetEquals(other.pieces);
            }
        }
        internal void StartNextTurn()
        {
            tileInfo = GetTileInfo();
        }
        internal IEnumerable<Tile> EndNextTurn()
        {
            var after = GetTileInfo();
            foreach (Tile t in after.Keys.ToArray())
                if (after[t].Equals(tileInfo[t]))
                    after.Remove(t);
            tileInfo = new();
            return after.Keys;
        }
        private Dictionary<Tile, Tileinfo> GetTileInfo()
        {
            Dictionary<Tile, Tileinfo> r = new();
            for (int x = 0; x < Map.Game.Diameter; x++)
                for (int y = 0; y < Map.Game.Diameter; y++)
                {
                    Tile t = Map.Game.GetTile(x, y);
                    if (t != null)
                        r.Add(t, new(t));
                }
            return r;
        }

        HashSet<Tile> wizPts = new();
        internal void StartMove()
        {
            wizPts = GetWizPts();
        }
        internal IEnumerable<Tile> EndMove()
        {
            var after = GetWizPts();
            after.ExceptWith(wizPts);
            wizPts = new();
            return after;
        }
        private HashSet<Tile> GetWizPts()
        {
            HashSet<Tile> r = new();
            for (int x = 0; x < Map.Game.Diameter; x++)
                for (int y = 0; y < Map.Game.Diameter; y++)
                {
                    Tile t = Map.Game.GetTile(x, y);
                    if (t != null && t.WizardPoints > -1)
                        r.Add(t);
                }
            return r;
        }
    }
}
