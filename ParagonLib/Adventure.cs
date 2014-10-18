using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ParagonLib
{
    [DataContract]
    public class Adventure
    {
        [DataMember]
        public int XpEarned { get; set; }
        [DataMember]
        public int GpEarned { get; set; }

        [DataMember]
        [Description("Sometimes Living campaigns require you to mark the region/setting of the adventure.")]
        public string Region { get; set; }

        [DataMember]
        public string Treasure { get; set; }

        [DataMember]
        public string Text { get; set; }

        public int StartXp { get; set; }
        public int EndXp { get { return StartXp + XpEarned; } }



    }
}
