using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public class Race : RulesElement
    {
        protected string speed;

        protected string characteristics;

        protected string physicalQualities;

        protected string playing;

        protected string vision;

        protected string size;

        public string Size
        {
            get { return size; }
        }


        public string Vision
        {
            get { return vision; }
        }


        public string Playing
        {
            get { return playing; }
        }


        public string PhysicalQualities
        {
            get { return physicalQualities; }
        }


        public string Characteristics
        {
            get { return characteristics; }
        }


        public string Speed
        {
            get { return speed; }
        }

    }
}
