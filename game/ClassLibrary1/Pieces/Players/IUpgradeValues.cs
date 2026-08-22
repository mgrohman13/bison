namespace ClassLibrary1.Pieces.Players
{
    //[Serializable]
    //[DataContract(IsReference = true)]
    public interface IUpgradeValues
    {
        public void Init(Game game);
        public void Upgrade(Game game, Research.Type type, double researchMult);
    }
}
