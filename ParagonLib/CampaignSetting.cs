using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib
{
    public class CampaignSetting
    {
        public static List<CampaignSetting> Settings = new List<CampaignSetting>();
        public string Name { get; set; }
        public string System { get; set; }
        public string UpdateUrl { get; set; }

        List<FilterType> Filters = new List<FilterType>();

        public CampaignSetting(string name, string system)
        {
            this.Name = name;
            this.System = system;
        }
        
        public static CampaignSetting Load(string Setting, string System)
        {
            return Settings.FirstOrDefault(n => n.Name == Setting && n.System == System);
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
                // TODO:  <Load> elements.  We need to mark a way to enable optional elements.
                Settings.Add(setting);
            }
        }

        private void Set(string type, string mode, IEnumerable<string> sources, IEnumerable<string> elements)
        {
            bool blacklist = string.Equals(mode, "Blacklist", StringComparison.InvariantCultureIgnoreCase);
            foreach (var item in sources)
                Filters.Add(new FilterType() {IsBlacklist = blacklist, IsSource = true, Name = item });
            foreach (var item in elements)
                Filters.Add(new FilterType() {IsBlacklist = blacklist, IsSource = false, Name = item });
        }



        struct FilterType
        {
            public bool IsBlacklist;

            public bool IsSource ;

            public string Name ;
        }
    }
}
