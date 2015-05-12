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
using SmartWeakEvent;
using ParagonLib.Utils;
using ParagonLib.Rules;

namespace ParagonLib.RuleEngine
{
    public static class RuleFactory
    {
        internal static ConcurrentDictionary<string, RulesElement> Rules = new ConcurrentDictionary<string, RulesElement>();
        internal static ConcurrentDictionary<string, RuleData> RulesBySystem = new ConcurrentDictionary<string, RuleData>();
        internal static ConcurrentDictionary<string, IFactory> RuleFactories = new ConcurrentDictionary<string, IFactory>();
        internal static Dictionary<string, Dictionary<string, CategoryInfo>> CategoriesBySystem = new Dictionary<string, Dictionary<string, CategoryInfo>>();
        //internal static ConcurrentDictionary<string, RuleData> RulesBySystem = new ConcurrentDictionary<string, RulesElement>();

        private static ConcurrentQueue<Task> LoadingThreads = new ConcurrentQueue<Task>();
        private static ConcurrentQueue<XDocument> FilesToRegen = new ConcurrentQueue<XDocument>();
        private static List<string> knownSystems = new List<string>();
        private static Queue<XElement> UpdateQueue = new Queue<XElement>();
        private static Thread UpdateThread;
        private static AutoResetEvent WaitFileLoaded = new AutoResetEvent(false);
        
        public static readonly string BaseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio");
        public static readonly string RulesFolder = Path.Combine(BaseFolder, "Rules");
        public static readonly string SettingsFolder = Path.Combine(BaseFolder, "Settings");
        public static readonly string PicturesFolder = Path.Combine(BaseFolder, "Character Portraits");

        static RuleFactory()
        {
            Directory.CreateDirectory(RulesFolder);
            FileLoaded += (f,e) => WaitFileLoaded.Set();
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
            LoadingThreads.Enqueue(Task.Run(() => LoadRulesFolder(RulesFolder)));
            //LoadingThreads.Enqueue(Task.Run(() => LoadRulesFolder(SettingsFolder)));
            LoadingThreads.Enqueue(Task.Run(() => LoadRulesFolder(PicturesFolder)));

            Validate = true;
        }

        public delegate void FileLoadedEventHandler(string Filename, EventArgs e);

        private static FastSmartWeakEvent<FileLoadedEventHandler> fileLoader = new FastSmartWeakEvent<FileLoadedEventHandler>();
        public static event FileLoadedEventHandler FileLoaded
        {
            add { fileLoader.Add(value); }
            remove { fileLoader.Remove(value); }
        }

        private static bool runningBackgroundRegen;

