using System;
using System.Collections.Generic;
using System.Linq;

namespace ParagonLib
{
    public class Workspace
    {
        private Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();
        public Dictionary<string, WeakReference> AllElements { get; set; }
        public string System { get; set; }

        public Workspace()
        {
            AllElements = new Dictionary<string, WeakReference>();
        }

        public void AliasStat(string Stat, string Alias)
        {
            if (Stats.ContainsKey(Stat) && Stats.ContainsKey(Alias))
                throw new NotImplementedException();
            if (!Stats.ContainsKey(Stat))
                Stats[Stat] = new Stat(this);
            Stats[Alias] = Stats[Stat];
        }

        //TODO: Make this better.
        public Workspace.Stat GetStat(string name)
        {
            if (!Stats.ContainsKey(name))
                Stats[name] = new Stat(this);
            return Stats[name];
        }

        public void Recalculate(bool block = true) //TODO: Default should be false.
        {
            if (!block)
                throw new NotImplementedException(); //TODO: Do the below, but asyncronously.
            foreach (var stat in Stats)
            {
                stat.Value.Reset();
            }
            foreach (var item in AllElements.Values.ToArray())
            {
                CharElement el;
                if ((item.Target != null) && !((el = (CharElement)item.Target).Parent != null && el.Parent.IsAlive))
                {
                    el.Recalculate();
                }
            }
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

        public class Stat
        {
            private List<bit> bits = new List<bit>();

            private Workspace workspace;

            public Stat(Workspace workspace)
            {
                this.workspace = workspace;
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

            private bool Dirty { get; set; }

            public void Add(string value, string condition, string requires, string type)
            {
                bits.Add(new bit(value, condition, requires, type));
                this.Dirty = true;
            }

            private int calc(string p)
            {
                //TODO: Stat references. eg: <statadd name="Fortitude Defense" value="+Great Fortitude" type="Feat" />
                return workspace.ParseInt(p);
            }

            public void Reset()
            {
                this.bits.Clear();
                this.Dirty = true;
            }

            private struct bit
            {
                public string condition;
                public string requires;
                public string type;
                public string value;

                public bit(string value, string condition, string requires, string type)
                {
                    this.value = value;
                    this.condition = condition;
                    this.requires = requires;
                    this.type = type;
                }
            }
        }
    }
}