using NUnit.Framework;
using ParagonLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    class TestGrammar
    {
        [Test]
        public void TestAttackLine()
        {
            // Below are a list of sample lines, in increasing complexity:
            string[] lines = {
             "Dexterity vs. AC",
             "Strength or Wisdom vs. Will",
             "Strength, Wisdom, or Charisma vs. Fortitude",
             "Strength vs. AC, Dexterity vs. AC, or Constitution vs. AC",
             "Strength -2 vs. AC",
             "Strength + 2 vs. AC, three attacks",
             "Strength + 2 vs. Reflex, Constitution + 2 vs. Reflex, or Dexterity + 2 vs. Reflex",
             "Your highest ability vs. AC",
             "Highest mental ability vs. Will",
             "Your highest physical ability vs AC",
             "Strength vs. AC (melee; main weapon and off-hand weapon) or Dexterity vs. AC (ranged), two attacks",
            };
            foreach (var item in lines)
            {
                GrammarParser.ParsePowerLines(new ParagonLib.RuleBases.Power.PowerLine("Attack", item));
            }
            
        }
    }
}
