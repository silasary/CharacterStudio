using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace ParagonLib
{
    public static class RuleFactory
    {
        private static int num = 0;
        private static Dictionary<string, RulesElement> Rules;
        static AutoResetEvent FileLoaded = new AutoResetEvent(false);
        static RuleFactory()
        {
            Rules = new Dictionary<string, RulesElement>();
            var RulesFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Rules");
            Directory.CreateDirectory(RulesFolder);
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Loading = true;
                foreach (var file in Directory.EnumerateFiles(RulesFolder, "*", SearchOption.AllDirectories))
                {
                    LoadFile(file);
                    FileLoaded.Set();
                }
                Loading = false;
            });
        }

        public static CharElement New(string id, Workspace workspace, string type = null, string System = "")
        {
            var sysid = String.Format("{0}+{1}",workspace.System, id);
            RulesElement re;
            if (Rules.ContainsKey(sysid))
                re = Rules[sysid];
            else if (Rules.ContainsKey(id))
                re = Rules[id];
            else
                re = Load(id);
            var el = new CharElement(id, GenerateUID(), workspace, re);

            return el;
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
                Rules[String.Format("{0}+{1}", re.System, re.InternalId)] = re;

        }

        private static RulesElement Load(string id)
        {
            while (Loading)
            {
                FileLoaded.WaitOne();
                if (Rules.ContainsKey(id))
                    return Rules[id];

            }
            return null;
        }

        private static void LoadFile(string file)
        {
            if (Path.GetExtension(file) == ".part")
            {
                XDocument doc = XDocument.Load(file);
                foreach (var item in doc.Root.Elements())
                {
                    Load(item);
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
    }
}