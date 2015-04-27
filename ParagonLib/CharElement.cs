using ParagonLib.RuleBases;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ParagonLib
{
    public class CharElement
    {
        public enum AquistitionMethod { Unknown, Granted, Selected}
        public List<CharElement> Children = new List<CharElement>();

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
            int level;
            int.TryParse(Level, out level);
            bool disabled = false;
            //if (!workspace.MeetsRequirement(requires) || workspace.Level < level)
            //    disabled = true; // TODO: Do these somewhere else.
            CharElement child;
            if ((child = this.Children.Find(e => e.RulesElementId == name)) != null)
            {
                if (child.Parent == null || !child.Parent.IsAlive) 
                    child.Parent = new WeakReference(this);
                child.Method = AquistitionMethod.Granted;
                child.Disabled = disabled;
                return;
            }

            child = RuleFactory.New(name, workspace, type);
            child.Method = AquistitionMethod.Granted;
            this.Children.Add(child);
            child.Parent = new WeakReference(this);
            child.Disabled = disabled;

        }

        public void Select(string name, string category, string number, string type, string requires, string Level, string optional, string Default)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(number), "<select> requires a number.");
            var num = workspace.ParseInt(number);
            for (int i = 0; i < num; i++)
            {
                var hash = String.Format("{0}_Select_{1}:{2},{3}", RulesElementId, type, number,i);
                Selection sel;
                if (!workspace.Choices.ContainsKey(hash))
                    sel = workspace.Choices[hash] = new Selection(parent: this);
                else
                    sel = workspace.Choices[hash];
                sel.workspace = workspace;
                sel.Category = category;
                sel.Type = type;
                sel.Requires = requires;
                sel.Level = int.Parse(Level??"0"); //Let's not use Workspace here.
                sel.Default = Default;
                if (string.IsNullOrEmpty(name))
                    sel.Name = hash;
                else
                    sel.Name = name;
                sel.Recalculate();
            }
        }

        public void Replace(string power_replace, string optional, string Level, string retrain)
        {
            //TODO: Fill me in!
        }

        public void Modify(string name, string type, string Field, string value, string requires)
        {
            //TODO: Fill me in!
        }

        public void Drop()
        {
            //TODO: Drop()
        }
        internal void Recalculate()
        {
            if (this.SelfId == -1)
                SelfId = workspace.GenerateUID();
            if (Disabled)
                return; 
            if (this.RulesElement == null)
                this.RulesElement = RuleFactory.FindRulesElement(RulesElementId, workspace.System);
            if (this.RulesElement == null)
            {
                Logging.Log("PartLoader", TraceEventType.Error, "{0} could not be loaded.", RulesElementId);
                return;
            }
            try
            {
                if (this.RulesElement.Calculate != null)
                    this.RulesElement.Calculate(this, workspace);
            }
            catch (Exception c)
            {
                Logging.Log("Crashlog", TraceEventType.Critical, c.ToString());
            }
            foreach (var child in this.Children) // If this throws an error because the array changed, something is wrong.
            {                                   //  Don't change this method, fix the cause.
                child.Recalculate();
            }
        }

        public bool Disabled { get; set; }
    }
}