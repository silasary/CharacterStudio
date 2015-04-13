using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ParagonLib.Compiler;


namespace ParagonLib
{
    //TODO: [Obsolete]
    public class RulesElement
    {
        public List<Instruction> Rules = new List<Instruction>();
        public SpecificsDict Specifics = new SpecificsDict();

        public RulesElement(XElement item)
        {
            if (item == null)
                return;
            this.Name = item.Attribute("name").Value;
            this.Type = item.Attribute("type").Value;
            if (item.Attribute("source") == null)
                this.Source = "ERROR: Source Unknown";
            else
                this.Source = item.Attribute("source").Value;
            this.InternalId = item.Attribute("internal-id").Value;
            this.System = item.Document.Root.Attribute("game-system").Value;
            if (item.Document.Root.Attribute("Filename")!=null)
                this.SourcePart = item.Document.Root.Attribute("Filename").Value;

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
                            Rules.Add(new Instruction(rule.Name.LocalName, Builders.MakeDict(rule.Attributes()), SourcePart, ((IXmlLineInfo)rule).LineNumber));
                        }
                        Body = Instruction.Merge(Rules);
                        Calculate = Body.Compile();
                        break;
                    default:
                        break;
                }
            }
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Source { get; set; }

        public string System { get; set; }

        public string InternalId { get; set; }

        public string[] Category { get; set; }

        public string SourcePart { get; set; }

        public Action<CharElement, Workspace> Calculate { get; set; }

        public global::System.Linq.Expressions.Expression<Action<CharElement, Workspace>> Body { get; set; }

    }
}