using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CivTechUpdates
{
    class Tech
    {
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
                return ;
            }

            XDocument reader = XDocument.Load(techSourceFile);
            XDocument doc = XDocument.Load(techUpdateFile);

            var techs = reader.Element("GameInfo").Element("Technologies");
            var output = doc.Element("GameInfo").Element("Technologies");
            output.Descendants().Remove();
            foreach (var row in techs.Elements("Row"))
            {
                /*
                		<Update>
			                <Set YieldChange="3" />
			                <Where ImprovementType="IMPROVEMENT_FARM" YieldType="YIELD_FOOD" />
		                </Update>
                */

                var costStr = row.Attribute("Cost").Value;
                var techType = row.Attribute("TechnologyType").Value;
                int cost;
                if (int.TryParse(costStr, out cost) == false)
                {
                    Console.WriteLine("Failed to read cost for tech " + techType);
                    continue;
                }
                cost = (int)(cost * 0.75f);


                output.Add(new XElement("Update",
                                    new XElement("Set", new XAttribute("Cost", cost)),
                                    new XElement("Where", new XAttribute("TechnologyType", techType))
                                )
                            );
            }

            doc.Save(techUpdateFile);
        }
    }
}
