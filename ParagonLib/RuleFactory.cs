using Kamahl.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        internal static ConcurrentDictionary<string, RulesElement> Rules;
        private static List<string> knownSystems = new List<string>();
        private static int num = 0;
        private static ConcurrentDictionary<string, RulesElement> RulesBySystem;
        private static Queue<XElement> UpdateQueue = new Queue<XElement>();
        private static Thread UpdateThread;
        private static AutoResetEvent WaitFileLoaded = new AutoResetEvent(false);
        public static readonly string RulesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Rules");
        public static readonly string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Settings");

        static RuleFactory()
        {
            Rules = new ConcurrentDictionary<string, RulesElement>();
            RulesBySystem = new ConcurrentDictionary<string, RulesElement>();
            Directory.CreateDirectory(RulesFolder);
            FileLoaded += (e) => WaitFileLoaded.Set();
            ThreadPool.QueueUserWorkItem(LoadRulesFolder, RulesFolder);
            ThreadPool.QueueUserWorkItem(LoadRulesFolder, SettingsFolder);
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
            var el = new CharElement(id, workspace.GenerateUID(), workspace, re);

            return el;
        }

        internal static RulesElement FindRulesElement(string id, string System, CampaignSetting setting = null)
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
                re = GetRule(id, setting);
            return re;
        }

        internal static Search Search(string System, string Type, string Category, string Default)
        {
            return new Search(System, Type, Category, Default);
        }

        private static RulesElement GenerateLevelset(string System)
        {
            var levelset = new RulesElement(null) { Type = "Levelset", System = System, Name = "LEVELSET", InternalId = "_LEVELSET_", Source = "Internal" };
            foreach (var level in Search(System, "Level", null, null).Results().OrderBy(n => int.Parse(n.Name)))
            {
                var Parameters = new Dictionary<string, string>();
                Parameters.Add("name", level.InternalId);
                Parameters.Add("Level", level.Name);
                Parameters.Add("type", "Level");
                levelset.Rules.Add(new Instruction("grant", Parameters));
            }
            return levelset;
        }

        [Obsolete]
        private static int GenerateUID()
        {
            return num++;
        }

        private static void Load(XElement item)
        {
            try
            {
                var re = new RulesElement(item);
                Rules[re.InternalId] = re;
                if (!String.IsNullOrEmpty(re.System))
                    RulesBySystem[String.Format("{0}+{1}", re.System, re.InternalId)] = re;
            }
            catch (XmlException c)
            {
                Logging.Log("Xml Loader", TraceEventType.Error, "Failed to load {0}. {1}", item.Attribute("internal-id").Value, c.Message);
                System.Diagnostics.Debug.WriteLine("Failed to load {0}. {1}", item.Attribute("internal-id").Value, c.Message);
            }
        }

        private static RulesElement GetRule(string id, CampaignSetting setting)
        {
            if (setting != null)
            {
                //setting.CustomRules
            }
            while (Loading)
            {
                WaitFileLoaded.WaitOne(50);
                if (Rules.ContainsKey(id))
                    return Rules[id];
            }
            return null;
        }

        internal static void LoadFile(string file, string fallback)
        {
            if (!File.Exists(file))
            {
                var xml = new WebClient().DownloadString(Uri(fallback, file));
                File.WriteAllText(file, xml);
            }
            LoadFile(file);
        }

        internal static void LoadFile(string file)
        {
            string ext = Path.GetExtension(file);
            if (!new string[] { ".part", ".index", ".setting" }.Contains(ext))
                return;
            try
            {
                XDocument doc = XDocument.Load(file);
                string system = null;
                if (doc.Root.Attribute("game-system") != null)
                {
                    system = doc.Root.Attribute("game-system").Value;
                    if (!knownSystems.Contains(system))
                        knownSystems.Add(system);
                }
                else
                    Logging.LogIf(ext == ".part", TraceEventType.Critical, "Xml Validation", "{0} does not have a defined system.", file);

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
                    foreach (var n in doc.Root.Elements("Obsolete"))
                    {
                        File.Delete(Path.Combine(Path.GetDirectoryName(file), n.Element("Filename").Value));
                    }
                    foreach (var n in doc.Root.Elements("Part"))
                    {
                        string newfile;
                        if (!File.Exists(newfile = Path.Combine(Path.GetDirectoryName(file), n.Element("Filename").Value)))
                        {
                            Logging.Log("Updater", TraceEventType.Information, "Getting {0} from {1}", n.Element("Filename").Value, Path.GetFileName(file));
                            var xml = new WebClient().DownloadString(Uri(n.Element("PartAddress").Value, newfile));
                            File.WriteAllText(newfile, xml);
                            LoadFile(newfile);
                        }
                    }
                }
                if (ext == ".setting")
                {
                    if (doc.Root.Attribute("name") == null)
                    {
                        doc.Root.Add(new XAttribute("name", Path.GetFileNameWithoutExtension(file)));
                    }
                    CampaignSetting.ImportSetting(doc);
                }
            }
            catch (XmlException v)
            {
                Logging.Log("Xml Loader", TraceEventType.Error, "Failed to load {0}. {1}", file, v.Message);
                System.Diagnostics.Debug.WriteLine("Failed to load {0}. {1}", file, v.Message);
            }
        }

        private static void LoadRulesFolder(object RulesFolderString)
        {
            LoadRulesFolder((string)RulesFolderString);
        }

        private static void LoadRulesFolder(string RulesFolder)
        {
            if (!Directory.Exists(RulesFolder))
                return;
            while (Loading)
                Thread.Sleep(0);
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
            WebClient wc = new WebClient();
            while (UpdateQueue.Count > 0)
            {
                var UpdateInfo = UpdateQueue.Dequeue();
                var file = UpdateInfo.Attribute("filename").Value;
                try
                {
                    string filename = UpdateInfo.Element("Filename") != null ? UpdateInfo.Element("Filename").Value : Path.GetFileName(file);
                    try
                    {
                        Version verLocal = new Version(UpdateInfo.Element("Version").Value);
                        Version verRemote = new Version(wc.DownloadString(Uri(UpdateInfo.Element("VersionAddress").Value, filename)));
                        if (verRemote > verLocal)
                        {
                            Logging.Log("Updater", TraceEventType.Information, "Updating {0} to {1}", filename, verRemote);
                            string newfile;
                            var xml = wc.DownloadString(Uri(UpdateInfo.Element("PartAddress").Value, filename));
                            XDocument upd = XDocument.Parse(xml);
                            Version verUpdated = new Version(upd.Root.Element("UpdateInfo").Element("Version").Value);
                            if (verUpdated != verRemote) // Someone screwed up.  Probably Adam again.
                            {
                                upd.Root.Element("UpdateInfo").Element("Version").Value = verRemote.ToString();
                                xml = upd.ToString(SaveOptions.None);
                            }
                            File.WriteAllText(newfile = Path.Combine(Path.GetDirectoryName(file), filename), xml);
                            LoadFile(newfile);
                        }
                    }
                    catch (ArgumentException)
                    {
                        Logging.Log("Updater", TraceEventType.Warning, "Invalid Version number '{0}' in file {1}.", UpdateInfo.Element("Version").Value, Path.GetFileName(file));
                        string verLocal = UpdateInfo.Element("Version").Value;
                        string verRemote = wc.DownloadString(Uri(UpdateInfo.Element("VersionAddress").Value, filename));
                        if (verRemote != verLocal)
                        {
                            Logging.Log("Updater", TraceEventType.Information, "Updating {0} to {1}", filename, verRemote);
                            string newfile;
                            var xml = wc.DownloadString(Uri(UpdateInfo.Element("PartAddress").Value, filename));
                            XDocument upd = XDocument.Parse(xml);
                            string verUpdated = upd.Root.Element("UpdateInfo").Element("Version").Value;
                            if (verUpdated != verRemote) // Someone screwed up.  Probably Adam again.
                            {
                                upd.Root.Element("UpdateInfo").Element("Version").Value = verRemote;
                                xml = upd.ToString(SaveOptions.None);
                            }
                            File.WriteAllText(newfile = Path.Combine(Path.GetDirectoryName(file), filename), xml);
                            LoadFile(newfile);
                        }
                    }
                }
                catch (XmlException v)
                { Logging.Log("Updater", TraceEventType.Warning, "Error updating {0}: {1}", Path.GetFileName(file), v.ToString()); }
                catch (WebException v)
                { Logging.Log("Updater", TraceEventType.Warning, "Error updating {0}: {1}", Path.GetFileName(file), v.ToString()); }
            }
        }

        private static Uri Uri(string p, string filename)
        {
            return new Uri(p.Replace("^", filename));
        }

        private static void ValidateRules(object state)
        {
            for (int n = 0; n < Rules.Count; n++)
            {
                Thread.Sleep(0);
                KeyValuePair<string, RulesElement> item;

                item = Rules.ElementAt(n);
                var CSV_Specifics = new string[] { "Racial Traits", "_SupportsID" };
                foreach (var spec in CSV_Specifics)
                {
                    if (item.Value.Specifics.ContainsKey(spec) && !String.IsNullOrWhiteSpace(item.Value.Specifics[spec].FirstOrDefault()))
                    {
                        var errors = item.Value.Specifics[spec].FirstOrDefault().Split(',').Select((v) => v.Trim()).Select((i) => new KeyValuePair<string, RulesElement>(i, FindRulesElement(i, item.Value.System))).Where(p => p.Value == null || p.Value.System != item.Value.System);
                        foreach (var e in errors)
                        {
                            if (e.Value == null)
                                Logging.Log("Xml Validation", TraceEventType.Error, "{0} not found.", e.Key);
                            else
                                Logging.Log("Xml Validation", TraceEventType.Warning, "{0} does not exist in {1}. Falling back to {2}", e.Key, item.Value.System, e.Value.System);
                        }
                    }
                }
                foreach (var rule in item.Value.Rules)
                {
                    if (rule.Validate == null)
                        continue;
                    var error = rule.Validate();
                    if (error != null)
                        Logging.Log("Xml Validation", TraceEventType.Error, error);

                }
            }
        }
    }
}