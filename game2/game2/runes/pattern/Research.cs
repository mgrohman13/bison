using game2.game;
using game2.sides;

namespace game2.runes.pattern
{
    internal class Research : IRunePattern<Research>
    {
        public static Research NewPattern(Player player, int researchLevel, float runeValue, int? forceCharges = null)
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
        public void Play(Rune rune, object? _ = null)
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
