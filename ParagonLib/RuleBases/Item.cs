using ParagonLib.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public class Item : RulesElement, IItem
    {
        protected string weight;
        protected int gold;
        protected int silver;
        protected int copper;
        protected string group;
        protected string fullText;
        protected string itemSlot;
        protected int count;

        public string Weight
        {
            get { return weight; }
        }

        public int Gold
        {
            get { return gold; }
        }

        public int Silver
        {
            get { return silver; }
        }

        public int Copper
        {
            get { return copper; }
        }

        public string Group
        {
            get { return group; } // TODO: Maybe enum?
        }

        public string FullText
        {
            get { return fullText; }
        }

        public string ItemSlot
        {
            get { return itemSlot; }
        }

        public int Quantity
        {
            get { return count; }
        }
    }
}
