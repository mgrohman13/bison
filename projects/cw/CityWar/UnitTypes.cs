using System;
using System.Collections.Generic;

namespace CityWar
{
    [Serializable]
    public class UnitTypes
    {
        private UnitSchema schema;

        public UnitTypes()
        {
            schema = new UnitSchema();
            schema.ReadXml(Game.ResourcePath + "Units.xml");
        }

        public UnitSchema GetSchema()
        {
            return schema;
        }
    }
}
