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
        protected string target;

        // List of edge cases:
        // * Chain Lightning [ID_FMP_POWER_466] has Three attacks, 
        //      Both extras are siblings
        // * Storm of Raining Blades [ID_FMP_POWER_13285] has three attacks, 
        //      Tertiary is child of Secondary
        //
        // Fortunately, Monk Full Discipline powers use _ChildPower.

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
    }
}

