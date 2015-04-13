using System;

namespace ParagonLib.RuleBases
{
    public abstract class RulesElementBase
    {
        protected string prereqs;

        private string printPrereqs;

        public string PrintPrereqs
        {
            get { return printPrereqs; }
            set { printPrereqs = value; }
        }


        public string Prereqs
        {
            get { return prereqs; }
            set { prereqs = value; }
        }


        public string[] Category
        {
            get { return category; }
        }

        public string GameSystem
        {
            get { return system; }
        }

        public string InternalId
        {
            get { return internalId; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Source
        {
            get { return source; }
        }

        public string Type
        {
            get { return type; }
        }

        public readonly Action<CharElement, Workspace> Calculate;

        protected string[] category;

        protected string internalId;

        protected string name;

        protected string source;

        protected string system;

        protected string type;

        public RulesElementBase()
        {
            // This is a nasty hack.  But considering that Expression Trees can't compile non-static methods into a TypeBuilder, we have no alternative.
            var m = this.GetType().GetMethod("Calculate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (m != null)
                Calculate = (Action<CharElement, Workspace>)m.CreateDelegate(typeof(Action<CharElement, Workspace>));
        }
    }
}