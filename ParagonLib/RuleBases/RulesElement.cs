using System;
using System.Runtime.CompilerServices;
using ParagonLib.RuleEngine;
using ParagonLib.Rules;

namespace ParagonLib.RuleBases
{
    public abstract class RulesElement : IRulesElement
    {
        public Action<CharElement, Workspace> Calculate { get; protected set; }

        [AccessedThroughProperty("_Text")]
        protected string _text;
        [AccessedThroughProperty("Category")]
        protected string[] category;
        [AccessedThroughProperty("Flavor")]
        protected string flavor;
        [AccessedThroughProperty("InternalId")]
        protected string internalId;
        [AccessedThroughProperty("Name")]
        protected string name;
        [AccessedThroughProperty("Prereqs")]
        protected string prereqs;
        [AccessedThroughProperty("PrintPrereqs")]
        protected string printPrereqs;
        [AccessedThroughProperty("ShortDescription")]
        protected string short_Description;
        [AccessedThroughProperty("Source")]
        protected string source;
        [AccessedThroughProperty("GameSystem")]
        protected string system;
        [AccessedThroughProperty("Type")]
        protected string type;

        public RulesElement()
        {
            // This is a nasty hack.  But considering that Expression Trees can't compile non-static methods into a TypeBuilder, we have no alternative.
            var m = this.GetType().GetMethod("Calculate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (m != null)
                Calculate = (Action<CharElement, Workspace>)m.CreateDelegate(typeof(Action<CharElement, Workspace>));
        }

        public string GetSpecific(Specifics specific)
        {
            return GetSpecific(specific.ToString());
        }
#region _GENERATED_

public string _Text {
    get {
        return this._text;
    }
}

public string[] Category {
    get {
        return this.category;
    }
}

public string Flavor {
    get {
        return this.flavor;
    }
}

public string InternalId {
    get {
        return this.internalId;
    }
}

public string Name {
    get {
        return this.name;
    }
}

public string Prereqs {
    get {
        return this.prereqs;
    }
}

public string PrintPrereqs {
    get {
        return this.printPrereqs;
    }
}

public string ShortDescription {
    get {
        return this.short_Description;
    }
}

public string Source {
    get {
        return this.source;
    }
}

public string GameSystem {
    get {
        return this.system;
    }
}

public string Type {
    get {
        return this.type;
    }
}

protected virtual string GetSpecific(string specific) {
    return null;
}
#endregion _GENERATED_
	}
}