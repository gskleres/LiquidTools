using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DistrictCostUpdates
{
	class Districts
    {
        // params
        private const string DistrictFilePath = @"F:\SteamLib\steamapps\common\Sid Meier's Civilization VI\Base\Assets\Gameplay\Data\Districts.xml";
        private const string OutputModFile = @"F:\SteamLib\steamapps\common\Sid Meier's Civilization VI\DLC\Liquid\LiquidMod\Data\LowerDistrictCost.xml";
        private const float CostModifier = 0.75f;


		private const string GameInfoRootNodeName = "GameInfo";
		private const string RowElemName = "Row";
		private const string ElementNodeName = "Districts";
		private const string CostAttribute = "Cost";
		private const string KeyAttribute = "DistrictType";

		private const string UpdateElemName = "Update";
		private const string SetElemName = "Set";
		private const string WhereElemName = "Where";
			
		static void Main(string[] args)
		{
            

			XDocument reader = XDocument.Load(DistrictFilePath);
			XDocument doc = XDocument.Load(OutputModFile);

			var techs = reader.Element(GameInfoRootNodeName).Element(ElementNodeName);
			var output = doc.Element(GameInfoRootNodeName).Element(ElementNodeName);
			output.Descendants().Remove();
			foreach (var row in techs.Elements(RowElemName))
			{
		
                var costStr = row.Attribute(CostAttribute).Value;
				var key = row.Attribute(KeyAttribute).Value;
				int cost;
				if (int.TryParse(costStr, out cost) == false)
				{
					Console.WriteLine("Failed to read cost for tech " + key);
					continue;
				}
				cost = (int)(cost * CostModifier);


				output.Add(new XElement(UpdateElemName,
									new XElement(SetElemName, new XAttribute(CostAttribute, cost)),
									new XElement(WhereElemName, new XAttribute(KeyAttribute, key))
								)
							);
			}

			doc.Save(OutputModFile);
		}
	}
}