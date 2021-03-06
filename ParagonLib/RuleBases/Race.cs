using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public class Race : RulesElement
    {
        public string Characteristics
        {
            get { return characteristics; }
        }

        public string PhysicalQualities
        {
            get { return physicalQualities; }
        }

        public string Playing
        {
            get { return playing; }
        }

        public string Size
        {
            get { return size; }
        }

        public string Speed
        {
            get { return speed; }
        }

        public string Vision
        {
            get { return vision; }
        }

        protected string averageWeight;
        protected string abilityScores;
        protected string languages;

        protected string characteristics;
        protected string physicalQualities;
        protected string playing;
        protected string size;
        protected string speed;
        protected string vision;
#region _GENERATED_

protected override string GetSpecific(string specific) {
    return base.GetSpecific(specific);
}
#endregion _GENERATED_
	}
}
