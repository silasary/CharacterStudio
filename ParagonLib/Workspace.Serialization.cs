using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    [Serializable]
    [KnownType(typeof(Stat))]
    partial class Workspace : ISerializable
    {

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("statblock", Stats.Values.Distinct().ToArray(),typeof(Stat[]));
        }
        
    }
}
