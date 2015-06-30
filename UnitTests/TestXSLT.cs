using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace UnitTests
{
    public class TestXSLT
    {
        [Test]
        [TestCaseSource(typeof(FullTests), "Parts")]
        public void TestXSLTs(string part)
        {
            Directory.CreateDirectory("Visualizations");
            var vis = new ParagonLib.Visualizers.PowerVisualizer();
            XDocument Part = XDocument.Load(part, LoadOptions.SetLineInfo);
            var xd = new XmlDocument();
            foreach (var re in Part.Root.Elements("RulesElement"))
            {
                if (re.Attribute("type").Value != "Power")
                    continue;
                xd.Load(re.CreateReader());
                var output = vis.RulesElementToHtml(xd);
                File.WriteAllText(Path.Combine("Visualizations", re.Attribute("name").Value + ".html"), output);
            }
        }
    }
}
