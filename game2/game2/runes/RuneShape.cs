
using game2.game;
using game2.runes.pattern;

namespace game2.runes
{
    public class RuneShape
    {
        internal readonly IRunePattern Pattern;

        public readonly (int, int) Charges, Draw, Discard;

        internal RuneShape(IRunePattern pattern, (int, int)? charges = null, (int, int)? draw = null, (int, int)? discard = null)
        {
            (int, int) zero = (0, 0);
            (int, int) one = (1, 1);

            this.Pattern = pattern;
            this.Charges = charges ?? one;
            this.Draw = draw ?? zero;
            this.Discard = discard ?? zero;
        }
        internal static RuneShape NewRuneShape(IRunePattern pattern)
        {
            RuneShape shape = pattern.NewShape();
            return shape;
        }

        internal Rune DrawRune()
        {
            throw new NotImplementedException();
        }

        internal int GenCharges() => Generate(Charges);
        internal int GenDraw() => Generate(Draw);
        internal int GenDiscard() => Generate(Discard);
        internal static int Generate((int, int) range) => Game.Rand.RangeInt(range.Item1, range.Item2);

        internal static List<RuneShape> GenStartDeck()
        {
            //throw new NotImplementedException();
            return null;
        }
    }
}
