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
                foreach (var alias in Aliases)
                {
                    writer.WriteStartElement("alias");
                    writer.WriteAttributeString("name", alias);
                    writer.WriteEndElement();
                }
                foreach (var bit in bits)
                {
                    writer.WriteStartElement("statadd");
                    foreach (var field in typeof(bit).GetFields())
                    {
                        if (!string.IsNullOrEmpty( field.GetValue(bit) as string))
                            writer.WriteAttributeString(field.Name, (string)field.GetValue(bit));
                        if (field.GetValue(bit) is int)
                            writer.WriteAttributeString(field.Name, field.GetValue(bit).ToString());
                    }
                    //writer.WriteAttributeString("calcvalue",
                    writer.WriteEndElement( );
                }
                writer.WriteEndElement( );
            }
        }
        
    }
}
