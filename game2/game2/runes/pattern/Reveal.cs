using game2.sides;

namespace game2.runes.pattern
{
    internal class Reveal : IRunePattern<Reveal>
    {
        public static Reveal NewPattern(Player player, int researchLevel, float runeValue, int? forceCharges = null)
        {
            throw new NotImplementedException();
        }
        RuneShape IRunePattern.NewShape()
        {
            throw new NotImplementedException();
        }

        public bool CanPlay(Rune rune)
        {
            throw new NotImplementedException();
        }
        void IRunePattern.Play(Rune rune, object? target)
        {
            throw new NotImplementedException();
        }
        (bool play, object target) IRunePattern.HandleChoice(IChoiceHandler handler)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<IRuneEffect>? GetEffects()
        {
            throw new NotImplementedException();
        }
    }
}
