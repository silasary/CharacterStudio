using Kamahl.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ParagonLib
{
    public static class RuleFactory
    {
        internal static Dictionary<string, RulesElement> Rules;
        private static List<string> knownSystems = new List<string>();
        private static int num = 0;
        private static Dictionary<string, RulesElement> RulesBySystem;
        private static Queue<XElement> UpdateQueue = new Queue<XElement>();
        private static Thread UpdateThread;
        private static AutoResetEvent WaitFileLoaded = new AutoResetEvent(false);

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

        public delegate void FileLoadedEventHandler(string Filename);

        public static event FileLoadedEventHandler FileLoaded;

        public static IEnumerable<object> KnownSystems
        {
            get
            {
                return knownSystems.ToArray();
            }
        }

        public static bool Loading { get; set; }

        public static bool Validate { get; set; }

        public static void Load(XDocument doc)
        {
            foreach (var item in doc.Root.Elements())
            {
                Load(item);
            }
            var system = doc.Root.Attribute("game-system").Value;
            if (!knownSystems.Contains(system))
                knownSystems.Add(system);
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
                re = GetRule(id);
            return re;
        }

        internal static Search Search(string System, string Type, string Category, string Default)
        {
            return new Search(System, Type, Category, Default);
        }

        private static RulesElement GenerateLevelset(string System)
        {
            var levelset = new RulesElement(null) { Type = "Levelset", System = System, Name = "LEVELSET", InternalId = "_LEVELSET_", Source = "Internal" };
            foreach (var level in Search(System, "Level", null, null).Results())
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

        private static RulesElement GetRule(string id)
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
            string ext = Path.GetExtension(file);
            if (!new string[] { ".part", ".index", ".setting" }.Contains(ext))
                return;
            try
            {
                XDocument doc = XDocument.Load(file);
                var system = doc.Root.Attribute("game-system").Value;
                if (!knownSystems.Contains(system))
                    knownSystems.Add(system);
                if (ext == ".part")
                    foreach (var item in doc.Root.Descendants(XName.Get("RulesElement")))
                    {
                        Load(item);
                    }
                var UpdateInfo = doc.Root.Element("UpdateInfo");
                if (UpdateInfo != null)
                {
                    UpdateInfo.Add(new XAttribute("filename", file));
                    QueueUpdate(UpdateInfo);
                }
                if (ext == ".index")
                {
                    foreach (var n in UpdateInfo.Elements("Obsolete"))
                    {
                        File.Delete(Path.Combine(Path.GetDirectoryName(file), n.Element("Filename").Value));
                    }
                    foreach (var n in UpdateInfo.Elements("Part"))
                    {
                        string newfile;
                        if (!File.Exists(newfile = Path.Combine(Path.GetDirectoryName(file), n.Element("Filename").Value)))
                        {
                            var xml = Singleton<WebClient>.Instance.DownloadString(Uri(n.Element("PartAddress").Value, newfile));
                            File.WriteAllText(newfile, xml);
                        }
                    }
                }
            }
            catch (XmlException v)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load {0}. {1}", file, v.Message);
            }
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

        private static void QueueUpdate(XElement UpdateInfo)
        {
            UpdateQueue.Enqueue(UpdateInfo);
            if (UpdateThread == null || !UpdateThread.IsAlive)
            {
                UpdateThread = new Thread(UpdateLoop) { IsBackground = true, Name = "UpdateLoop" };
                UpdateThread.Start();
            }
        }

        private static void UpdateLoop()
        {
            while (UpdateQueue.Count > 0)
            {
                var UpdateInfo = UpdateQueue.Dequeue();
                var file = UpdateInfo.Attribute("filename").Value;
                try
                {
                    string filename = UpdateInfo.Element("Filename") != null ? UpdateInfo.Element("Filename").Value : Path.GetFileName(file);
                    Version verLocal = new Version(UpdateInfo.Element("Version").Value);
                    Version verRemote = new Version(Singleton<WebClient>.Instance.DownloadString(Uri(UpdateInfo.Element("VersionAddress").Value, filename)));
                    if (verRemote > verLocal)
                    {
                        string newfile;
                        var xml = Singleton<WebClient>.Instance.DownloadString(Uri(UpdateInfo.Element("PartAddress").Value, filename));
                        File.WriteAllText(newfile = Path.Combine(Path.GetDirectoryName(file), filename), xml);
                        LoadFile(newfile);
                    }
                }
                catch (WebException v)
                { Logging.Log("Updater", "Error updating {0}: {1}", Path.GetFileName(file), v.ToString()); }
            }
        }

        private static Uri Uri(string p, string filename)
        {
            return new Uri(p.Replace("^", filename));
        }

        private static void ValidateRules(object state)
        {
            foreach (var item in Rules)
            {
                var CSV_Specifics = new string[] { "Racial Traits", "_SupportsID" };
                foreach (var spec in CSV_Specifics)
                {
                    if (item.Value.Specifics.ContainsKey(spec) && !String.IsNullOrWhiteSpace(item.Value.Specifics[spec]))
                    {
                        var errors = item.Value.Specifics[spec].Split(',').Select((v) => v.Trim()).Select((i) => new KeyValuePair<string, RulesElement>(i, FindRulesElement(i, item.Value.System))).Where(p => p.Value == null || p.Value.System != item.Value.System);
                        foreach (var e in errors)
                        {
                            if (e.Value == null)
                                Logging.Log("Xml Validation", "ERROR: {0} not found.", e.Key);
                            else
                                Logging.Log("Xml Validation", "WARNING: {0} does not exist in {1}. Falling back to {2}", e.Key, item.Value.System, e.Value.System);
                        }
                    }
                }
                foreach (var rule in item.Value.Rules)
                {
                    if (rule.Validate == null)
                        continue;
                    var error = rule.Validate();
                    if (error != null)
                        Logging.Log("Xml Validation", error);

                }
            }
        }
    }
}