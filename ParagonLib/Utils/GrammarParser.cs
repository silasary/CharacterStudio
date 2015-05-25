using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerLine = ParagonLib.RuleBases.Power.PowerLine;

namespace ParagonLib.Utils
{
    class GrammarParser
    {
        internal static void ParsePowerLines(PowerLine[] lines)
        {
            var AttackLine = lines.FirstOrDefault(n => n.Name == "Attack");
            if (AttackLine.Name != null)
            {
                //var tokens = Tokenize(AttackLine.Value);
            }
        
        }

    }
}
