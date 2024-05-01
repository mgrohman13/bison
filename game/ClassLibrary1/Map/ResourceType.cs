using ClassLibrary1.Pieces.Players;
using System;

namespace ClassLibrary1.Map
{
    public partial class Map
    {
        [Serializable]
        internal enum ResourceType
        {
            Artifact,
            Biomass,
            Metal,
            Foundation,
        }
    }
}
