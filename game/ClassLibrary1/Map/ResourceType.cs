using System;
using System.Runtime.Serialization;

namespace ClassLibrary1.Map
{
    public partial class Map
    {
        [Serializable]
        [DataContract(IsReference = true)]
        internal enum ResourceType
        {
            Artifact,
            Biomass,
            Metal,
            Foundation,
        }
    }
}
