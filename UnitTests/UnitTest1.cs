using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParagonLib;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestStatAdd()
        {
            Workspace ws = new Workspace();
            CharElement ce = new CharElement("test_statadd", 0,ws,null);
            var param =new System.Collections.Generic.Dictionary<string,string>();
            param.Add("name","Speed");
            param.Add("value", "+5");
            var inst= new Instruction("statadd", param);
            inst.Calculate(ce, ws);
            Debug.Assert(ws.GetStat("Speed").Value == 5);
        }

        [TestMethod]
        public void TestStatAlias()
        {
            Workspace ws = new Workspace();
            CharElement ce = new CharElement("test_statalias", 0,ws,null);
            var param = new System.Collections.Generic.Dictionary<string, string>();
            param.Add("name", "Strength");
            param.Add("value", "+12");
            var inst = new Instruction("statadd", param);
            inst.Calculate(ce, ws);
            var param2 = new System.Collections.Generic.Dictionary<string, string>();
            param2.Add("name", "Strength");
            param2.Add("alias", "str");
            var inst2 = new Instruction("statalias", param2);
            inst2.Calculate(ce, ws);
            Debug.Assert(ws.GetStat("str").Value == 12, "Expected 'str' to equal assigned value of 'Strength'");
        }

        [TestMethod]
        public void LoadRules()
        {
            Workspace ws = new Workspace();
            RuleFactory.New("ID_INTERNAL_LEVEL_1",ws);
        }

        [TestMethod]
        public void TestGrant()
        {
            var ws = new Workspace();
            var elements = 
                //===Start===
new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "Tests"),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "Granter"),
        new XAttribute("type", "Test"),
        new XAttribute("internal-id", "TEST_GRANT_GRANTER"),
        new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("To grant an Element")),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("grant"), new XAttribute("name", "TEST_GRANT_GRANTEE"), new XAttribute("type", "Test"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "Grantee"),
        new XAttribute("type", "Test"),
        new XAttribute("internal-id", "TEST_GRANT_GRANTEE"),
        new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("To be granted")),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "GRANTEE_VALUE"), new XAttribute("value", "+1"))
            )
        )
    ));
            //===END===
            RuleFactory.Load(elements);
            var charElement = RuleFactory.New("TEST_GRANT_GRANTER", ws);
            ws.Recalculate(true);
            Debug.Assert(ws.GetStat("GRANTEE_VALUE").Value == 1);
            ws.Recalculate(true);
            Debug.Assert(ws.GetStat("GRANTEE_VALUE").Value == 1);
            GC.KeepAlive(charElement);
        }

        [TestMethod]
        public void TestSelect()
        {
            var ws = new Workspace();
            var elements =
                //===Start===
new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "Tests"),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "Selector"),
        new XAttribute("type", "Test"),
        new XAttribute("internal-id", "TEST_SELECT_SELECTOR"),
        new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("To offer a choice of Elements")),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("select"), new XAttribute("category", "TEST_SELECT_SELECTOR"), new XAttribute("type", "Test"), new XAttribute("number", "1"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "Option A"),
        new XAttribute("type", "Test"),
        new XAttribute("internal-id", "TEST_SELECT_OPTIONA"),
        new XElement(XName.Get("Category"), "TEST_SELECT_SELECTOR"),
        new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("Select one of these.")),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "SELECTED_VALUE"), new XAttribute("value", "+1"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "Option B"),
        new XAttribute("type", "Test"),
        new XAttribute("internal-id", "TEST_SELECT_OPTIONB"),
        new XElement(XName.Get("Category"), "TEST_SELECT_SELECTOR"),
        new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("Select one of these.")),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "SELECTED_VALUE"), new XAttribute("value", "+2"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "Option C"),
        new XAttribute("type", "Test"),
        new XAttribute("internal-id", "TEST_SELECT_OPTIONC"),
        new XElement(XName.Get("Category"), "TEST_SELECT_SELECTOR"),
        new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("Select one of these.")),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "SELECTED_VALUE"), new XAttribute("value", "+3"))
            )
        )
    ));

            RuleFactory.Load(elements);
            var charElement = RuleFactory.New("TEST_SELECT_SELECTOR", ws);
            ws.Recalculate(true);
            Debug.Assert(charElement.Choices.Count == 1);
            var options = charElement.Choices.Values.First().Options;
            Debug.Assert(options.Count() == 3);
            GC.KeepAlive(charElement);

        }
    }
}
