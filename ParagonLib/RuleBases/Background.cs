using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public abstract class Background : RulesElement
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

        //private Action<CharElement, Workspace> calculate;

        //public Background()
        //{
        //    calculate = Calculate;
        //    Calculate = Calc;
        //}

        //private void Calc(CharElement e, Workspace ws)
        //{
        //    calculate(e,ws);
        //}
    }
}
