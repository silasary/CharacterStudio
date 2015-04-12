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

        public string type;

        public string source;

        public string system;

        public string internalId;

        protected string[] category;

        public string[] Category
        {
            get { return category; }
        }

        public Action<CharElement, Workspace> Calculate { get; set; }
    }
}

