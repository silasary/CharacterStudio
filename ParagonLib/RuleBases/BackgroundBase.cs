using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public abstract class BackgroundBase : RulesElement
    {
        protected string _type;

        public string BackgroundType
        {
            get { return _type; }
        }

        public string campaign;

        public string commonKnowledge;

        public string benefit;

        public string associatedSkills;

        public string associatedLanguages;
    }
}
