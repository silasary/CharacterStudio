using ParagonLib.Compiler;
using ParagonLib.RuleBases;
using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.LazyRules
{
    class LazyFactory : IFactory
    {

        Dictionary<string, RulesElement> initialized = new Dictionary<string, RulesElement>();

        private System.Xml.Linq.XDocument doc;

        public LazyFactory(System.Xml.Linq.XDocument doc)
        {
            // TODO: Complete member initialization
            this.doc = doc;
        }

        public RulesElement New(string internalId)
        {
            if (ids != null && !ids.Contains(internalId))
                return null;
            if (initialized.ContainsKey(internalId))
                return initialized[internalId];
            var ele = this.doc.Descendants("RulesElement").FirstOrDefault(re => re.Attribute("internal-id").Value == internalId);
            if (ele == null)
            {
                // Mark as null, so we don't search again.
                initialized[internalId] = null;
                return null;
            }
            var lre = LazyRulesElement.New(ele);
            initialized[internalId] = lre;
            return lre;
        }

        string gamesystem;
        public string GameSystem
        {
            get 
            {
                if (gamesystem == null)
                    gamesystem = doc.Root.Attribute("game-system").Value;
                return gamesystem;
            }
        }

        Dictionary<string, List<string>> _cats;
        string[] ids;

        public void DescribeCategories(Dictionary<string, Rules.CategoryInfo> dict)
        {
            if (_cats == null)
            {
                List<string> Ids = new List<string>();
                var cats = new Dictionary<string, List<string>>();
                foreach (var ele in this.doc.Descendants("RulesElement"))
                {
                    var InternalId = ele.Attribute("internal-id").Value;
                    Ids.Add(InternalId);
                    if (ele.Element("Category") != null)
                        foreach (var cat in ele.Element("Category").Value.Split(',').Select(n => n.Trim()))
                        {
                            if (!cats.ContainsKey(cat))
                                cats[cat] = new List<string>();
                            cats[cat].Add(InternalId);
                        }
                }
                _cats = cats;
                if (ids == null)
                    ids = Ids.ToArray();
            }
            foreach (var cat in _cats)
            {
                if (!dict.ContainsKey(cat.Key))
                    dict.Add(cat.Key, new Rules.CategoryInfo());
                dict[cat.Key].Members.AddRange(cat.Value);
            }
        }


        public void InitMetadata()
        {
            List<string> Ids = new List<string>();
            var cats = new Dictionary<string, List<string>>();
            foreach (var ele in this.doc.Descendants("RulesElement"))
            {
                var InternalId = ele.Attribute("internal-id").Value;
                Ids.Add(InternalId);
                var metadata = new RuleData()
                    {
                         InternalId = InternalId,
                         Name = ele.Attribute("name").Value,
                         GameSystem = GameSystem,
                         Type = ele.Attribute("type").Value
                    };
                if (ele.Element("Category") != null)
                    metadata.Categories = ele.Element("Category").Value.Split(',').Select(n => n.Trim()).ToArray();
                RuleFactory.RegisterMetadata(metadata);
            }
        }
    }
}
