using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using ParagonLib.RuleBases;
using ParagonLib.RuleEngine;

namespace ParagonLib
{
    public class CampaignSetting
    {
        public static List<CampaignSetting> Settings = new List<CampaignSetting>();
        public string Name { get; set; }
        public string System { get; set; }
        public string UpdateUrl { get; set; }
        public bool Loaded { get; set; }

        Dictionary<string, FilterType> Filters = new Dictionary<string, FilterType>();

        public CampaignSetting(string name, string system)
        {
            this.Name = name;
            this.System = system;
        }
        
        public static CampaignSetting Load(string Setting, string System, string url = null)
        {
            var setting = Settings.FirstOrDefault(n => n.Name == Setting && n.System == System);

            if (setting == null)
            {
                setting = new CampaignSetting(Setting, System) { Loaded = false };
                var wc = new System.Net.WebClient();
                Directory.CreateDirectory(RuleFactory.SettingsFolder);
                var file = Path.Combine(RuleFactory.SettingsFolder, Setting + ".setting");
                if (File.Exists(file))
                {
                    //ImportSetting(global::System.Xml.Linq.XDocument.Load(file));
                    RuleFactory.LoadFile(file);
                    setting = Settings.FirstOrDefault(n => n.Name == Setting && n.System == System);
                }
                else
                {
                    wc.DownloadStringCompleted += (o, e) =>
                        {
                            if (e.Error != null)
                                return;
                            File.WriteAllText(file, e.Result);
                            RuleFactory.LoadFile(file);

                        };
                    wc.DownloadStringAsync(new Uri(url), file);
                    setting = new CampaignSetting(Setting, System) { Loaded = false };
                    setting.UpdateUrl = url;
                }
            }
            return setting;
        }

        internal static void ImportSetting(System.Xml.Linq.XDocument doc)
        {
            var name = doc.Root.Attribute("name").Value;
            var url = doc.Root.Element("UpdateInfo").Element("PartAddress").Value;
            foreach (var system in doc.Root.Elements("System"))
            {
                var setting = new CampaignSetting(name, system.Attribute("game-system").Value);
                setting.UpdateUrl = url;
                foreach (var type in system.Elements("Type"))
                {
                    setting.Set(type: type.Attribute("type").Value, mode: type.Attribute("mode").Value, sources: type.Elements("Source").Select(n => n.Attribute("name").Value), elements: type.Elements("Element").Select(n => n.Attribute("name").Value));
                }
                setting.Loaded = true;
                setting.CustomRules = new Lazy<Dictionary<string, RulesElement>>(_createCustomRules(system.Elements("Load").Select(n => n.Attribute("file").Value).ToArray(), new Uri(new Uri(url),".").AbsoluteUri, setting), LazyThreadSafetyMode.PublicationOnly);
                Settings.Add(setting);
            }
        }

        private static Func<Dictionary<string, RulesElement>> _createCustomRules(string[] files, string baseurl, CampaignSetting setting)
        {
            return () => // Returning a delayed Function.
            {
                var dict = new Dictionary<string, RulesElement>();
                Directory.CreateDirectory(Path.Combine( RuleFactory.SettingsFolder, setting.Name));
                foreach (var file in files) {
                    var path= Path.Combine(RuleFactory.SettingsFolder, setting.Name, file);
                    RuleFactory.LoadFile(path, baseurl + "/"+file, dict);
                }
                return dict;
            };
        }

        private void Set(string type, string mode, IEnumerable<string> sources, IEnumerable<string> elements)
        {
            bool blacklist = string.Equals(mode, "Blacklist", StringComparison.InvariantCultureIgnoreCase);
            List<FilterEntry> filters = new List<FilterEntry>();
            foreach (var item in sources)
                filters.Add(new FilterEntry() {IsSource = true, Name = item });
            foreach (var item in elements)
                filters.Add(new FilterEntry() {IsSource = false, Name = item });
            Filters[type] = new FilterType() { IsBlacklist = blacklist, Items = filters.ToArray() };
        }

        public bool IsRuleLegal(RuleData ele)
        {
            if (!Loaded)
            {
                var setting = Settings.FirstOrDefault(n => n.Name == Name && n.System == System);
                if (setting != null)
                {
                    this.Filters = setting.Filters;
                    this.CustomRules = setting.CustomRules;
                    this.UpdateUrl = setting.UpdateUrl;
                    Loaded = true;
                }
            }
            if (!Filters.ContainsKey(ele.Type)) // No filters for this kind of element - It's legal.
                return true;

            var filter = Filters[ele.Type];
            foreach (var item in filter.Items)
            {
                bool match =  //TODO:
                    //(item.IsSource && ele.Source == item.Name) 
                    //|| 
                    (!item.IsSource && ele.InternalId == item.Name);
                if (match)
                    return !filter.IsBlacklist; // true if whitelist, false if blacklist.
            }
            return filter.IsBlacklist; // false if whitelist, true if blacklist.
        }

        struct FilterType
        {
            public bool IsBlacklist;

            public FilterEntry[] Items;
        }

        struct FilterEntry
        {
            public bool IsSource ;

            public string Name ;
        }

        public Lazy<Dictionary<string, RulesElement>> CustomRules { get; set; }
    }
}
