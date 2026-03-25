using System;
using System.Runtime.Serialization;

namespace ClassLibrary1.Pieces.Players
{
    //[Serializable]
    //[DataContract(IsReference = true)]
    public interface IUpgradeValues
    {
        public void Upgrade(Research.Type type, double researchMult);
    }
}
