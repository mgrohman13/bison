using Type = ClassLibrary1.Research.Type;

namespace ClassLibrary1
{
    interface IResearch
    {
        Game Game { get; }
        int GetBlueprintLevel();
        Type GetType();
        int GetMinCost();
        int GetMaxCost();
        bool HasType(Type research);
        bool MakeType(Type type);
        double GetMult(Type type, double pow);
    }
}
