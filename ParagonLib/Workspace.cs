using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ParagonLib
{
    public class Workspace
    {
        private Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();
        Regex funcregex = new Regex(@"(?<Func>[A-Z]+)\((?<Arg>[a-z A-Z0-9]*)\)");
        private Dictionary<string, Func<string,string, int>> ParserFunctions;

        public Workspace()
        {
            AllElements = new Dictionary<string, WeakReference>();
            AdventureLog = new List<Adventure>();
            ParserFunctions = new Dictionary<string, Func<string,string, int>>();
            ParserFunctions["ABILITYMOD"] = (p,q) => { return (ParseInt(p) - 10 ) / 2; };
            ParserFunctions["HALF"] = (p, q) => { return ParseInt(p) / 2; };
            
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
                throw new InvalidOperationException("You can't Alias into an existing stat.");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string[] GetTextStringData(string name)
        {
            if (!Stats.ContainsKey(name))
                return new string[0];
            return Stats[name].String;
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
            int val;
            if (!int.TryParse(p, out val))
            {
                var m = funcregex.Match(p);
                if (m.Success)
                    val = ParserFunctions[m.Groups["Func"].Value](m.Groups["Arg"].Value,null);
                else    
                    val = GetStat(p).Value;
            }
            return (plus ? 1 : -1) * val;
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
                DefaultDictionary<string, int> TypeBonus = new DefaultDictionary<string, int>();
                foreach (var bit in bits)
                {
                    if (bit.Level > Level)
                        continue;
                    int val = calc(bit.value);
                    if (string.IsNullOrEmpty(bit.type))
                        total += val;
                    else if (TypeBonus[bit.type] < val)
                        TypeBonus[bit.type] = val;
                }
                foreach (var type in TypeBonus)
                {
                    total += type.Value;
                }
                return total;
            }

            private bool Dirty { get; set; }

            public void Add(string value, string condition, string requires, string type, string Level)
            {
                bits.Add(new bit(value, condition, requires, type, string.IsNullOrEmpty(Level) ? workspace.Level : int.Parse(Level)));
                this.Dirty = true;
            }

            public void AddText(string text, string condition, string requires, string Level)
            {
                bits.Add(new bit(text, condition, requires, string.IsNullOrEmpty(Level) ? workspace.Level : int.Parse(Level)));
                this.Dirty = true;
            }

            public void Reset()
            {
                this.bits.Clear();
                this.Dirty = true;
            }

            private int calc(string p)
            {
                return workspace.ParseInt(p);
            }

            private struct bit
            {
                public string condition;
                public int Level;
                public string requires;
                public string type;
                public string value;
                public string String;

                public bit(string value, string condition, string requires, string type, int Level)
                {
                    this.value = value;
                    this.condition = condition;
                    this.requires = requires;
                    this.type = type;
                    this.Level = Level;
                    this.String = "";
                }

                public bit(string text, string condition, string requires, int Level)
                {
                    this.String = text;
                    this.condition = condition;
                    this.requires = requires;
                    this.type = "";
                    this.Level = Level;
                    this.value = "";

                }
                
            }

            public string[] String { get {
                return StringAt(workspace.Level);
            } }

            public string[] StringAt(int Level)
            {
                List<string> val = new List<string>();
                DefaultDictionary<string, int> TypeBonus = new DefaultDictionary<string, int>();
                foreach (var bit in bits)
                {
                    if (bit.Level > Level)
                        continue;
                    if (!string.IsNullOrEmpty(bit.String))
                        val.Add(bit.String);
                }
                return val.ToArray();
            }
        }
    }
}