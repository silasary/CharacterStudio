﻿using NUnit.Framework;
using ParagonLib;
using ParagonLib.Compiler;
using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
#pragma warning disable CS0612

namespace UnitTests
{
    class TestStats
    {
        [Test]
        public void TestStatAdd()
        {
            Workspace ws = new Workspace("Test", null);
            CharElement ce = new CharElement("test_statadd", 0, ws, null);
            var param = new System.Collections.Generic.Dictionary<string, string>();
            param.Add("name", "Speed");
            param.Add("value", "+5");
            var inst = new Instruction("statadd", param);
            inst.Calculate(ce, ws);
            Debug.Assert(ws.GetStat("Speed").Value == 5);
        }

        [Test]
        public void TestStatAlias()
        {
            Workspace ws = new Workspace("Test", null);
            CharElement ce = new CharElement("test_statalias", 0, ws, null);
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

        [Test]
        public void TestStatRecursion()
        {
            RuleFactory.Load(new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "Test"),
     new XElement(XName.Get("RulesElement"),
         new XAttribute("name", "Languages"),
         new XAttribute("type", "Test"),
         new XAttribute("internal-id", "TEST_STAT_RECURSION"),
         new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("To test that stats can reference stats.")),
         new XElement(XName.Get("rules"),
             new XElement(XName.Get("statadd"), new XAttribute("name", "Intelligence modifier"), new XAttribute("value","3")),
             new XElement(XName.Get("statadd"), new XAttribute("name", "Languages Known"), new XAttribute("value", "+1")), 
             new XElement(XName.Get("statadd"), new XAttribute("name", "Languages Known"), new XAttribute("value", "+Intelligence modifier")),
             new XElement(XName.Get("select"), new XAttribute("Category", "LANGUAGE"), new XAttribute("type", "Test"), new XAttribute("number", "Languages Known"))
             )
         ))));

            var ws = new Workspace("Test", null);
            var ele = RuleFactory.New("TEST_STAT_RECURSION", ws);
            ws.Recalculate();
            Debug.Assert(ws.GetStat("Languages Known").Value == 4); // 1 + int mod.

            GC.KeepAlive(ele);
        }
        [Test]
        public void TestStatTypes()
        {
            RuleFactory.Load(new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "Test"),
     new XElement(XName.Get("RulesElement"),
         new XAttribute("name", "Languages"),
         new XAttribute("type", "Test"),
         new XAttribute("internal-id", "TEST_STAT_TYPES"),
         new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("To test that stats can reference stats.")),
         new XElement(XName.Get("rules"),
             new XElement(XName.Get("statadd"), new XAttribute("name", "Buff"), new XAttribute("value", "3")),
             new XElement(XName.Get("statadd"), new XAttribute("name", "Buff"), new XAttribute("value", "+1"), new XAttribute("type", "Feat")),
             new XElement(XName.Get("statadd"), new XAttribute("name", "Buff"), new XAttribute("value", "+2"), new XAttribute("type", "Feat"))
             )
         ))));

            var ws = new Workspace("Test", null);
            RuleFactory.New("TEST_STAT_TYPES", ws); //This is very fickle, and risks the wrath of the GC.  But it should be safe for the lifespan of the test.
            ws.Recalculate();
            Assert.AreEqual(ws.GetStat("Buff").Value, 5);
        }

        [Test]
        public void TestStatFunctions()
        {
            RuleFactory.Load(new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "Test"),
     new XElement(XName.Get("RulesElement"),
         new XAttribute("name", "Languages"),
         new XAttribute("type", "Test"),
         new XAttribute("internal-id", "TEST_STAT_FUNC"),
         new XElement(XName.Get("specific"), new XAttribute("name", "Purpose"), new XText("To test that stats can reference stats.")),
         new XElement(XName.Get("rules"),
             new XElement(XName.Get("statadd"), new XAttribute("name", "int"), new XAttribute("value", "+16")),
             new XElement(XName.Get("statadd"), new XAttribute("name", "int mod"), new XAttribute("value", "+ABILITYMOD(int)"))
             )
         ))));

            var ws = new Workspace("Test", null);
            RuleFactory.New("TEST_STAT_FUNC", ws);
            ws.Recalculate();
            var Int = ws.GetStat("int");
            var IntMod = ws.GetStat("int mod");
            Assert.AreEqual((Int.Value - 10) / 2, IntMod.Value);

        }
    }
}

#pragma warning restore CS0612