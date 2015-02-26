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

        private string cached_name;
        public string Name
        {
            get
            {
                if (RulesElement != null && string.IsNullOrEmpty(cached_name))
                    cached_name = RulesElement.Name;
                return cached_name;
            }
            set
            {
                cached_name = value; // Custom Name or Serializer.
            }
        }

        private string cached_type;
        public string Type
        {
            get
            {
                if (RulesElement != null && string.IsNullOrEmpty(cached_type))
                    cached_type = RulesElement.Type;
                return cached_type;
            }
            set
            {
                cached_type = value; // Serializer.
            }
        }

        public int SelfId { get; set; }

        public Workspace workspace { get; set; }

        internal RulesElement RulesElement { get; set; }

        public void Grant(string name, string type, string requires, string Level)
        {
            CharElement child;
            if ((child = this.Children.Find(e => e.RulesElementId == name)) != null)
            {
                if (child.Parent == null || !child.Parent.IsAlive) 
                    child.Parent = new WeakReference(this);
                child.Method = AquistitionMethod.Granted;
                return;
            }
            child = RuleFactory.New(name, workspace, type);
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
                    sel = this.Choices[hash] = new Selection(parent: this);
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

        public void Replace(string power_replace, string optional)
        {
            //TODO: Fill me in!
        }

        public void Modify(string name, string Field, string value, string requires)
        {
            //TODO: Fill me in!
        }

        internal void Recalculate()
        {
            if (this.SelfId == -1)
                SelfId = workspace.GenerateUID();
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