using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ParagonLib
{
    [DataContract(Name = "JournalEntry", Namespace = "")]
    public class Adventure
    {
        [DataMember(Order = 1)]
        public string Title { get; set; }
        [DataMember(Order = 2)]
        public int StartXp { get; set; }
        [DataMember(Order = 3)]
        public int XpEarned { get; set; }
        [DataMember(Order = 4)]
        public int EndXp { get { return StartXp + XpEarned; } private set { } }

        [DataMember(Order = 6)]
        public int GpEarned { get; set; }

        [DataMember(Order = 7)]
        public string Treasure { get; set; }

        [DataMember(Order = 8)]
        [Description("Sometimes Living campaigns require you to mark the region/setting of the adventure.")]
        public string Region { get; set; }


        [DataMember(Order = 9)]
        public string Notes { get; set; }

    }
}
