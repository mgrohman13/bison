using game2.sides;

namespace game2.runes.pattern
{
    internal interface IRunePattern<Pattern> : IRunePattern where Pattern : IRunePattern<Pattern>
    {
        static abstract Pattern NewPattern(Player player, int researchLevel, float runeValue, int? forceCharges = null);
    }

    public interface IRunePattern
    {
        internal RuneShape NewShape();

        internal bool CanPlay(Rune rune);
        internal void Play(Rune rune, object? target);
        internal (bool play, object target) HandleChoice(IChoiceHandler handler);
        IEnumerable<IRuneEffect>? GetEffects();
    }
}
