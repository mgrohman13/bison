using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CityWar
{
	public class UnitTypes
	{
		private static UnitSchema schema = new UnitSchema();
		private static DateTime readTime = DateTime.MinValue;

		public static UnitSchema GetSchema()
		{
			if (File.GetLastWriteTime(Game.Path + "Units.xml") > readTime)
			{
				readTime = DateTime.Now;

				schema.Clear();
				schema.ReadXml(Game.Path + "Units.xml");
			}
			return schema;
		}
	}
}
