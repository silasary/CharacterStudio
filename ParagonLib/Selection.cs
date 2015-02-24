using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class Selection
    {
        private CharElement Parent;

        public Selection(CharElement parent)
        {
            // TODO: Complete member initialization
            this.Parent = parent;
        }
        public string Category { get; set; }

        public int Level { get; set; }

        public string Requires { get; set; }

        public string Type { get; set; }

        public string Default { get; set; }
        
        public Workspace workspace { get; set; }

        internal void Recalculate()
        {
            if (workspace.Level < Level)
            {
                Value = "";
                return;
            }
            Options = RuleFactory.Search(workspace.System, Type, Category, Default).Results();
            if (string.IsNullOrEmpty(Value))
            {
                Value = "";
                return;
            }
            var chosen = Options.Where(r => r.InternalId == Value);
            if (chosen.Count() == 0)
                Value = "";
            if (String.IsNullOrEmpty(Value))
            {
                Child = null;
            }
            else
            {
                if (Child == null)
                    Child = RuleFactory.New(Value, workspace, Type);


            }

        }

        public string Value { get; set; }
        [Obsolete]
        public CharElement Child { get; set; }

        public IEnumerable<RulesElement> Options { get; set; }
    }
}
