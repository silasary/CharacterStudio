using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class Loot
    {
        public enum AddRemove {Add= 1, Remove = -1, Neutral = 0}

        /*
            <loot count="-1" equip-count="0" ShowPowerCard="1" >
                <RulesElement name="Hunting Panther Ki Focus +1" type="Magic Item" internal-id="ID_FMP_MAGIC_ITEM_9544" url="http://www.wizards.com/dndinsider/compendium/item.aspx?fid=9544&amp;ftype=1" charelem="256573e8" legality="rules-legal" />
            </loot>
        */
        public AddRemove Count { get; set; }

        public AddRemove Equipped { get; set; }

        public Item ItemRef { get; set; }

        public AddRemove ShowPowerCard { get; set; }
    }
}
