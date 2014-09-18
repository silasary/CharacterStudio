using System.Collections.Generic;
using System.Linq;

namespace ParagonLib
{
    internal class RulesElement
    {
        private List<Instruction> Rules = new List<Instruction>();
        private List<string> Specifics = new List<string>();

        public RulesElement(System.Xml.Linq.XElement item)
        {
            this.Name = item.Attribute("name").Value;
            this.Type = item.Attribute("type").Value;
            this.Source = item.Attribute("source").Value;
            this.System = item.Parent.Attribute("game-system").Value;

            foreach (var element in item.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "specific":
                        Specifics.Add(string.Format("{0}:{1}", element.Attribute("name").Value, element.Value));
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
    }
}