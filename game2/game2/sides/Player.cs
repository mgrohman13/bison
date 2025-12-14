using game2.game;
using game2.map;
using game2.pieces;
using game2.pieces.behavior.attributes;
using game2.pieces.player;
using game2.runes;
using game2.runes.pattern;

namespace game2.sides
{
    public class Player(Game game) : Side(game)
    {
        private readonly List<RuneShape> _deck = new();
        private readonly List<Rune> _hand = new();

        public Core Core { get; private set; }

        public Resources Resources => _resources;

        public IReadOnlyList<RuneShape> Deck => _deck.AsReadOnly();
        public IReadOnlyList<Rune> Hand => _hand.AsReadOnly();

        internal void StartGame()
        {
            Tile start = Game.Map.GetRandomTile(new(0, 0), t => t.Terrain is map.Terrain.Plains);
            Core = Core.NewCore(start);

            _deck.AddRange(GenStartDeck());
            _hand.AddRange(GenStartHand());
        }
        private List<RuneShape> GenStartDeck()
        {
            return [NewPattern<Trade>(1).NewShape()];

            Pattern NewPattern<Pattern>(int charges) where Pattern : IRunePattern<Pattern> => Pattern.NewPattern(this, Resources.Research, RuneValue(), charges);

            Func<int, IRunePattern>[] patterns =
            [
                c => NewPattern<BuildUnit>(c),
                c => NewPattern<Draw>(c),
                c => NewPattern<Reveal>(c),
                c => NewPattern<Trade>(c),
                c => NewPattern<Research>(c),
            ];

            int startResearch = 100;//TODO: randomize Game.Consts.StartingLevel();

            List<RuneShape> deck = [];
            bool chargeAlt = Game.Rand.Bool();
            foreach (int a in Game.Rand.Iterate(patterns.Length))
            {
                _resources.Research += Game.Rand.Round(startResearch / (float)patterns.Length);

                int charges = chargeAlt ^ ((a & 1) == 0) ? 1 : 2;
                if (a < 2)
                    charges++;
                IRunePattern pattern = patterns[a](charges);

                deck.Add(pattern.NewShape());
            }

            _resources.Research = 0;
            return deck;
        }
        private List<Rune> GenStartHand()
        {
            List<Rune> hand = [];

            foreach (var shape in Game.Rand.Iterate(Deck.Where(s => s.Pattern is BuildUnit || s.Pattern is Draw)))
                hand.Add(shape.DrawRune());
            DrawRunes(1);

            return hand;
        }

        private float RuneValue()
        {
            float researchMult = Game.Consts.GetResearchMult(Resources.Research);
            float value = 10f * researchMult;//TODO: randomize
            return value;
        }

        internal void DrawRunes(int draw)
        {
            //TODO: weighted draw
            for (int a = 0; a < draw; a++)
                Game.Rand.SelectValue(Deck).DrawRune();
        }
        internal void DiscardRunes(List<Rune> discard)
        {
            foreach (var rune in Game.Rand.Iterate(discard))
                if (!_hand.Remove(rune))
                    throw new Exception();
        }

        public Resources GetTurnEnd()
        {
            Resources r = new();
            foreach (Piece p in Game.Rand.Iterate(Pieces))
                r += p.GetTurnEnd();

            //hack...
            foreach (var check in Game.Rand.Iterate(Game.Map.Pieces.OfType<Resource>()))
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
