using System;
using System.Collections.Generic;
using System.Linq;

namespace ParagonLib
{
    public class Workspace
    {
        private Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();

        public Workspace()
        {
            AllElements = new Dictionary<string, WeakReference>();
            AdventureLog = new List<Adventure>();
        }

        public List<Adventure> AdventureLog { get; set; }

        public Dictionary<string, WeakReference> AllElements { get; set; }

        protected int level;

        public int Level
        {
            get
            {
                level = 1;
                var earned = GetStat("XP Earned");
                var needed = GetStat("XP Needed");
                while (earned.ValueAt(level) >= needed.ValueAt(level) && (needed.ValueAt(level) != needed.ValueAt(level - 1)))
                    level++;
                return level;
            }
        }

        public string System { get; set; }

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
            foreach (var adventure in AdventureLog)
            {
                GetStat("XP Earned").Add(adventure.XpEarned.ToString(), null, null, null, null);
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
                    p = p.Substring(1);
                    break;

                case '-':
                    plus = false;
                    p = p.Substring(1);
                    break;

                default:
                    //throw new ArgumentOutOfRangeException(String.Format("'{0}' is not a valid sign. Full string: {1}", p[0], p));
                    plus = true;
                    break;
            }
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
                    return ValueAt(workspace.level);
                }
            }

            public int ValueAt(int Level)
            {
                int total = 0;
                foreach (var bit in bits)
                {
                    if (bit.Level > Level)
                        continue;
                    int val = calc(bit.value);
                    total += val;
                    //Todo: Type.
                }
                return total;
            }



            private bool Dirty { get; set; }

            public void Add(string value, string condition, string requires, string type, string Level)
            {
                bits.Add(new bit(value, condition, requires, type, String.IsNullOrEmpty(Level) ? workspace.Level : int.Parse(Level)));
                this.Dirty = true;
            }

            public void Reset()
            {
                this.bits.Clear();
                this.Dirty = true;
            }

            private int calc(string p)
            {
                //TODO: Stat references. eg: <statadd name="Fortitude Defense" value="+Great Fortitude" type="Feat" />
                return workspace.ParseInt(p);
            }

            private struct bit
            {
                public string condition;
                public int Level;
                public string requires;
                public string type;
                public string value;

                public bit(string value, string condition, string requires, string type, int Level)
                {
                    this.value = value;
                    this.condition = condition;
                    this.requires = requires;
                    this.type = type;
                    this.Level = Level;
                }
            }
        }
    }
}