using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RulesDb
{
    class D20RuleDb
    {
        Dictionary<string, D20RulesElement> elements = new Dictionary<string, D20RulesElement>();
        internal D20RulesElement GetOrAddRule(string Name, string Type, string InternalId, string Source)
        {
            lock (this)
            {
                if (!elements.ContainsKey(InternalId))
                {
                    return elements[InternalId] = new D20RulesElement(Name, Type, InternalId, Source);
                }
            }
            return elements[InternalId];
        }

        internal D20RulesElement GetRule(string InternalId)
        {
            return elements[InternalId];
        }
    }
}
