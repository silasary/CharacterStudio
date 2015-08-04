using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace RulesDb
{
    class ParseXml
    {

        public string GameSystem { get; private set; }

        internal void Parse(XmlDocument xmlDocument)
        {
            foreach (XmlNode item in xmlDocument)
            {
                if (item.NodeType == XmlNodeType.Element)
                {
                    ParseRoot(item);
                }
            }
        }

        private void ParseRoot(XmlNode item)
        {
            if (item.Name == "D20Rules")
                GameSystem= item.Attributes["game-system"].Value;
            foreach (XmlNode child in item)
            {
                if (child.NodeType==XmlNodeType.Element)
                {
                    if (child.Name == "Region")
                        ParseRoot(child);
                    else if (child.Name == "RulesElement")
                        ParseRulesElement(child);
                }
            }
        }

        private void ParseRulesElement(XmlNode element)
        {
            var Name = element.Attributes["name"].Value;
            var Type = element.Attributes["type"].Value;
            var InternalId = element.Attributes["internal-id"].Value;
            var Source = element.Attributes["source"].Value;
            var re = RulesDb.GetOrAddRule(GameSystem, Name, Type, InternalId, Source);
            foreach (XmlNode item in element.ChildNodes)
            {
                if (item.NodeType == XmlNodeType.Element)
                {
                    switch (item.Name)
                    {
                        case "Category":
                            re.SetCategories(item.InnerText);
                            break;
                        case "Prereqs":
                            break;
                        case "specific":
                            re.SetSpecific(item.Attributes["name"].Value, item.Value);
                            break;
                        case "rules":
                            // TODO
                            break;
                        case "Flavor":
                            re.SetFlavor(item.Value);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
