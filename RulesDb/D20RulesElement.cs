using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesDb
{
    public class D20RulesElement
    {
        public string Name { get; }
        public string Type { get; }
        public string InternalId { get; }
        public string Source { get; }
        public string[] Categories { get; private set; }
        public string Flavor { get; private set; }
        public Dictionary<string, string> Specifcs { get; } = new Dictionary<string, string>();

        public D20RulesElement(string Name, string Type, string InternalId, string Source)
        {
            this.Name = Name;
            this.Type = Type;
            this.InternalId = InternalId;
            this.Source = Source;
        }

        internal void SetCategories(string p)
        {
            this.Categories = p?.Split(',').Select(s => s.Trim()).ToArray();
        }

        internal void SetFlavor(string p)
        {
            this.Flavor = p;
        }

        internal void SetSpecific(string p1, string p2)
        {
            this.Specifcs[p1] = p2;
        }
    }
}