        public delegate void WaitingForRuleHandler(string internalID);
        public static event WaitingForRuleHandler WaitingForRule;

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
                    LoadingThreads.TryPeek(out peek);
                    if (peek != null && peek.IsCompleted)
                        LoadingThreads.TryDequeue(out peek);
                }
                return LoadingThreads.Count >0 && LoadingThreads.ToArray().Any(n => !n.IsCompleted);
            }
        }

        public static bool Validate { get; set; }
        
        public static void Load(XDocument doc)
        { // Unit Tests Only.
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
            //if (RulesBySystem.ContainsKey(sysid))
            //    re = RulesBySystem[sysid];
            //else if (Rules.ContainsKey(id))
            //    re = Rules[id];
            //else 
            if (setting != null && setting.CustomRules.Value.ContainsKey(id))
                return setting.CustomRules.Value[id];
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

        private static RulesElement GetRule(string id, string GameSystem, CampaignSetting setting)
        {
            foreach (var factory in RuleFactories.Values.ToArray())
            {
                RulesElement rule = null;
                if (factory.GameSystem == GameSystem)
                rule = factory.New(id);
                if (rule != null)
                    return rule;
            }
            if (Loading)
            {
                if (WaitingForRule != null)
                    WaitingForRule(id);
                WaitFileLoaded.WaitOne(10); // Waiting here doesn't actually help, except in a race condition.
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
                if (doc.Root.Attribute("Filename") != null) 
                    doc.Root.Attribute("Filename").Remove();
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
                    if (UpdateInfo.Attribute("filename") != null)
                        UpdateInfo.Attribute("filename").Remove();
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
                    if (doc.Root.Name == "PartIndex")
                    {
                        // This happens ASAP, because we already queued an update.
                        UpdateInfo.Element("Filename").Value = Path.GetFileName(file);
                        doc.Root.Name = "Setting";
                        var sys = new XElement("System", new XAttribute("game-system", "D&D4E")); // Anyone passing off .indexes as .settings is a 4E user.
                        doc.Root.Add(sys);
                        foreach (var part in doc.Root.Elements("Part"))
                        {
                            sys.Add(new XElement("Load", new XAttribute("file", part.Element("Filename").Value)));
                            part.Remove();
                        }
                        doc.Save(file);
                    }
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
            {
                CreateLazyRules(doc, setting);
            }
            if (!success) // It worked, but we want to regen anyway.  Probably because ParagonLib updated.
            {
                if (setting != null)
                    doc.Root.Add(new XAttribute("SettingSpecific", "true"));
                FilesToRegen.Enqueue(doc);
                if (!runningBackgroundRegen)
                {
                    runningBackgroundRegen = true;
                    new Thread(RegenLoop) { Name = "RegenThread", IsBackground = true, Priority = ThreadPriority.Lowest }.Start();
                }
            }
            try
            {
                if (code!=null)
                    LoadRuleAssembly(setting, code);
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

        private static void CreateLazyRules(XDocument doc, Dictionary<string, RulesElement> setting)
        {
            //RuleFactories.TryAdd(doc)
            var filename = Path.GetFileNameWithoutExtension(doc.Root.Attribute("Filename") == null ? "Unknown" : doc.Root.Attribute("Filename").Value);
            Version version;
            try
            {
                VersionParser.TryParse(doc.Root.Element("UpdateInfo").Element("Version").Value, out version);
            }
            catch (NullReferenceException)
            {
                version = new Version();
            }
            var sname = string.Format("{0}, Version={1}", filename, version);
            var factory = new LazyRules.LazyFactory(doc);
            if (!CategoriesBySystem.ContainsKey(factory.GameSystem))
                CategoriesBySystem[factory.GameSystem] = new Dictionary<string, CategoryInfo>();
            factory.DescribeCategories(CategoriesBySystem[factory.GameSystem]);
            factory.InitMetadata();
            RuleFactories[sname] = factory;
            //foreach (var re in doc.Descendants("RulesElement"))
            //{
            //    var rule = LazyRulesElement.New(re);
            //    if (setting != null)
            //        setting[rule.InternalId] = rule;
            //    else
            //    {

            //        Rules[rule.InternalId] = rule;
            //        if (!String.IsNullOrEmpty(rule.GameSystem))
            //            RulesBySystem[String.Format("{0}+{1}", rule.GameSystem, rule.InternalId)] = rule;
            //    }
            //}
        }

        private static void LoadRuleAssembly(Dictionary<string, RulesElement> setting, System.Reflection.Assembly code)
        {
            
            var FactoryType = code.GetType("Factory", false);
            if (FactoryType != null)
            {
                //TODO: Insert into list, then use it.
                // usage: RE rule = factory.New(internalId);
                var factory = Activator.CreateInstance(FactoryType) as IFactory;
                var name = code.GetName();
                var sname = string.Format("{0}, Version={1}", name.Name, name.Version);
                if (!CategoriesBySystem.ContainsKey(factory.GameSystem))
                    CategoriesBySystem[factory.GameSystem] = new Dictionary<string, CategoryInfo>();
                factory.DescribeCategories(CategoriesBySystem[factory.GameSystem]);
                factory.InitMetadata();
                RuleFactories[sname] = factory;

            }
            //TODO:
            else 
            {
                throw new InvalidDataException("No Factory class");
                //foreach (var t in code.GetTypes())
                //{
                //    var rule = Activator.CreateInstance(t) as RulesElement;
                //    if (rule == null)
                //        continue;
                //    if (setting != null)
                //        setting[rule.InternalId] = rule;
                //    else
                //    {

                //        Rules[rule.InternalId] = rule;
                //        if (!String.IsNullOrEmpty(rule.GameSystem))
                //            RulesBySystem[String.Format("{0}+{1}", rule.GameSystem, rule.InternalId)] = rule;
                //    }
                //}
            }
        }

        private static void RegenLoop()
        {
            XDocument doc;
            while (FilesToRegen.TryDequeue(out doc))
            {
                if (doc.Root.Attribute("SettingSpecific") == null)
                    LoadRuleAssembly(null, AssemblyGenerator.CompileToDll(doc, true));
                else
                    AssemblyGenerator.CompileToDll(doc, true); // We lost the dictionary reference, so just prepare it for next time.
            }
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
            foreach (var file in Directory.EnumerateFiles(RulesFolder, "*", SearchOption.AllDirectories))
            {
                LoadFile(file);
                LastUpdated = new object(); // Searches and the like use this to choose to regen.
                fileLoader.Raise(file, new EventArgs());
            }
        }

        private static void QueueUpdate(XElement UpdateInfo)
        {
            UpdateQueue.Enqueue(UpdateInfo);
            lock (UpdateQueue)
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
                        Version verLocal = VersionParser.Parse(UpdateInfo.Element("Version").Value);
                        Version verRemote = VersionParser.Parse(wc.DownloadString(Uri(UpdateInfo.Element("VersionAddress").Value, filename)));
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

        public static object LastUpdated { get; private set; }

        public static void RegisterMetadata(RuleData metadata)
        {
            //Rules[metadata.InternalId] = metadata;
            RulesBySystem[String.Format("{0}+{1}", metadata.GameSystem, metadata.InternalId)] = metadata;
        }
    }
}