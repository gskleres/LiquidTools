using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MedUnitCost
{
    /*
        select * from Units u
        inner join Technologies t on u.prereqTech = t.TechnologyType
        inner join eras te on t.eratype = te.eratype

        where te.ChronologyIndex >= 3
    */
    class Program
    {
        public const string EraSourceFile = @"F:\SteamLib\steamapps\common\Sid Meier's Civilization VI\Base\Assets\Gameplay\Data\Eras.xml";
        public const string TechSourceFile = @"F:\SteamLib\steamapps\common\Sid Meier's Civilization VI\Base\Assets\Gameplay\Data\Technologies.xml";
        public const string CivicSourceFile = @"F:\SteamLib\steamapps\common\Sid Meier's Civilization VI\Base\Assets\Gameplay\Data\Civics.xml";
        public const string UnitSourceFile = @"F:\SteamLib\steamapps\common\Sid Meier's Civilization VI\Base\Assets\Gameplay\Data\Units.xml";
        public const string OutputFile = @"F:\SteamLib\steamapps\common\Sid Meier's Civilization VI\DLC\Liquid\LiquidMod\Data\LowerMedAndLaterUnitCost.xml";

        public const float CostModifier = 0.50f;


        public const string GameInfoName = "GameInfo";
        public const string ErasName = "Eras";
        public const string TechName = "Technologies";
        public const string CivicName = "Civics";
        public const string UnitName = "Units";

        public const string RowName = "Row";
        public const string EraTypeName = "EraType";
        public const string TechTypeName = "TechnologyType";
        public const string CivicTypeName = "CivicType";
        public const string PrereqTechName = "PrereqTech";
        public const string PrereqCivicName = "PrereqCivic";
        public const string UnitTypeName = "UnitType";

        public const string CostName = "Cost";
        public const string ChronologyIndex = "ChronologyIndex";



        private const string UpdateElemName = "Update";
        private const string SetElemName = "Set";
        private const string WhereElemName = "Where";

        static void Main(string[] args)
        {
            var eras = XDocument.Load(EraSourceFile)
                                .Element(GameInfoName)
                                .Element(ErasName)
                                .Elements(RowName)
                                .Select(elem =>
                                    new
                                    {
                                        EraType = elem.Attribute(EraTypeName).Value,
                                        ChronologyIndex = int.Parse(elem.Attribute(ChronologyIndex).Value),
                                    })
                                .ToDictionary(e => e.EraType);
            var techs = XDocument.Load(TechSourceFile)
                                .Element(GameInfoName)
                                .Element(TechName)
                                .Elements(RowName)
                                .Select(elem =>
                                new
                                {
                                    TechnologyType = elem.Attribute(TechTypeName).Value,
                                    Era = eras[elem.Attribute(EraTypeName).Value],
                                })
                                .ToDictionary(t => t.TechnologyType);
            var civics = XDocument.Load(CivicSourceFile)
                                .Element(GameInfoName)
                                .Element(CivicName)
                                .Elements(RowName)
                                .Select(elem =>
                                new
                                {
                                    CivicType = elem.Attribute(CivicTypeName).Value,
                                    Era = eras[elem.Attribute(EraTypeName).Value],
                                })
                                .ToDictionary(c => c.CivicType);
            var units = XDocument.Load(UnitSourceFile)
                                 .Element(GameInfoName)
                                 .Element(UnitName)
                                 .Elements(RowName)
                                 .Select(elem => new
                                 {
                                     UnitType = elem.Attribute(UnitTypeName).Value,
                                     Cost = int.Parse(elem.Attribute(CostName).Value),
                                     PrereqCivic = elem.Attribute(PrereqCivicName) != null ? civics[elem.Attribute(PrereqCivicName).Value] : null,
                                     PrereqTech = elem.Attribute(PrereqTechName) != null ?  techs[elem.Attribute(PrereqTechName).Value] : null,
                                 });

            var output = XDocument.Load(OutputFile);

            var civicsTab = civics.Values.Where(civ => civ.Era.ChronologyIndex >= 3).ToArray();
            var techsTab = techs.Values.Where(tech => tech.Era.ChronologyIndex >= 3).ToArray();

            var unitsTab = units.Where(u =>
                                    (u.PrereqCivic != null && u.PrereqCivic.Era.ChronologyIndex >= 3) ||
                                    (u.PrereqTech != null && u.PrereqTech.Era.ChronologyIndex >= 3)
                                ).ToArray();

            var outputUnits = output.Element(GameInfoName).Element(UnitName);
            outputUnits.Descendants().Remove();

            foreach (var unit in unitsTab)
            {

                var cost = (int)(unit.Cost * CostModifier);

                outputUnits.Add(new XElement(UpdateElemName,
                                    new XElement(SetElemName, new XAttribute(CostName, cost)),
                                    new XElement(WhereElemName, new XAttribute(UnitTypeName, unit.UnitType))
                                )
                            );

            }
            output.Save(OutputFile);
        }
    }
}
