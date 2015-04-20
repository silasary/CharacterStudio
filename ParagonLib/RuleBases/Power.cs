using System;

namespace ParagonLib.RuleBases
{
    public class Power : RulesElement
    {
        
        protected string powerUsage;
        protected string display;
        protected string[] keywords;
        protected string actionType;
        protected string attackType;

        //TODO: Look at powers.  Do we have any that do really weird things.
        //I seem to recall at least one with two Effect: lines, but I don't remember where.

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
    }
}

