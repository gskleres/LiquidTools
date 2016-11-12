using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CivicCostUpdates
{
	class Civics
	{
        private const string GameInfoRootNodeName = "GameInfo";
        private const string RowElemName = "Row";
        private const string ElementNodeName = "Civics";
        private const string CostAttribute = "Cost";
        private const string KeyAttribute = "CivicType";

        private const string UpdateElemName = "Update";
        private const string SetElemName = "Set";
        private const string WhereElemName = "Where";
            
        static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine(@"Usage: 
{0} pathToOriginalCiv6_Technologies.xml pathToMod_LowerTechCost.xml CostModifier", System.AppDomain.CurrentDomain.FriendlyName);
			}
			string techSourceFile = args[0];
			string techUpdateFile = args[1];
			string costModStr = args[2];
			float costMod;
			if (float.TryParse(costModStr, out costMod) == false)
			{
				Console.WriteLine("Could not read cost modifier float value (should be 0.xx value-ish): " + costModStr);
				return;
			}

			XDocument reader = XDocument.Load(techSourceFile);
			XDocument doc = XDocument.Load(techUpdateFile);

			var techs = reader.Element(GameInfoRootNodeName).Element(ElementNodeName);
			var output = doc.Element(GameInfoRootNodeName).Element(ElementNodeName);
			output.Descendants().Remove();
			foreach (var row in techs.Elements(RowElemName))
			{
				/*
						<Update>
							<Set YieldChange="3" />
							<Where ImprovementType="IMPROVEMENT_FARM" YieldType="YIELD_FOOD" />
						</Update>
				*/

				var costStr = row.Attribute(CostAttribute).Value;
				var key = row.Attribute(KeyAttribute).Value;
				int cost;
				if (int.TryParse(costStr, out cost) == false)
				{
					Console.WriteLine("Failed to read cost for tech " + key);
					continue;
				}
				cost = (int)(cost * 0.75f);


				output.Add(new XElement(UpdateElemName,
									new XElement(SetElemName, new XAttribute(CostAttribute, cost)),
									new XElement(WhereElemName, new XAttribute(KeyAttribute, key))
								)
							);
			}

			doc.Save(techUpdateFile);
		}
	}
}