
using game2.game;
using game2.runes.pattern;
using game2.sides;

namespace game2.runes
{
    public class RuneShape
    {
        public readonly Player Player;

        public readonly IRunePattern Pattern;

        private Resources _resources;
        public readonly (int min, int max) Charges, Draw, Discard;

        public Resources Resources => _resources;

        internal RuneShape(Player player, IRunePattern pattern, Resources resources,
            (int min, int max)? charges = null, (int min, int max)? draw = null, (int min, int max)? discard = null)
        {
            (int, int) zero = (0, 0);
            (int, int) one = (1, 1);

            this.Player = player;
            this.Pattern = pattern;
            this._resources = resources;

            this.Charges = charges ?? one;
            this.Draw = draw ?? zero;
            this.Discard = discard ?? zero;
        }
        internal static RuneShape NewRuneShape(IRunePattern pattern) => pattern.NewShape();

        internal Rune DrawRune() =>
            new(Player, this, GenCharges(), _resources, Pattern.GetEffects(), GenDraw(), GenDiscard());

        internal int GenCharges() => Generate(Charges);
        internal int GenDraw() => Generate(Draw);
        internal int GenDiscard() => Generate(Discard);
        internal static int Generate((int min, int max) range) => Game.Rand.RangeInt(range.min, range.max);
    }
}
