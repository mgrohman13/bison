using game2.game;
using game2.map;
using game2.sides;

namespace game2.runes
{
    public class Rune
    {
        public readonly RuneShape Shape;
        public readonly Resources Resources;
        public readonly int Draw, Discard;

        private readonly Player _player;
        private readonly List<IRuneEffect> _effects;

        private int _charges;

        public int Charges => _charges;
        public IReadOnlyList<IRuneEffect> Effects => _effects.AsReadOnly();

        internal Rune(Player player, RuneShape shape, int charges = 1, Resources resources = new(),
            IEnumerable<IRuneEffect>? effects = null, int draw = 0, int discard = 0)
        {
            Shape = shape;
            Resources = resources;
            Draw = draw;
            Discard = discard;

            _player = player;
            _effects = [.. effects ?? []];

            _charges = charges;
        }
        internal static Rune DrawRune(RuneShape shape)
        {
            Rune rune = shape.DrawRune();
            return rune;
        }

        public bool CanPlay() =>
            _player.HasResources(Resources) && Shape.Pattern.CanPlay(this);
        public Tile? Play(IChoiceHandler handler)
        {
            Tile? result = null;
            if (CanPlay())
            {
                (bool, Tile?) played = Shape.Pattern.HandleChoice(handler);
                if (played.Item1)
                {
                    result ??= PlayRune(played.Item2);
                    result ??= played.Item2;
                }
            }
            return result;
        }

        public object[,] GetInfo()
        {
            throw new NotImplementedException();
        }

        //public Choice? Play(Tile? tile)
        //{
        //    Choice? choice = Shape.Pattern.Play(this, tile);
        //    if (choice != null)
        //        PlayRune();
        //    return choice;
        //}
        //internal void Accept(Choice choice)
        //{
        //    PlayRune();
        //}

        private Tile? PlayRune(Tile? tile)
        {
            _player.SpendResources(Resources);
            _charges--;

            tile = Shape.Pattern.Play(this, tile);
            foreach (IRuneEffect effect in Game.Rand.Iterate(_effects))
                effect.PlayRune(this);

            if (Draw > 0)
                _player.DrawRunes(Draw);
            if (Discard > 0)
                _player.DiscardRunes(Discard);

            return tile;
        }
    }
}
