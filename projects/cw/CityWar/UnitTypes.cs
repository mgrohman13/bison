using System;
using System.Collections.Generic;

namespace CityWar
{
    public static class UnitTypes
    {
        private static UnitSchema schema;
        public static UnitSchema GetSchema()
        {
            if (schema == null)
            {
                schema = new UnitSchema();
                schema.ReadXml(Game.Path + "Units.xml");
            }
            return schema;
        }
    }
}
