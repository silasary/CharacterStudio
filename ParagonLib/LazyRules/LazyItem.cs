using ParagonLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ParagonLib.LazyRules
{
    internal class LazyItem : LazyRulesElement, IItem
    {
        internal LazyItem(XElement ele)
            : base(ele)
        {
            
        }

        public string Weight { get { return GetSpecific(); } }

        public int Gold  { get { return GetSpecificInt(); } }

        public int Silver  { get { return GetSpecificInt(); } }

        public int Copper  { get { return GetSpecificInt(); } }

        public string Group  { get { return GetSpecific(); } }

        public string FullText  { get { return GetSpecific(); } }

        public string ItemSlot  { get { return GetSpecific(); } }

        public int Quantity  { get { return GetSpecificInt(); } }
    }
}
