using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ParagonLib
{
    public partial class Workspace
    {
        internal Dictionary<string, Stat> Stats = new Dictionary<string, Stat>();
        Regex funcregex = new Regex(@"(?<Func>[A-Z]+)\((?<Arg>[a-z A-Z0-9]*)\)");
        private Dictionary<string, Func<string,string, int>> ParserFunctions;

        internal CharElement Levelset;

        public IEnumerable<Selection> Selections(params string[] Types)
        {
            foreach (var item in AllElements)
            {
                var choices = (item.Value.Target as CharElement).Choices;
                foreach (var c in choices)
                {
                    if (Types.Length == 0 || Types.Contains(c.Value.Type,StringComparer.CurrentCultureIgnoreCase))
                        yield return c.Value;
                }
            }
            yield break;
        }

        public Character CharacterRef { get; private set; }

        public Workspace(string System, Character character)
        {
            this.System = System;
            if (System != "" && !RuleFactory.KnownSystems.Contains(System))
                Logging.Log("Load Character", TraceEventType.Warning,"Warning: Requested system '{0}' not loaded.", System);
            AllElements = new Dictionary<string, WeakReference>();
            AdventureLog = new List<Adventure>();
            ParserFunctions = new Dictionary<string, Func<string,string, int>>();
            ParserFunctions["ABILITYMOD"] = (p,q) => { return (ParseInt(p) - 10 ) / 2; };
            ParserFunctions["HALF"] = (p, q) => { return ParseInt(p) / 2; };
            if (!String.IsNullOrEmpty(System))
                Levelset = RuleFactory.New("_LEVELSET_", this);
            CharacterRef = character;
            
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

        public string System { get; private set; }

        public void AliasStat(string Stat, string Alias)
        {
            if (Stats.ContainsKey(Stat) && Stats.ContainsKey(Alias))
            {
                if (Stats[Stat] == Stats[Alias]) // We've already done the Alias.  Let's not get into a massive headache.
                    return;
                throw new InvalidOperationException("You can't Alias into an existing stat.");
            }
            if (!Stats.ContainsKey(Stat))
                Stats[Stat] = new Stat(this, Stat);
            Stats[Alias] = Stats[Stat];
            Stats[Alias].AddAlias(Alias);
        }

        //TODO: Make this better.
        public Workspace.Stat GetStat(string name)
        {
            if (!Stats.ContainsKey(name))
                Stats[name] = new Stat(this, name);
            return Stats[name];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string[] GetStatString(string name)
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
                adventure.StartXp = GetStat("XP Earned").Value;
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
            if (string.IsNullOrEmpty(p))
                return 0;
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
        [DataContract]
        [KnownType(typeof(bit))]
        public partial class Stat
        {
            [DataMember]
            private List<bit> bits = new List<bit>();
            private List<string> Aliases = new List<string>();
            private Workspace workspace;

            public Stat(Workspace workspace, string Alias)
            {
                this.workspace = workspace;
                this.Aliases.Add(Alias);
            }
            [DataMember, XmlAttribute]
            public int Value
            {
                get
                {
                    return ValueAt(workspace.level);
                }
                private set { } // Exists for the serializer.
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
            [DataContract]
            private struct bit
            {
                [DataMember(IsRequired=false), XmlAttribute]
                public string condition;
                [DataMember(IsRequired = false), XmlAttribute]
                public int Level;
                [DataMember(IsRequired = false), XmlAttribute]
                public string requires;
                [DataMember(IsRequired = false), XmlAttribute]
                public string type;
                [DataMember(IsRequired = false), XmlAttribute]
                public string value;
                [DataMember(IsRequired = false), XmlAttribute]
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

            internal void AddAlias(string Alias)
            {
                this.Aliases.Add(Alias);
            }
        }

        IEnumerable<int> counter() { var i = 0; while (true) yield return i++; }
        internal int GenerateUID()
        {
            var ids = AllElements.Where(i => i.Value.IsAlive)
                                 .Select(i => (i.Value.Target as CharElement).SelfId)
                                 .ToArray();
            return counter().First(i => !ids.Contains(i));

        }

        public CampaignSetting Setting { get; set; }
    }
}