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
            Options = workspace.Search(Type, Category, Default).Results();
            if (workspace.Setting != null)
                Options = Options.Where(n => workspace.Setting.IsRuleLegal(n));
            // TODO:  If Method = Grant, Set value ""
            if (string.IsNullOrEmpty(Value))
            {
                var guess = Parent.Children.Where(n => n.Method == CharElement.AquistitionMethod.Unknown).Select(n => n.RulesElementId).Intersect(Options.Select(r => r.InternalId)).FirstOrDefault( );
                if (!string.IsNullOrEmpty(guess))
                {
                    Value = guess;
                    Child = Parent.Children.Where(n => n.RulesElementId == guess).FirstOrDefault();
                    Child.Method = CharElement.AquistitionMethod.Selected;
                }
                else
                {
                    Value = "";
                    return;
                }
            }
            var chosen = Options.Where(r => r.InternalId == Value);
            if (chosen.Count() == 0)
                Value = "";
            if (String.IsNullOrEmpty(Value))
            {
                    Parent.Children.Remove(Child);
                    Child = null;
             }
            else
            {
                if (Child == null)
                {
                    Child = RuleFactory.New(Value, workspace, Type);
                    Child.Method = CharElement.AquistitionMethod.Selected;
                    Parent.Children.Add(Child);
                }
            }
        }

        public string Value { get; set; }
        public CharElement Child { get; set; }

        public IEnumerable<RulesElement> Options { get; set; }
    }
}
