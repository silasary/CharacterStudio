using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using System.Xml;

namespace ParagonLib
{
    public static class RuleFactory
    {
        private static int num = 0;
        private static Dictionary<string, RulesElement> Rules;
        private static Dictionary<string, RulesElement> RulesBySystem;
        static AutoResetEvent WaitFileLoaded = new AutoResetEvent(false);
        
        public delegate void FileLoadedEventHandler(string Filename);
        public static event FileLoadedEventHandler FileLoaded;
        static RuleFactory()
        {
            Rules = new Dictionary<string, RulesElement>();
            RulesBySystem = new Dictionary<string, RulesElement>();
            var RulesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Rules");
            Directory.CreateDirectory(RulesFolder);
            FileLoaded += (e) => WaitFileLoaded.Set();
            ThreadPool.QueueUserWorkItem(LoadRulesFolder, RulesFolder);
            Validate = true;
            
        }

        private static void LoadRulesFolder(object RulesFolderString)
        {
            LoadRulesFolder((string)RulesFolderString);
        }

        private static void LoadRulesFolder(string RulesFolder)
        {
            Loading = true;
            foreach (var file in Directory.EnumerateFiles(RulesFolder, "*", SearchOption.AllDirectories))
            {
                LoadFile(file);
                FileLoaded(file);
            }
            Loading = false;
            if (Validate)
                ThreadPool.QueueUserWorkItem(ValidateRules);
        }

        private static void ValidateRules(object state)
        {
            foreach (var item in Rules)
            {
                var CSV_Specifics = new string[] { "Racial Traits" };
                foreach (var spec in CSV_Specifics)
                {
                    if (item.Value.Specifics.ContainsKey(spec))
                    {
                        var errors = item.Value.Specifics[spec].Split(',').Select((v) => v.Trim()).Select((i) => new KeyValuePair<string, RulesElement>(i, FindRulesElement(i, item.Value.System))).Where(p => p.Value == null || p.Value.System != item.Value.System);
                        foreach (var e in errors)
                        {
                            if (e.Value == null)
                                Console.WriteLine("ERROR: {0} not found.", e.Key);
                            else
                                Console.WriteLine("WARNING: {0} does not exist in {1}. Falling back to {2}", e.Key, item.Value.System, e.Value.System);
                        }
                    }
                }



            }
        }

        public static CharElement New(string id, Workspace workspace, string type = null)
        {
            RulesElement re = FindRulesElement(id, workspace.System);
            var el = new CharElement(id, GenerateUID(), workspace, re);

            return el;
        }

        internal static RulesElement FindRulesElement(string id, string System)
        {
            if (id == "_LEVELSET_")
                return GenerateLevelset(System);
            var sysid = String.Format("{0}+{1}", System, id);
            RulesElement re;
            if (RulesBySystem.ContainsKey(sysid))
                re = RulesBySystem[sysid];
            else if (Rules.ContainsKey(id))
                re = Rules[id];
            else
                re = Load(id);
            return re;
        }

        private static RulesElement GenerateLevelset(string System)
        {
            var levelset = new RulesElement(null) { Type = "Levelset", System = System, Name = "LEVELSET", InternalId = "_LEVELSET_", Source = "Internal" };
            foreach (var level in Search(System, "Level", null))
            {
                var Parameters = new Dictionary<string, string>();
                Parameters.Add("name", level.InternalId);
                Parameters.Add("Level", level.Name);
                Parameters.Add("type", "Level");
                levelset.Rules.Add(new Instruction("grant", Parameters));
            }
            return levelset;

        }

        private static int GenerateUID()
        {
            return num++;
        }

        private static void Load(XElement item)
        {
            var re = new RulesElement(item);
            Rules[re.InternalId] = re;
            if (!String.IsNullOrEmpty(re.System))
                RulesBySystem[String.Format("{0}+{1}", re.System, re.InternalId)] = re;

        }

        private static RulesElement Load(string id)
        {
            while (Loading)
            {
                WaitFileLoaded.WaitOne(1000);
                if (Rules.ContainsKey(id))
                    return Rules[id];

            }
            return null;
        }

        private static void LoadFile(string file)
        {
            if (Path.GetExtension(file) == ".part")
            {
                try
                {
                    XDocument doc = XDocument.Load(file);
                    var system = doc.Root.Attribute("game-system").Value;
                    if (!knownSystems.Contains(system))
                        knownSystems.Add(system);
                    foreach (var item in doc.Root.Descendants(XName.Get("RulesElement")))
                    {
                        Load(item);
                    }
                    var UpdateInfo = doc.Root.Element("UpdateInfo");
                    if (UpdateInfo != null)
                    {


                    }
                }
                catch (XmlException v)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to load {0}. {1}", file, v.Message);
                }
            }
        }

        public static bool Loading { get; set; }

        public static void Load(XDocument doc)
        {
            foreach (var item in doc.Root.Elements())
            {
                Load(item);
            }
        }

        internal static IEnumerable<RulesElement> Search(string System, string Type, string Category)
        {
            var Categories = Category == null ? new string[0] : Category.Split(',');
            var catCount = Categories.Count();
            var Comparer = new CategoryComparer();
            foreach (var item in Rules.Values.ToArray())
            {
                if (String.IsNullOrEmpty(Type) || item.Type == Type)
                {
                    if (String.IsNullOrEmpty(System) || String.IsNullOrEmpty(item.System) || item.System == System)
                    {
                        if (Categories.Intersect(item.Category, Comparer).Count() == catCount)
                        {
                            yield return item;
                        }
                    }
                }
            }
            yield break;
        }

        public static bool Validate { get; set; }

        private static List<string> knownSystems = new List<string>();

        public static IEnumerable<object> KnownSystems
        {
            get
            {
                return knownSystems.ToArray();
            }
        }
    }
}