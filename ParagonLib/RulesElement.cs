using System.Collections.Generic;
using System.Linq;

namespace ParagonLib
{
    public class RulesElement
    {
        public List<Instruction> Rules = new List<Instruction>();
        public Dictionary<string, string> Specifics = new Dictionary<string, string>();

        public RulesElement(System.Xml.Linq.XElement item)
        {
            
            this.Name = item.Attribute("name").Value;
            this.Type = item.Attribute("type").Value;
            if (item.Attribute("source") == null)
                this.Source = "ERROR: Source Unknown";
            else
                this.Source = item.Attribute("source").Value;
            this.InternalId = item.Attribute("internal-id").Value;
            this.System = item.Document.Root.Attribute("game-system").Value;
            if (item.Element("Category") == null)
                this.Category = new string[0];
            else
                this.Category = item.Element("Category").Value.Split(',');

            foreach (var element in item.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "specific":
                        Specifics.Add(element.Attribute("name").Value, element.Value);
                        break;
                    case "rules":
                        foreach (var rule in element.Elements())
                        {
                            Rules.Add(new Instruction(rule.Name.LocalName, MakeDict(rule.Attributes())));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private Dictionary<string, string> MakeDict(IEnumerable<System.Xml.Linq.XAttribute> enumerable)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            foreach (var item in enumerable)
            {
                d.Add(item.Name.LocalName, item.Value);
            }
            return d;
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Source { get; set; }

        public string System { get; set; }

        public string InternalId { get; set; }

        public string[] Category { get; set; }
    }
}