using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ParagonLib.Rules;
using System.Xml.Xsl;
using System.Reflection;
using System.Xml;
using ParagonLib.RuleEngine;
using CharacterStudio.Rules;

namespace ParagonLib.Visualizers
{
    public class PowerVisualizer
    {
        XslCompiledTransform transform;
         
        public PowerVisualizer()
        {
            transform = new XslCompiledTransform(true);
            var sheet = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType(), "PowerConverter.xslt");
            transform.Load(XmlReader.Create(sheet));
        }

        public string ConvertToHtml(IPower power)
        {
            var rd = new XDocument(new XElement("RuleData", new XAttribute("name", power.Name), new XAttribute("internal-id", power.InternalId)));
            //typeof(Specifics).
            
            return null;
        }

        public string RulesElementToHtml(XmlDocument xml)
        {
            var output = new StringBuilder();
            var writer = XmlWriter.Create(output);
            transform.Transform(xml, writer);
            return output.ToString();
        }
    }
}
