﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ParagonLib.Compiler;


namespace ParagonLib.RuleBases
{
    public class LazyRulesElement : RulesElement
    {
        public static RulesElement New(XElement item)
        {
            var type = item.Attribute("type").Value;
            if (type == "Level")
                return new LazyLevelElement(item);
            return new LazyRulesElement(item);
        }

        public List<Instruction> Rules = new List<Instruction>();
        public SpecificsDict Specifics = new SpecificsDict();

        protected LazyRulesElement(XElement item)
        {
            if (item == null)
                return;
            this.name = item.Attribute("name").Value;
            this.type = item.Attribute("type").Value;
            if (item.Attribute("source") == null)
                this.source = "ERROR: Source Unknown";
            else
                this.source = item.Attribute("source").Value;
            this.internalId = item.Attribute("internal-id").Value;
            this.system = item.Document.Root.Attribute("game-system").Value;
            if (item.Document.Root.Attribute("Filename") != null)
                this.SourcePart = item.Document.Root.Attribute("Filename").Value;

            if (item.Element("Category") == null)
                this.category = new string[0];
            else
                this.category = item.Element("Category").Value.Split(',');

            foreach (var element in item.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "specific":
                        Specifics.Add(element.Attribute("name").Value, element.Value);
                        break;
                    case "rules": // Yes, the first call to Calculate() compiles then calls Calculate().
                        Calculate = new Action<CharElement, Workspace>((e, ws) =>
                        {
                            foreach (var rule in element.Elements())
                            {
                                Rules.Add(new Instruction(rule.Name.LocalName, Builders.MakeDict(rule.Attributes()), SourcePart, ((IXmlLineInfo)rule).LineNumber));
                            }
                            var Body = Instruction.Merge(Rules);
                            Calculate = Body.Compile();
                            Calculate(e, ws);
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        public string SourcePart { get; private set; }

        private class LazyLevelElement : LazyRulesElement, ILevel
        {
            public LazyLevelElement(XElement item) : base(item)
            {
                int xp;
                if (int.TryParse(item.Element("specific").Value, out xp))
                    XpNeeded = xp;
            }
            public ILevel PreviousLevel {get; set;}

            public int TotalXpNeeded
            {
                get
                {
                    if (PreviousLevel != null)
                        return XpNeeded + PreviousLevel.TotalXpNeeded;
                    return XpNeeded;
                }
            }

            public int XpNeeded { get; private set; }
        }
    }

    
}