using Kamahl.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ParagonLib.Compiler;
using ParagonLib.RuleBases;

namespace ParagonLib
{
    public static class RuleFactory
    {
        private static ConcurrentBag<Task> LoadingThreads = new ConcurrentBag<Task>();
        private static ConcurrentQueue<XDocument> FilesToRegen = new ConcurrentQueue<XDocument>();
        internal static ConcurrentDictionary<string, RulesElement> Rules;
        private static List<string> knownSystems = new List<string>();
        internal static ConcurrentDictionary<string, RulesElement> RulesBySystem;
        private static Queue<XElement> UpdateQueue = new Queue<XElement>();
        private static Thread UpdateThread;
        private static AutoResetEvent WaitFileLoaded = new AutoResetEvent(false);
        public static readonly string BaseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio");
        public static readonly string RulesFolder = Path.Combine(BaseFolder, "Rules");
        public static readonly string SettingsFolder = Path.Combine(BaseFolder, "Settings");
        public static readonly string PicturesFolder = Path.Combine(BaseFolder, "Character Portraits");

        static RuleFactory()
        {
            Rules = new ConcurrentDictionary<string, RulesElement>();
            RulesBySystem = new ConcurrentDictionary<string, RulesElement>();
            Directory.CreateDirectory(RulesFolder);
            FileLoaded += (e) => WaitFileLoaded.Set();
            try
            {
                var DefRules = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DefaultRules");
                if (Directory.Exists(DefRules))
                {
                    foreach (var item in Directory.EnumerateFiles(DefRules, "*.index", SearchOption.AllDirectories))
                    {
                        var dest = RulesFolder + item.Substring(DefRules.Length);
                        if (!File.Exists(dest))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(dest));
                            File.Copy(item, dest);
                        }
                    }
                }
                DefRules = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DefaultPortraits");
                if (Directory.Exists(DefRules))
                {
                    foreach (var item in Directory.EnumerateFiles(DefRules, "*.index", SearchOption.AllDirectories))
                    {
                        var dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Character Portraits")+ item.Substring(DefRules.Length);
                        if (!File.Exists(dest))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(dest));
                            File.Copy(item, dest);
                        }
                    }
                }
            }
            catch (Exception c)
            {
                Logging.Log("Crashlog", TraceEventType.Error, "Loading Default Rules", c);
            }
            ThreadPool.QueueUserWorkItem(LoadRulesFolder, RulesFolder);
            ThreadPool.QueueUserWorkItem(LoadRulesFolder, SettingsFolder);
            ThreadPool.QueueUserWorkItem(LoadRulesFolder, PicturesFolder);

            Validate = true;
        }

        public delegate void FileLoadedEventHandler(string Filename);

        public static event FileLoadedEventHandler FileLoaded;
        private static bool runningBackgroundRegen;

        public static IEnumerable<object> KnownSystems
        {
            get
            {
                return knownSystems.ToArray();
            }
        }

        public static bool Loading
        {
            get
            {
                lock (LoadingThreads)
                {
                    Task peek;
                    LoadingThreads.TryTake(out peek);
                    if (peek != null && !peek.IsCompleted)
                        LoadingThreads.Add(peek); // Put it back again.
                }
                return LoadingThreads.ToArray().All(n => n.IsCompleted);
            }
        }

        public static bool Validate { get; set; }
        [Obsolete]
        public static void Load(XDocument doc)
        {
            LoadPart(doc, null);
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
                re = GetRule(id, System, setting);
            return re;
        }

        private static Search Search(string System, string Type, string Category, string Default)
        {
            return new Search(System, Type, Category, Default, null);
        }

        private static RulesElement GenerateLevelset(string System)
        {
            return new GeneratedLevelset(System);
        }

        private static RulesElement GetRule(string id, string System, CampaignSetting setting)
        {
            if (setting != null)
            {
                if (setting.CustomRules.Value.ContainsKey(id))
                    return setting.CustomRules.Value[id];
            }
            if (Loading)
            {
                WaitFileLoaded.WaitOne(1000);
                if (Rules.ContainsKey(id))
                {
                    return Rules[id];
                }
                if (setting != null && setting.CustomRules.Value.ContainsKey(id))
                    return setting.CustomRules.Value[id];
            }

            return null;
        }

        internal static void LoadFile(string file, string fallback, Dictionary<string, RulesElement> setting = null)
        {
            if (!File.Exists(file))
            {
                var xml = new WebClient().DownloadString(Uri(fallback, file));
                File.WriteAllText(file, xml);
            }
            LoadFile(file, setting);
        }

        internal static void LoadFile(string file, Dictionary<string, RulesElement> setting = null)
        {
            string ext = Path.GetExtension(file);
            if (!new string[] { ".part", ".index", ".setting" }.Contains(ext))
                return;
            try
            {
                XDocument doc = XDocument.Load(file, LoadOptions.SetLineInfo);
                doc.Root.Add(new XAttribute("Filename", file));
                string system = null;
                if (doc.Root.Attribute("game-system") != null)
                {
                    system = doc.Root.Attribute("game-system").Value;
                    if (!knownSystems.Contains(system))
                        knownSystems.Add(system);
                }
                else
                    Logging.LogIf(ext == ".part", TraceEventType.Critical, "Xml Validation", "{0} does not have a defined system.", file);
                
                var UpdateInfo = doc.Root.Element("UpdateInfo");
                if (UpdateInfo != null)
                {
                    UpdateInfo.Add(new XAttribute("filename", file));
                    QueueUpdate(UpdateInfo);
                }

                if (ext == ".part")
#if ASYNC
                    LoadingThreads.Add(Task.Factory.StartNew(() => LoadPart(doc, setting)));
#else
                    LoadPart(doc, setting);
#endif
                if (ext == ".index")
                    LoadIndex(file, doc);
                
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
                XmlReader reader = XmlReader.Create(file);
                try{
                    Logging.Log("Xml Loader", TraceEventType.Information, "Attempting to Recover.");
                    string name;
                    do
                    {
                        reader.Read( );
                        name = reader.LocalName;
                    } while(name != "PartAddress");
                    reader.Read( );
                    string value = reader.Value;
                    reader.Close( );
                    new WebClient().DownloadFile(value, file);
                }
                catch (XmlException)
                {
                    reader.Close( );
                    Logging.Log("Xml Loader", TraceEventType.Warning, "Cannot recover. Deleting file.");
                    if (File.Exists(file + ".borked"))
                        File.Delete(file + ".borked");
                    File.Move(file, file + ".borked");
                }
                catch (WebException c)
                {
                    Logging.Log("Xml Loader", TraceEventType.Warning, "Failed Recovery: {0}", c);
                }
            }
        }

        private static void LoadPart(XDocument doc, Dictionary<string, RulesElement> setting)
        {
            System.Reflection.Assembly code;
            var success = AssemblyGenerator.TryLoadDll(out code, doc);
            if (!success && code == null) // We completely failed
               code = AssemblyGenerator.CompileToDll(doc, false);
            else if (!success) // It worked, but we want to regen anyway.  Probably because ParagonLib updated.
            {
                FilesToRegen.Enqueue(doc);
                if (!runningBackgroundRegen)
                {
                    runningBackgroundRegen = true;
                    Task.Factory.StartNew(RegenLoop);
                }
            }
            try
            {
                
                foreach (var t in code.GetTypes())
                {
                    var rule = (RulesElement)Activator.CreateInstance(t);
                    if (setting != null)
                        setting[rule.InternalId] = rule;
                    else
                    {

                        Rules[rule.InternalId] = rule;
                        if (!String.IsNullOrEmpty(rule.GameSystem))
                            RulesBySystem[String.Format("{0}+{1}", rule.GameSystem, rule.InternalId)] = rule;
                    }
                }
            }
            catch (System.Reflection.ReflectionTypeLoadException c)
            {
                File.WriteAllText(c.Types[0].Assembly.Location + ".regen", c.LoaderExceptions[0].Message);
                LoadPart(doc, setting);
            }
            var system = doc.Root.Attribute("game-system").Value;
            if (!knownSystems.Contains(system))
                knownSystems.Add(system); 
        }

        private static void RegenLoop()
        {
            XDocument doc;
            while (FilesToRegen.TryDequeue(out doc))
                AssemblyGenerator.CompileToDll(doc, true);
            runningBackgroundRegen = false;
        }

        private static void LoadIndex(string file, XDocument doc)
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
                    try
                    {
                        var uri = Uri(n.Element("PartAddress").Value, newfile);
                        Logging.Log("Index Loader", TraceEventType.Information, "{0}: Getting {1} from {2}", Path.GetFileName(file), n.Element("Filename").Value, uri);
                        var xml = new WebClient().DownloadString(uri);
                        File.WriteAllText(newfile, xml);
                        LoadFile(newfile);
                    }
                    catch (WebException c)
                    {
                        Logging.Log("Index Loader", TraceEventType.Warning, "{0}: Failed getting {1} from index. {2}", Path.GetFileName(file), newfile, c);
                    }
                }
            }
            foreach (var n in doc.Root.Elements("File"))
            {
                string newfile;
                if (!File.Exists(newfile = Path.Combine(Path.GetDirectoryName(file), n.Element("Filename").Value)))
                {
                    try
                    {
                        var uri = Uri(n.Element("FileAddress").Value, newfile);
                        Logging.Log("Index Loader", TraceEventType.Information, "{0}: Getting {1} from {2}", Path.GetFileName(file), n.Element("Filename").Value, uri);
                        new WebClient().DownloadFile(uri, newfile);
                    }
                    catch (WebException c)
                    {
                        Logging.Log("Index Loader", TraceEventType.Warning, "{0}: Failed getting {1} from index. {2}", Path.GetFileName(file), newfile, c);
                    }
                }
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
            //while (Loading)
            //    Thread.Sleep(0);
            foreach (var file in Directory.EnumerateFiles(RulesFolder, "*", SearchOption.AllDirectories))
            {
                LoadFile(file);
                FileLoaded(file);
            }
            GC.Collect(1, GCCollectionMode.Forced);
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
            return new Uri(p.Replace("^", Path.GetFileNameWithoutExtension(filename)));
        }

        private static void ValidateRules(object state)
        {
            for (int n = 0; n < Rules.Count; n++)
            {
                Thread.Sleep(0);
                KeyValuePair<string, RulesElement> item;

                item = Rules.ElementAt(n);
                var CSV_Specifics = new string[] { "Racial Traits", "_SupportsID" };
                //foreach (var spec in CSV_Specifics)
                //{
                //    if (item.Value.Specifics.ContainsKey(spec) && !String.IsNullOrWhiteSpace(item.Value.Specifics[spec].FirstOrDefault()))
                //    {
                //        var errors = item.Value.Specifics[spec].FirstOrDefault().Split(',').Select((v) => v.Trim()).Select((i) => new KeyValuePair<string, RulesElement>(i, FindRulesElement(i, item.Value.System))).Where(p => p.Value == null || p.Value.System != item.Value.System);
                //        foreach (var e in errors)
                //        {
                //            if (e.Value == null)
                //                Logging.Log("Xml Validation", TraceEventType.Error, "{0} not found.", e.Key);
                //            else
                //                Logging.Log("Xml Validation", TraceEventType.Warning, "{0} does not exist in {1}. Falling back to {2}", e.Key, item.Value.System, e.Value.System);
                //        }
                //    }
                //}
            }
        }
    }
}