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
        private Search search;

        public GeneratedLevelset(string system)
        {
            this.system = system;
            //List<Expression> grants = new List<Expression>();
            //foreach (var level in new Search(system, "Level", null, null, null).Results.OrderBy(n => int.Parse(n.Name)))
            //{
            //    var Parameters = new Dictionary<string, string>();
            //    Parameters.Add("name", level.InternalId);
            //    Parameters.Add("Level", level.Name);
            //    Parameters.Add("type", "Level");
            //    grants.Add(Instruction.Generate("grant", Parameters, null, 0,int.Parse(level.Name)));
            //}
            //Calculate = Builders.Lambda(grants.ToArray()).Compile();
            Calculate = Calc;
        }

        private void Calc(CharElement element, Workspace ws)
        {
            int level = 1;
            var earned = ws.GetStat("XP Earned");
            //var needed = ws.GetStat("XP Needed");
            ILevel lastlevel = null;
            if (search == null)
                search = ws.Search("Level", null, null);
            var AllLevels = search.Results;
            if (AllLevels.Count() == 0)
                return;
            while (true) //earned.ValueAt(level) >= needed.ValueAt(level) && (needed.ValueAt(level) != needed.ValueAt(level - 1)))
            {
                this.CurrentLevel = level;
                var l = AllLevels.FirstOrDefault(n => n.Name == level.ToString());
                if (l == null)
                    break;
                element.Grant(l.InternalId, "Level",null,level.ToString());
                var levelele = element.Children.FirstOrDefault(n => n.Name == level.ToString());
                var Level = (ILevel)levelele.RulesElement;
                if (Level == null)
                    break;
                Level.PreviousLevel = lastlevel;
                lastlevel = Level;
                var needed = Level.TotalXpNeeded;

                var levelup = earned.Value >= needed;

                if (levelup)
                    level++;
                else
                    break;
            }
        }

        public int CurrentLevel { get; private set; }
    }
}
