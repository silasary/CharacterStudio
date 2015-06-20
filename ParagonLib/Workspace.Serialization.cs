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
            internal void Write(XmlWriter writer, Serializer.SFVersion SaveFileVersion)
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
                    if (SaveFileVersion > Serializer.SFVersion.v007b)
                    {
                        foreach (var field in typeof(bit).GetFields())
                        {
                            if (!string.IsNullOrEmpty(field.GetValue(bit) as string))
                                writer.WriteAttributeString(field.Name, (string)field.GetValue(bit));
                            if (field.GetValue(bit) is int && (int)field.GetValue(bit) != 0) // Level and charelem
                                writer.WriteAttributeString(field.Name, field.GetValue(bit).ToString());
                        }
                    }
                    else
                    {
                        //<statadd Level="1" value="1" statlink="Intelligence" abilmod="true" charelem="e93ad38" />
                        if (bit.Level > 0)
                            writer.WriteAttributeString("Level", bit.Level.ToString());
                        int val;
                        if (int.TryParse(bit.value, out val))
                            writer.WriteAttributeString("value", bit.value.ToString());
                        else
                        {
                            writer.WriteAttributeString("value", "1");
                            var m = workspace.funcregex.Match(bit.value);
                            if (m.Success)
                            {
                                if (m.Groups["Func"].Value == "ABILITYMOD")
                                    writer.WriteAttributeString("abilmod", "true");
                                writer.WriteAttributeString("statlink", m.Groups["Arg"].Value);
                            }
                            else
                                writer.WriteAttributeString("statlink", bit.value.Trim('+'));
                        }
                    }
                    var realval = calc(bit, workspace.Level);
                    if (SaveFileVersion > Serializer.SFVersion.v007b && realval.ToString() != bit.value)
                        writer.WriteAttributeString("calcvalue", realval.ToString());
                    writer.WriteEndElement( );
                }
                writer.WriteEndElement( );
            }
        }
        
    }
}
