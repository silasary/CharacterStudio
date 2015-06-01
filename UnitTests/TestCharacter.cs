using System;
using ParagonLib;
using System.Xml.Linq;
using System.Diagnostics;
using NUnit.Framework;
using System.IO;
using ParagonLib.RuleEngine;
using ParagonLib.CharacterData;

namespace UnitTests
{
    public class TestCharacter
    {
        [Test]
        public void TestLeveling()
        {
            var elements =
new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "Test"),
    //new XElement(XName.Get("RulesElement"),
    //    new XAttribute("name", "TestRules"),
    //    new XAttribute("type", "Ruleset"),
    //    new XAttribute("internal-id", "TEST_RULESET"),
    //    new XElement(XName.Get("rules"),
    //        new XElement(XName.Get("grant"), new XAttribute("name", "TEST_LEVEL_1"), new XAttribute("type", "Level"), new XAttribute("Level", "1")),
    //        new XElement(XName.Get("grant"), new XAttribute("name", "TEST_LEVEL_2"), new XAttribute("type", "Level"), new XAttribute("Level", "2"))
    //        )
    //    ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "1"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_1"),
        new XElement("specific", new XAttribute("name", "XP Needed"), 50),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "2"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_2"),
        new XElement("specific", new XAttribute("name", "XP Needed"), 50),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
        new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "3"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_3"),
        new XElement("specific", new XAttribute("name", "XP Needed"), 50),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "4"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_4"),
        new XElement("specific", new XAttribute("name", "XP Needed"), 50),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "5"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_5"),
        new XElement("specific", new XAttribute("name", "XP Needed"), 50),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        )
        ));
            RuleFactory.Load(elements);
            var c = new Character("Test");
            var ws = c.workspace;
            //var ruleset = RuleFactory.New("TEST_RULESET", ws); // This should be implicit at some point.
            ws.Recalculate();            
            Debug.Assert(ws.Level == 1); // Start at level 1.
            c.workspace.AdventureLog.Add(new Adventure() { XPGain = 70 });
            ws.Recalculate();
            Debug.Assert(ws.Level == 2); // We earned 70, which is 50+leftovers. Expect 2.
            c.workspace.AdventureLog.Add(new Adventure() { XPGain = 30 });
            ws.Recalculate(); 
            Debug.Assert(ws.Level == 3); // Earned 30, which brings us to a total of 100. Exactly level 3.
            c.workspace.AdventureLog.Add(new Adventure() { XPGain = 120 });
            ws.Recalculate();
            Debug.Assert(ws.Level == 5); // Earned 120, which brings us to a total of 220. Level 5.
            //GC.KeepAlive(ruleset);
            
        }

        //[Test]
        public void TestSave()
        {
            var elements =
new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "TestSave"),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "1"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_1"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50")),
            new XElement(XName.Get("statadd"), new XAttribute("name", "Strength"), new XAttribute("value", "12")),
            new XElement(XName.Get("statadd"), new XAttribute("name", "Constitution"), new XAttribute("value", "15")),
            new XElement(XName.Get("statalias"), new XAttribute("name", "Strength"), new XAttribute("alias", "str")),
            new XElement(XName.Get("statadd"), new XAttribute("name", "str mod"), new XAttribute("value", "+ABILITYMOD(str)"))
        )        
    ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "2"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_2"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        )

));
            RuleFactory.Load(elements);
            var c = new Character("TestSave");
            var ws = c.workspace;
            c.workspace.AdventureLog.Add(new Adventure() { XPGain = 70 });
            ws.Recalculate();
            c.Save("Test.D20Character");
            File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Saved Characters", "Test.D20Character"), "./Test.D20Character", true);
        }
    }
}
