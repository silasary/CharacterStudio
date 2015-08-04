using ParagonLib.RuleBases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CharacterStudio.Rules;

namespace ParagonLib.LazyRules
{
    internal class LazyLevelElement : LazyRulesElement, ILevel
    {
        public LazyLevelElement(XElement item)
            : base(item)
        {
            int xp;
            if (int.TryParse(item.Element("specific").Value, out xp))
                XpNeeded = xp;
        }
        public ILevel PreviousLevel { get; set; }

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
