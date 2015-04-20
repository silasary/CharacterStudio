using ParagonLib.RuleBases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParagonLib
{
    public partial class Workspace
    {
        internal Dictionary<string, Stat> Stats = new Dictionary<string, Stat>(StringComparer.CurrentCultureIgnoreCase);
        Regex funcregex = new Regex(@"(?<Func>[A-Z]+)\((?<Arg>[a-z A-Z0-9]*)\)");
        private Dictionary<string, Func<string,string, int>> ParserFunctions;
        public Dictionary<string, Selection> Choices = new Dictionary<string, Selection>();

        public static readonly string[] D20AbilityScores = new string[] { "Strength", "Constitution", "Dexterity", "Intelligence", "Wisdom", "Charisma" };

        internal CharElement Levelset;

        public IEnumerable<Selection> Selections(params string[] Types)
        {
            foreach (var c in Choices)
            {
                if (Types.Length == 0 || Types.Contains(c.Value.Type,StringComparer.CurrentCultureIgnoreCase))
                    yield return c.Value;
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
        public Dictionary<string, Loot> AllLoot { get; set; }

        public int Level
        {
            get
            {
                Levelset.RulesElement.Calculate(Levelset, this);
                return (Levelset.RulesElement as RuleBases.GeneratedLevelset).CurrentLevel;
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
            foreach (var abil in D20AbilityScores)
            {
                if (CharacterRef == null) //TODO: Fix the Unit Tests to use Characters.
                    break; // HACK: If this isn't from a Unit Test, something's horribly wrong.
                GetStat(abil).Add(CharacterRef.AbilityScores[abil].ToString(), "", "", "", "0", null);
            }
            foreach (var adventure in AdventureLog)
            {
                adventure.XPStart = GetStat("XP Earned").Value;
                GetStat("XP Earned").Add(adventure.XPGain.ToString(), null, null, null, null,null);
                adventure.LevelAtEnd = Level;
                if (adventure.LootDiff != null)
                {
                    foreach (var loot in adventure.LootDiff)
                    {
                        //TODO DO THINGS HERE!!!
                    }
                }
            }
#if DEBUG //HACK:  REWRITE YOUR UNIT TESTS!
            foreach (var item in AllElements.Values.ToArray())
            {
                CharElement el;
                if ((item.Target != null) && !((el = (CharElement)item.Target).Parent != null && el.Parent.IsAlive) && el.RulesElement != null)
                {
                    if (el.RulesElement.Type == "Test")
                        el.Recalculate();
                }
            }
#endif
            Levelset.Recalculate();

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
        public partial class Stat
        {
            private List<bit> bits = new List<bit>();
            private List<string> Aliases = new List<string>();
            private Workspace workspace;

            public Stat(Workspace workspace, string Alias)
            {
                this.workspace = workspace;
                this.Aliases.Add(Alias);
            }
            public int Value
            {
                get
                {
                    return ValueAt((workspace.Levelset.RulesElement as GeneratedLevelset).CurrentLevel);
                }
            }

            public int ValueAt(int Level)
            {
                int total = 0;
                DefaultDictionary<string, int> TypeBonus = new DefaultDictionary<string, int>();
                foreach (var bit in bits)
                {
                    int val = calc(bit, Level);
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

            private int calc(Stat.bit bit, int Level)
            {
                int val;
                if (bit.Level > Level)
                    val = 0;
                else if (!workspace.MeetsRequirement(bit.requires))
                    val = 0;
                else
                    val = calc(bit.value);
                return val;
            }

            private bool Dirty { get; set; }

            public void Add(string value, string condition, string requires, string type, string Level, CharElement charelem)
            { // TODO: Support wearing= at some point
                bits.Add(new bit(value, condition, requires, type, string.IsNullOrEmpty(Level) ? workspace.Level : int.Parse(Level), charelem));
                this.Dirty = true;
            }

            public void AddText(string value, string condition, string requires, string Level, CharElement charelem)
            {
                bits.Add(new bit(value, condition, requires, string.IsNullOrEmpty(Level) ? workspace.Level : int.Parse(Level), charelem));
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
                public string conditional;
                public int Level;
                public string requires;
                public string type;
                public string value;
                public string String;
                private int charelem;

                public bit(string value, string condition, string requires, string type, int Level, CharElement source)
                {
                    this.value = value;
                    this.conditional = condition;
                    this.requires = requires;
                    this.type = type;
                    this.Level = Level;
                    this.String = "";
                    if (source == null)
                        this.charelem = 0;
                    else
                        this.charelem = source.SelfId;
                }

                public bit(string text, string condition, string requires, int Level, CharElement source)
                {
                    this.String = text;
                    this.conditional = condition;
                    this.requires = requires;
                    this.type = "";
                    this.Level = Level;
                    this.value = "";
                    if (source == null)
                        this.charelem = 0;
                    else
                        this.charelem = source.SelfId;
                }
                
            }

            public string[] String { get {
                return StringAt(workspace.Level);
            } }

            public string[] StringAt(int Level)
            {
                List<string> val = new List<string>();
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

        IEnumerable<int> counter() { var i = 1; while (true) yield return i++; }
        internal int GenerateUID()
        {
            var ids = AllElements.Where(i => i.Value.IsAlive)
                                 .Select(i => (i.Value.Target as CharElement).SelfId)
                                 .ToArray();
            return counter().First(i => !ids.Contains(i));

        }

        public CampaignSetting Setting { get; set; }

        internal bool MeetsRequirement(string p)
        {
            if (string.IsNullOrEmpty(p))
                return true;
            var negate = p.StartsWith("!");
            if (negate)
                p = p.Substring(1);
            var success = AllElements.Where(n => n.Value.IsAlive).Select(n => n.Value.Target as CharElement).FirstOrDefault(n => n.Name == p) != null;
            if (negate)
                return !success;
            else
                return success;

        }

        internal Search Search(string Type, string Category, string Default)
        {
            return new Search(System, Type, Category, Default, this);
        }
    }
}