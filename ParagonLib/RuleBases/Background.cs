using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ParagonLib.RuleEngine;

namespace ParagonLib.RuleBases
{
    public abstract class Background : RulesElement
    {
        [AccessedThroughProperty("BackgroundType")]
        protected string _type;
        
        [AccessedThroughProperty("Campaign")]
        protected string campaign;

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
#region _GENERATED_

public string BackgroundType {
    get {
        return this._type;
    }
}

public string Campaign {
    get {
        return this.campaign;
    }
}

protected override string GetSpecific(string specific) {
    if ((specific == "BackgroundType")) {
        return this._type;
    }
    if ((specific == "Campaign")) {
        return this.campaign;
    }
    return base.GetSpecific(specific);
}
#endregion _GENERATED_
	}
}
