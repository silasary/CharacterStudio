﻿using System;
using ParagonLib;
using System.Xml.Linq;
using System.Diagnostics;
using NUnit.Framework;

namespace UnitTests
{
    public class TestCharacter
    {
        [Test]
        public void TestLeveling()
        {
            var elements =
new XDocument(new XElement(XName.Get("D20Rules"), new XAttribute("game-system", "Tests"),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "TestRules"),
        new XAttribute("type", "Ruleset"),
        new XAttribute("internal-id", "TEST_RULESET"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("grant"), new XAttribute("name", "TEST_LEVEL_1"), new XAttribute("type", "Level"), new XAttribute("Level", "1")),
            new XElement(XName.Get("grant"), new XAttribute("name", "TEST_LEVEL_2"), new XAttribute("type", "Level"), new XAttribute("Level", "2"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "1"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_1"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "2"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_2"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
        new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "3"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_3"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "4"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_4"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        ),
    new XElement(XName.Get("RulesElement"),
        new XAttribute("name", "5"),
        new XAttribute("type", "Level"),
        new XAttribute("internal-id", "TEST_LEVEL_5"),
        new XElement(XName.Get("rules"),
            new XElement(XName.Get("statadd"), new XAttribute("name", "XP Needed"), new XAttribute("value", "50"))
            )
        )
        ));
            RuleFactory.Load(elements);
            var c = new Character();
            var ws = c.workspace;
            var ruleset = RuleFactory.New("TEST_RULESET", ws); // This should be implicit at some point.
            Debug.Assert(ws.Level == 1); // Start at level 1.
            c.workspace.AdventureLog.Add(new Adventure() { XpEarned = 70 });
            ws.Recalculate();
            Debug.Assert(ws.Level == 2); // We earned 70, which is 50+leftovers. Expect 2.
            c.workspace.AdventureLog.Add(new Adventure() { XpEarned = 30 });
            ws.Recalculate(); 
            Debug.Assert(ws.Level == 3); // Earned 30, which brings us to a total of 100. Exactly level 3.
            c.workspace.AdventureLog.Add(new Adventure() { XpEarned = 120 });
            ws.Recalculate();
            Debug.Assert(ws.Level == 5); // Earned 120, which brings us to a total of 220. Level 5.
            GC.KeepAlive(ruleset);
        }
    }
}