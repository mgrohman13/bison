using game2.game;

namespace game2.runes
{
    public interface IChoiceHandler
    {
        (bool play, int choiceIdx) SelectResource(List<Resources> choices);
    }
}
