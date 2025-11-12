using game2.game;
using game2.map;
using game2.pieces;
using game2.pieces.behavior.attributes;
using game2.pieces.player;
using game2.runes;

namespace game2.sides
{
    public class Player(Game game) : Side(game)
    {
#pragma warning disable CS8618 // Assuming StartGame is called before accessing  
        private List<RuneShape> _deck;
        private List<Rune> _hand;

        public Core Core { get; private set; }
#pragma warning restore CS8618

        public Resources Resources => _resources;

        public IReadOnlyList<RuneShape> Deck => _deck.AsReadOnly();
        public IReadOnlyList<Rune> Hand => _hand.AsReadOnly();

        internal void StartGame()
        {
            Tile start = Game.Map.GetRandomTile(new(0, 0), t => t.Terrain is map.Terrain.Plains);
            Core = Core.NewCore(start);

            _deck = RuneShape.GenStartDeck();
            _hand = GenStartHand();
        }
        private List<Rune> GenStartHand()
        {
            //throw new NotImplementedException();
            return null;
        }

        internal void DrawRunes(int draw)
        {
            throw new NotImplementedException();
        }
        internal void DiscardRunes(int discard)
        {
            throw new NotImplementedException();
        }

        public Resources GetTurnEnd()
        {
            Resources r = new();
            foreach (Piece p in Game.Rand.Iterate(Pieces))
                r += p.GetTurnEnd();

            //hack...
            foreach (var check in Game.Map.Pieces.OfType<Resource>())
            {
                int harvesters = check.Tile.GetNeighbors().Count(t =>
                    t.Piece is not null && t.Piece.Side == this && t.Piece.HasBehavior<Harvester>());
                if (harvesters > 1)
                    r -= check.GetGenerate() * (harvesters - 1);
            }

            return r;
        }
        internal void EndTurn()
        {
            Resources resources = new();
            foreach (Piece p in Game.Rand.Iterate(Pieces))
                p.EndTurn(ref resources);
            this._resources += resources;
        }

        internal void StartTurn()
        {
            foreach (Piece p in Game.Rand.Iterate(Pieces))
                p.StartTurn();
        }

        public bool HasResources(Resources resources) => this.Resources >= resources;

        internal void SpendResources(Resources resources) => this._resources -= resources;
    }
}
