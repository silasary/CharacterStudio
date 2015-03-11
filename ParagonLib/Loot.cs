using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace ParagonLib
{
    public class Loot : IEqualityComparer<Loot>
    {
        public enum AddRemove {Add= 1, Remove = -1, Neutral = 0}

        /*
            <loot count="-1" equip-count="0" ShowPowerCard="1" >
                <RulesElement name="Hunting Panther Ki Focus +1" type="Magic Item" internal-id="ID_FMP_MAGIC_ITEM_9544" url="http://www.wizards.com/dndinsider/compendium/item.aspx?fid=9544&amp;ftype=1" charelem="256573e8" legality="rules-legal" />
            </loot>
        */
        [XmlAttribute]
        public AddRemove Count { get; set; }

        [XmlAttribute]
        public AddRemove Equipped { get; set; }

        [XmlAttribute]
        public AddRemove Silvered { get; set; }

        [IgnoreDataMember]
        public Item ItemRef { get; set; }

        [XmlAttribute]
        public AddRemove ShowPowerCard { get; set; }

        internal int levelAquired;

        public int CharElemId { get; internal set; }

        #region IEqualityComparer implementation


        public bool Equals(Loot x, Loot y)
        {
            return x.Count == y.Count && x.Equipped == y.Equipped && x.ItemRef == y.ItemRef && x.levelAquired == y.levelAquired && x.Silvered == y.Silvered;
        }


        public int GetHashCode(Loot obj)
        {
            throw new NotImplementedException();
        }


        #endregion

    }
}
