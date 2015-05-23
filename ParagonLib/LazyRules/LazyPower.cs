using ParagonLib.Rules;
using System.Linq;
using System.Xml.Linq;

namespace ParagonLib.LazyRules
{
    internal class LazyPower : LazyRulesElement, IPower
    {
        public LazyPower(XElement xpower)
            : base(xpower)
        {
            foreach (var ele in xpower.Elements("specific"))
            {
                var value = ele.Value.Trim();
                switch (ele.Attribute("name").Value)
                {
                    case "Power Usage":
                        PowerUsage = value;
                        break;

                    case "Power Type":
                        PowerType = value;
                        break;

                    case "Keywords":
                        Keywords = value.Split(',').Select(k => k.Trim()).ToArray();
                        break;

                    default:
                        break;
                }
            }
        }

        public string ActionType { get; private set; }

        public string AttackType { get; private set; }

        public string Class { get; private set; }

        public string Display { get; private set; } 

        public string[] Keywords { get; private set; }

        public int Level { get; private set; }

        public string PowerType { get; private set; }

        public string PowerUsage { get; private set; }

        public string Target { get; private set; }

        public string Trigger { get; private set; }
    }
}