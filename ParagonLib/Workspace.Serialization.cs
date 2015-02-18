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
                foreach (var bit in bits)
                {
                    writer.WriteStartElement(bit.type);
                    foreach (var field in typeof(bit).GetFields())
                    {
                        if (!string.IsNullOrEmpty( (string)field.GetValue(bit)))
                            writer.WriteAttributeString(field.Name, (string)field.GetValue(bit));
                    }
                    //writer.WriteAttributeString(
                    writer.WriteEndElement( );
                }
                writer.WriteEndElement( );
            }
        }
        
    }
}
