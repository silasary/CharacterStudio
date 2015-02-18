using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

        partial class Stat {
            internal void Write(XmlWriter writer)
            {
                writer.WriteStartElement("Stat");
                writer.WriteAttributeString("value", this.Value.ToString( ));
                var ser = new DataContractSerializer(typeof(bit));
                foreach (var bit in bits)
                {
                    writer.WriteStartElement(bit.type);

                    //writer.WriteAttributeString(
                    writer.WriteEndElement( );
                }
                writer.WriteEndElement( );
            }
        }
        
    }
}
