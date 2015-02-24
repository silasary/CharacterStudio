using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ParagonLib
{
    partial class Workspace
    {
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
                        if (field.GetValue(bit) is int && (int)field.GetValue(bit) != 0) // Level.
                            writer.WriteAttributeString(field.Name, field.GetValue(bit).ToString());
                    }
                    var realval = calc(bit, workspace.level);
                    if (realval.ToString() != bit.value)
                        writer.WriteAttributeString("calcvalue", realval.ToString());
                    writer.WriteEndElement( );
                }
                writer.WriteEndElement( );
            }
        }
        
    }
}
