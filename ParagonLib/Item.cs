using ParagonLib.RuleBases;
using ParagonLib.RuleEngine;
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
        /// <summary>
        /// We need this because elements are lazy-loaded.
        /// </summary>
        protected string GameSystem;

        public RulesElement Augment { get; private set; }

        private RulesElement _base = null;
        public RulesElement Base
        {
            get
            {
                if (_base == null)
                    _base = RuleFactory.FindRulesElement(baseId, GameSystem);
                return _base;
            }
        }

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

        public int Count { get; set; }

        public RulesElement Curse { get; private set; }

        private RulesElement _enchantment;
        public RulesElement Enchantment
        {
            get
            {
                if (_enchantment == null)
                    _enchantment = RuleFactory.FindRulesElement(enchantmentId, GameSystem);
                return _enchantment;
            }
        }

        public string Type
        {
            get
            {
                if (Base == null)
                    return null;
                return Base.Type;
            }
        }

        private D20Currency _cost = null;
        internal string augmentId;
        internal string baseId;
        internal string curseId;
        internal string enchantmentId;
     
        public Item(string[] ids, string gameSystem)
        {
            this.GameSystem = gameSystem;
            this.baseId = ids[0];
            if (ids.Length > 1)
                this.enchantmentId = ids[1];
            if (ids.Length > 2)
                this.curseId = ids[2];
        }

        protected Item(Item clone)
        {
            this.GameSystem = clone.GameSystem;
            this.baseId = clone.baseId;
            this.enchantmentId = clone.enchantmentId;
            this.curseId = clone.curseId;
        }

        public string Name
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                if (Enchantment != null)
                {
                    sb.Append(Enchantment.Name).Append(" ");
                }
                sb.Append(Base.Name);
                return sb.ToString();
            }
        }
    }
}
