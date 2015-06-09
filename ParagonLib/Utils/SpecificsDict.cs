using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ParagonLib
{
    public class SpecificsDict : DefaultDictionary<Specifics, string>
	{
        public string this[string key]
        {
            get
            {
                return this[GetKey(key)];
            }
            set
            {
                this[GetKey(key)] = value;
            }
        }

        private Specifics GetKey(string key)
        {
            Specifics truekey;
            var success = Enum.TryParse(key.Replace(' ','_'),true, out truekey);
            if (success)
                return truekey;
            else
            {
                Logging.Log("PartLoader", System.Diagnostics.TraceEventType.Error, "Missing Specific Type '{0}'", key);
                return Specifics.Unknown;
            }
        }

        //// No longer needed: We dropped the support for multiple entries.
        //public void Add(string key, string val)
        //{
        //    if (!this.ContainsKey(key))
        //        this[key] = new List<string>();
        //    this[key].Add(val);
        //}
	}

}