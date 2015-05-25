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
            GrammarParser.ParsePowerLines(new ParagonLib.RuleBases.Power.PowerLine("Attack", "Strength vs AC"));
        }
    }
}
