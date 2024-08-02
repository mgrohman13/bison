namespace ClassLibrary1
{
    internal class ResearchMinMaxCost : IResearch
    {
        private readonly IResearch parent;
        private readonly int min, max;
        public ResearchMinMaxCost(IResearch parent, int min, int max)
        {
            this.parent = parent;
            this.min = min;
            this.max = max;
        }

        public int GetMinCost() => min;
        public int GetMaxCost() => max;

        public Game Game => ((IResearch)parent).Game;
        public int GetBlueprintLevel()
        {
            return ((IResearch)parent).GetBlueprintLevel();
        }
        public double GetMult(Research.Type type, double pow)
        {
            return ((IResearch)parent).GetMult(type, pow);
        }
        public bool HasType(Research.Type research)
        {
            return ((IResearch)parent).HasType(research);
        }
        public bool MakeType(Research.Type type)
        {
            return ((IResearch)parent).MakeType(type);
        }
        Research.Type IResearch.GetType()
        {
            return ((IResearch)parent).GetType();
        }
    }
}
