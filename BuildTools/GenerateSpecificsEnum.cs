using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildTools
{
    static class GenerateSpecificsEnum
    {
        
        public static bool Generate()
        {
            Dictionary<string, IEnumerable<string>> Specifics = new Dictionary<string, IEnumerable<string>>();
            foreach (var part in Directory.GetFiles("parts"))
            {
                var Part = XDocument.Load(part);
                foreach (var re in Part.Root.Descendants("RulesElement"))
                {
                    var type = re.Attribute("type").Value;
                    foreach (var spec in re.Elements("specific"))
                    {
                        var name = spec.Attribute("name").Value;
                        int indexof = name.IndexOfAny(new char[] { '(', '*' });
                        if (indexof > -1)
                            name = name.Substring(0, indexof);
                        name = Regex.Replace(name, "[ :-]", "_");
                        while (name.Contains("__"))
                            name = name.Replace("__", "_");
                        name = name.TrimEnd('_');
                        if (!Specifics.ContainsKey(name))
                            Specifics[name] = new List<string>();
                        if (!Specifics[name].Contains(type))
                            (Specifics[name] as List<string>).Add(type);
                    }
                }
            }
            //foreach (var s in Specifics.Keys.ToArray())
            //{
            //    Specifics[s] = Specifics[s].ToArray();
            //}
            StringBuilder ClassWriter = new StringBuilder();
            ClassWriter.Append(
@"namespace ParagonLib.RuleEngine
{
    public enum Specifics
    {
Unknown,
");
            foreach (var s in Specifics)
            {
                if (s.Value.Count() == 1 && s.Value.First() == "Power")
                    continue;
                ClassWriter.AppendLine(@"/// <summary></summary>");
                ClassWriter.AppendLine(string.Format(@"/// <affects>{0}</affects>", string.Join(", ", s.Value)));
                ClassWriter.AppendLine(s.Key + ",");
            }
            ClassWriter.Append(
@"
    }
}
");
            File.WriteAllText("Specifics.cs", ClassWriter.ToString());
            return true;
        }
    }
}
