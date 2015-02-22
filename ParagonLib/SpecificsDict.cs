using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ParagonLib
{
    public class SpecificsDict : Dictionary<string, List<string>>
	{
        public void Add(string key, string val)
        {
            if (!this.ContainsKey(key))
                this[key] = new List<string>();
            this[key].Add(val);
        }
	}

}