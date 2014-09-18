using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class Workspace
    {
        public class Stat
        {
            private struct bit
            {
                public string value;
                public string condition;
                public string requires;
                public string type;

                public bit(string value, string condition, string requires, string type)
                {
                    this.value = value;
                    this.condition = condition;
                    this.requires = requires;
                    this.type = type;
                }

            }

            private Workspace workspace;

            public Stat(Workspace workspace)
            {
                this.workspace = workspace;
            }
            
            bool Dirty { get; set; }

            List<bit> bits = new List<bit>();

            public void Add(string value, string condition, string requires, string type)
            {
                bits.Add(new bit(value, condition, requires, type));
            }

            public int Value
            {
                get
                {
                    int total = 0;
                    foreach (var bit in bits)
                    {
                        int val = calc(bit.value);
                        total += val;
                        //Todo: Type.
                    }
                    return total;
                }
            }

            private int calc(string p)
            {
                //TODO: Stat references. eg: <statadd name="Fortitude Defense" value="+Great Fortitude" type="Feat" />
                return workspace.ParseInt(p);
            }
        }

        public string System { get; set; } //TODO: Make this better.

        Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();

        public Workspace.Stat GetStat(string name)
        {
            if (!Stats.ContainsKey(name))
                Stats[name] = new Stat(this);
            return Stats[name];
        }

        public void AliasStat(string Stat, string Alias)
        {
            if (Stats.ContainsKey(Stat) && Stats.ContainsKey(Alias))
                throw new NotImplementedException();
            if (!Stats.ContainsKey(Stat))
                Stats[Stat] = new Stat(this);
            Stats[Alias] = Stats[Stat];
        }

        internal int ParseInt(string p)
        {
            bool plus;
            switch (p[0])
            {
                case '+':
                    plus = true;
                    break;
                case '-':
                    plus = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(String.Format("'{0}' is not a valid sign. Full string: {1}", p[0], p));
            }
            p = p.Substring(1);
            return (plus ? 1 : -1) * int.Parse(p);
        }
    }
}