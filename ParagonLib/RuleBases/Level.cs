using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public class Level : RulesElement, ILevel
    {
        protected int xPNeeded; // Nasty looking Variable, isn't it?

        public int XpNeeded
        {
            get { return xPNeeded; }
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
#region _GENERATED_

protected override string GetSpecific(string specific) {
    return base.GetSpecific(specific);
}
#endregion _GENERATED_
	}
}
