﻿using System;
using System.Collections.Generic;
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

        public int StartXp { get; set; }
        public int EndXp { get { return StartXp + XpEarned; } }


    }
}