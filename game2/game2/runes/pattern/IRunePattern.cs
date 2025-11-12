using game2.map;

namespace game2.runes.pattern
{
    internal interface IRunePattern
    {
        RuneShape NewShape();
        bool CanPlay(Rune rune);
        (bool, Tile?) HandleChoice(IChoiceHandler handler);
        Tile? Play(Rune rune, Tile? tile);
    }
}
