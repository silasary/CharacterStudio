using System;
using System.Collections.Generic;

namespace ParagonLib
{
    public class CharElement
    {
        public List<CharElement> Children = new List<CharElement>();

        public CharElement(string id, int p, Workspace workspace, RulesElement re)
        {
            RulesElementId = id;
            SelfId = p;
            this.workspace = workspace;
            this.RulesElement = re;
            workspace.AllElements[id] = new WeakReference(this);
        }

        public WeakReference Parent { get; set; }

        public string RulesElementId { get; private set; }

        public int SelfId { get; set; }

        public Workspace workspace { get; set; }

        internal RulesElement RulesElement { get; set; }

        public void Grant(string InternalId, string type, string requires, string Level)
        {
            if (this.Children.Find(e => e.RulesElementId == InternalId) != null)
                return;
            var child = RuleFactory.New(InternalId, workspace, type);
            this.Children.Add(child);
            child.Parent = new WeakReference(this);
        }

        public void Select(string name, string number, string type, string requires, string Level)
        {
            throw new NotImplementedException();
        }

        internal void Recalculate()
        {
            foreach (var rule in this.RulesElement.Rules)
            {
                rule.Calculate(this, this.workspace);
                foreach (var child in this.Children) // If this throws an error because the array changed, something is wrong.
                {                                   //  Don't change this method, fix the cause.
                    child.Recalculate();
                }
            }
        }
    }
}