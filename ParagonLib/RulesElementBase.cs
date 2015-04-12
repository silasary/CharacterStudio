using System;

namespace ParagonLib
{
    public abstract class RulesElementBase
    {
        public RulesElementBase()
        {
            var m = this.GetType().GetMethod("Calculate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (m != null)
                Calculate = (Action<CharElement, Workspace>)m.CreateDelegate(typeof(Action<CharElement, Workspace>));
        }

        protected string name;

        public string Name
        {
            get { return name; }
        }

        protected string type;

        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        protected string source;

        public string Source
        {
            get { return source; }
            set { source = value; }
        }

        public string system;

        protected string internalId;

        public string InternalId
        {
            get { return internalId; }
            set { internalId = value; }
        }

        protected string[] category;

        public string[] Category
        {
            get { return category; }
        }

        public Action<CharElement, Workspace> Calculate { get; set; }
    }
}

