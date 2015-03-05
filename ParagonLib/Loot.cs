using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class Loot
    {
        /*
            <loot count="-1" equip-count="0" ShowPowerCard="1" >
                <RulesElement name="Hunting Panther Ki Focus +1" type="Magic Item" internal-id="ID_FMP_MAGIC_ITEM_9544" url="http://www.wizards.com/dndinsider/compendium/item.aspx?fid=9544&amp;ftype=1" charelem="256573e8" legality="rules-legal" />
            </loot>
        */
        /// <summary>
        /// The number added/removed.  Can be negative.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Did we Equip/Unequip it? Not sure why it's an int.
        /// </summary>
        public int Equipped { get; set; }

        /// <summary>
        /// Does this need to be an int?
        /// </summary>
        public bool ShowPowerCard { get; set; }

        
    }
}
