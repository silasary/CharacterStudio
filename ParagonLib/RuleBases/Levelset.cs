using ParagonLib.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.RuleBases
{
    public class GeneratedLevelset : RulesElement
    {
        public GeneratedLevelset(string system)
        {
            this.system = system;
            List<Expression> grants = new List<Expression>();
            foreach (var level in new Search(system, "Level", null, null, null).Results().OrderBy(n => int.Parse(n.Name)))
            {
                var Parameters = new Dictionary<string, string>();
                Parameters.Add("name", level.InternalId);
                Parameters.Add("Level", level.Name);
                Parameters.Add("type", "Level");
                grants.Add(Instruction.Generate("grant", Parameters, null, 0));
            }
            Calculate = Builders.Lambda(grants.ToArray()).Compile();
        }
    }
}
