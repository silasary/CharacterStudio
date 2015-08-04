using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RulesDb;

namespace RulesDb
{
    public static class RulesDb
    {
        private static Dictionary<string, D20RuleDb> dbs = new Dictionary<string, D20RuleDb>();
        public static void LoadRulesFile(string Filename)
        {
            var doc = new XmlDocument();
            doc.Load(Filename);
            ParseXml(doc);
        }

        private static void ParseXml(XmlDocument xmlDocument)
        {
            var parser = new ParseXml();
            parser.Parse(xmlDocument);
        }

        public static D20RulesElement GetRule(string GameSystem, string InternalId)
        {
            if (!dbs.ContainsKey(GameSystem))
                dbs.Add(GameSystem, new D20RuleDb());
            return dbs[GameSystem].GetRule(InternalId);
        }

        public static D20RulesElement GetOrAddRule(string GameSystem, string Name, string Type, string InternalId, string Source = "")
        {
            if (!dbs.ContainsKey(GameSystem))
                dbs.Add(GameSystem, new D20RuleDb());
            return dbs[GameSystem].GetOrAddRule(Name, Type, InternalId, Source);
        }
    }
}
