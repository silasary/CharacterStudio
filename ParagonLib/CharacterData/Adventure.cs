using ParagonLib.CharacterData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ParagonLib.CharacterData
{
    [DataContract(Name = "JournalEntry", Namespace = "")]
    public class Adventure
    {
        public Adventure() : this(Guid.NewGuid()) { }
        public Adventure(Guid guid)
        {
            this.guid = guid;
            this.LootDiff = new Loot[0];
        }

        public Guid guid { get; set; }
        [DataMember(Order=0)]
        public DateTime TimeStamp { get; set; }
        [DataMember(Order = 1)]
        public string Title { get; set; }
        [DataMember(Order = 2)]
        public int XPStart { get; set; }
        [DataMember(Order = 3)]
        public int XPGain { get; set; }
        [DataMember(Order = 4)]
        public int XPTotal { get { return XPStart + XPGain; } private set { } }
        [DataMember(Order = 5)]
        public int LevelAtEnd { get; set; }
        [DataMember(Order = 6)]
        public double GPDelta { get; set; }

        [DataMember(Order = 7)]
        public string Treasure { get; set; }

        [DataMember(Order = 8)]
        [Description("Sometimes Living campaigns require you to mark the region/setting of the adventure.")]
        public string Region { get; set; }


        [DataMember(Order = 9)]
        public string Notes { get; set; }

        [DataMember(Order = 10, IsRequired=false)]
        [Description("Image Entries use these.")]
        public string Uri { get; set; }

        [DataMember(Order=11)]
        public Loot[] LootDiff { get; set; }
    }
}
