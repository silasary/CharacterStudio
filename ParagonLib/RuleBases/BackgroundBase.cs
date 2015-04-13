using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public abstract class BackgroundBase : RulesElementBase
    {
        protected string _type;

        public string BackgroundType
        {
            get { return _type; }
        }

        protected string campaign;

        protected string commonKnowledge;

        protected string benefit;

        protected string shortDescription;

        protected string associatedSkills;

        protected string associatedLanguages;
    }
}
