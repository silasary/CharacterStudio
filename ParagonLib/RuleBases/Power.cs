using ParagonLib.Compiler;
using ParagonLib.Grammar;
using ParagonLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using CharacterStudio.Rules;

namespace ParagonLib.RuleBases
{
    public class Power : RulesElement, IPower
    {
        public struct PowerLine
        {
            public readonly string Name;
            public readonly string Value;

            public PowerLine(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public override string ToString()
            {
                return String.Format("{0}: {1}", Name, Value);
            }

            public static implicit operator PowerLine(MissingElementAttribute e)
            {
                return new PowerLine(e.Element, e.Value);
            }

            public static explicit operator PowerLine(System.Xml.Linq.XElement e)
            {
                return new PowerLine(e.Attribute("name").Value.Trim(), e.Value.Trim());
            }
        }

        public Power()
        {
            var missing = this.GetType().CustomAttributes.OfType<MissingElementAttribute>();
            if (missing.Count() == 0)
            {
                //var t = this.GetType().CustomAttributes.First();
                //var first = Activator.CreateInstance(t.AttributeType, t.ConstructorArguments.Select(a => a.Value).ToArray());
                missing = this.GetType().CustomAttributes.Select(n => (MissingElementAttribute)Activator.CreateInstance(n.AttributeType, n.ConstructorArguments.Select(a => a.Value).ToArray()));
            }
            var ordered = missing.OrderBy(n => n.Order).ToArray();
            //TODO: Do useful stuff here.
            List<PowerLine> Lines = new List<PowerLine>();
            foreach (var line in ordered)
            {
                Lines.Add(line);
            }
            this.lines = Lines.ToArray();
            //GrammarParser.ParsePowerLines(out attackComponents, out damageComponents, lines);
            
        }
        
        AttackStat[] attackComponents;
        DamageStat damageComponents;
        public readonly PowerLine[] lines;

        protected string _class;
        protected string trigger;
        protected string powerUsage;
        protected string display;
        protected string[] keywords;
        protected string actionType;
        protected string attackType;
        protected string target;
        protected int level;
        protected string powerType;

        // List of edge cases:
        // * Chain Lightning [ID_FMP_POWER_466] has Three attacks, 
        //      Both extras are siblings
        // * Storm of Raining Blades [ID_FMP_POWER_13285] has three attacks, 
        //      Tertiary is child of Secondary
        //
        // Fortunately, Monk Full Discipline powers use _ChildPower.
        
        public AttackStat[] AttackComponents
        {
            get { return attackComponents; }
        }

        public DamageStat DamageComponents
        {
            get { return damageComponents; }
        }

        public int Level
        {
            get
            {
                return level;
            }
        }

        // Attack or Utility?
        public string PowerType
        {
            get
            {
                return powerType;
            }
        }

        public string Trigger
        {
            get
            {
                return trigger;
            }
        }

        public string Class
        {
            get
            {
                return _class;
            }
        }

        public string Target
        {
            get
            {
                return target;
            }
        }

        public string AttackType
        {
            get
            {
                return attackType;
            }
        }

        public string ActionType
        {
            get
            {
                return actionType;
            }
        }

        public string PowerUsage
        {
            get
            {
                return powerUsage;
            }
        }


        public string Display
        {
            get
            {
                return display;
            }
        }

        public string[] Keywords
        {
            get
            {
                return keywords;
            }
        }

        public PowerLine[] Lines { get { return lines; } }
#region _GENERATED_

protected override string GetSpecific(string specific) {
    return base.GetSpecific(specific);
}
#endregion _GENERATED_
	}
}

