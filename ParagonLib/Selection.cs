using ParagonLib.RuleBases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class Selection
    {
        private WeakReference<CharElement> parent;

        public CharElement Parent
        {
            get
            {
                CharElement o; 
                parent.TryGetTarget(out o);
                return o;
            }
            set
            {
                parent.SetTarget(value);
            }
        }

        public Selection(CharElement parent)
        {
            // TODO: Complete member initialization
            this.parent = new WeakReference<CharElement>(parent);
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
            Options = workspace.Search(Type, Category, Default);
            // TODO:  If Method = Grant, Set value ""
            if (string.IsNullOrEmpty(Value))
            {
                var guess = Parent.Children.Where(n => n.Method == CharElement.AquistitionMethod.Unknown).Select(n => n.RulesElementId).Intersect(Options.Results.Select(r => r.InternalId)).FirstOrDefault( );
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
            var chosen = Options.Results.Where(r => r.InternalId == Value);
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

        private string value;
        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
            }
        }
        public CharElement Child { get; set; }

        public Search Options { get; set; }

        public string Name { get; set; }
    }
}
