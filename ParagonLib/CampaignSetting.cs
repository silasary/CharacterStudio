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
            foreach (var system in doc.Root.Elements("System"))
            {
                var setting = new CampaignSetting(name, system.Attribute("game-system").Value);
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
            // TODO:  throw new NotImplementedException();
        }
    }
}
