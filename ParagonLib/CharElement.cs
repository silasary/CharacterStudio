using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ParagonLib
{
    public class CharElement
    {
        public enum AquistitionMethod { Unknown, Granted, Selected}
        public List<CharElement> Children = new List<CharElement>();
        public Dictionary<string, Selection> Choices = new Dictionary<string, Selection>();

        public AquistitionMethod Method;

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
            CharElement child;
            if ((child = this.Children.Find(e => e.RulesElementId == InternalId)) != null)
            {
                if (child.Parent == null || !child.Parent.IsAlive) 
                    child.Parent = new WeakReference(this);
                child.Method = AquistitionMethod.Granted;
                return;
            }
            child = RuleFactory.New(InternalId, workspace, type);
            child.Method = AquistitionMethod.Granted;
            this.Children.Add(child);
            child.Parent = new WeakReference(this);
        }

        public void Select(string category, string number, string type, string requires, string optional, string Level, string Default)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(number), "<select> requires a number.");
            var num = workspace.ParseInt(number);
            for (int i = 0; i < num; i++)
            {
                var hash = String.Format("{0}{1}:{2}", category, type, number);
                Selection sel;
                if (!Choices.ContainsKey(hash))
                    sel = this.Choices[hash] = new Selection();
                else
                    sel = this.Choices[hash];
                sel.workspace = workspace;
                sel.Category = category;
                sel.Type = type;
                sel.Requires = requires;
                sel.Level = int.Parse(Level??"0"); //Let's not use Workspace here.
                sel.Default = Default;
                sel.Recalculate();
            }
        }

        internal void Recalculate()
        {
            if (this.RulesElement == null)
                this.RulesElement = RuleFactory.FindRulesElement(RulesElementId, workspace.System);
            if (this.RulesElement == null)
            {
                Logging.Log("PartLoader", TraceEventType.Error, "{0} could not be loaded.", RulesElementId);
                return;
            }
            foreach (var rule in this.RulesElement.Rules)
            {
                rule.Calculate(this, this.workspace);
            }
            foreach (var child in this.Children) // If this throws an error because the array changed, something is wrong.
            {                                   //  Don't change this method, fix the cause.
                child.Recalculate();
            }
        }
    }
}