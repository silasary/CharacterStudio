using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildTools
{
    class LiteLoader
    {
        private static Uri Uri(string p, string filename)
        {
            return new Uri(p.Replace("^", Path.GetFileNameWithoutExtension(filename)));
        }

        public static void LoadIndex(string file)
        {
            XDocument doc = XDocument.Load(file);
            foreach (var n in doc.Root.Elements("Obsolete"))
            {
                File.Delete(Path.Combine(Path.GetDirectoryName(file), n.Element("Filename").Value));
            }
            foreach (var n in doc.Root.Elements("Part"))
            {
                string newfile;
                if (!File.Exists(newfile = Path.Combine("parts", n.Element("Filename").Value)))
                {
                    try
                    {
                        var uri = Uri(n.Element("PartAddress").Value, newfile);
                        var xml = new WebClient().DownloadString(uri);
                        File.WriteAllText(newfile, xml);
                    }
                    catch (WebException c)
                    {
                        Console.WriteLine("WARNING: {0}: Failed getting {1} from index. {2}", Path.GetFileName(file), newfile, c);
                    }
                }
            }
        }
    }
}
