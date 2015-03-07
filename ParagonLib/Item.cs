using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    /// <summary>
    /// Items need to be 1st class elements.
    /// </summary>
    public class Item
    {
        public RulesElement Base { get; private set; }
        public RulesElement Enchantment { get; private set; }
        public RulesElement Augment { get; private set; }
        public RulesElement Curse { get; private set; }

        public string Type { get { return Base.Type; } }
        private D20Currency _cost = null;
        public D20Currency Cost
        {
            get
            {
                if (_cost == null)
                {
                    if (Enchantment != null)
                        _cost = new D20Currency(Enchantment);
                    else
                        _cost = new D20Currency(Base);
                }
                return _cost;
            }
        }
    }
}
