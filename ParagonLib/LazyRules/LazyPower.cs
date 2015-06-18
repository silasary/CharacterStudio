using ParagonLib.Grammar;
using ParagonLib.Rules;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PowerLine = ParagonLib.RuleBases.Power.PowerLine;

namespace ParagonLib.LazyRules
{
    internal class LazyPower : LazyRulesElement, IPower
    {
        internal LazyPower(XElement xpower)
            : base(xpower)
        {
            var lines = new List<PowerLine>();
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
                    case "Display":
                        Display = value;
                        break;
                    case "Action Type":
                        ActionType = value;
                        break;
                    case "Attack Type":
                        AttackType = value;
                        break;
                    default:
                        lines.Add((PowerLine)ele);
                        break;
                }
            }
            this.Lines = lines.ToArray();
            AttackStat[] attackComponents;
            DamageStat damageComponents;
            //GrammarParser.ParsePowerLines(out attackComponents, out damageComponents, Lines);
            //AttackComponents = attackComponents;
            //DamageComponents = damageComponents;
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

        PowerLine[] Lines;

        public AttackStat[] AttackComponents { get; private set; }

        public DamageStat DamageComponents { get; private set; }
    }
}