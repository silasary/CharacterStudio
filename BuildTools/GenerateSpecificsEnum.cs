﻿using System;
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
        class SpecificDetails
        {
            public string Purpose { get; set; }
            public string Usage { get; set; }
        }
        public static bool Generate()
        {
            Environment.CurrentDirectory = Path.Combine(BuildTools.SolutionDir, "ParagonLib", "RuleEngine");
            Dictionary<string, IEnumerable<string>> Specifics = new Dictionary<string, IEnumerable<string>>();
            Dictionary<string, SpecificDetails> Purposes = new Dictionary<string, SpecificDetails>();
            

            ScanParts(Specifics);
            ScanOldSource(Purposes);

            StringBuilder ClassWriter = new StringBuilder();
            EmitSpecificsSource(Specifics, Purposes, ClassWriter);
            File.WriteAllText("Specifics.cs", ClassWriter.ToString());
            Console.WriteLine("Generated Specifics.cs with {0} entries.", Specifics.Count);
            return true;
        }

        private static void ScanOldSource(Dictionary<string, SpecificDetails> Purposes)
        {
            if (!File.Exists("Specifics.cs"))
                return;
            var Summary = new Regex("<summary>(.*)</summary>");
            var Usage = new Regex("\\[Usage\\(\"(.*)\"\\)\\]");
            var Name = new Regex(@"^\W+([A-Za-z_]+),");

            var data = new SpecificDetails();
            foreach (var line in File.ReadAllLines("Specifics.cs"))
            {
                var match = Summary.Match(line);
                if (match.Success)
                    data.Purpose = match.Groups[1].Value;
                else if ((match = Usage.Match(line)).Success)
                    data.Usage = match.Groups[1].Value;
                else if ((match = Name.Match(line)).Success)
                {
                    var name = match.Groups[1].Value;
                    if (!Purposes.ContainsKey(name))
                        Purposes[name] = new SpecificDetails();
                    Purposes[name].Purpose = data.Purpose;
                    Purposes[name].Usage = data.Usage;
                    data = new SpecificDetails();
                }
            }
        }

        private static void EmitSpecificsSource(Dictionary<string, IEnumerable<string>> Specifics, Dictionary<string, SpecificDetails> Purposes, StringBuilder ClassWriter)
        {
            ClassWriter.Append(
@"using System;
/*
    This file is automatically generated!
    Many modifications you make may be lost.
    With that said...
    * Edits within the contents of the <summary/> tags will be respected.
    * Edits to the Mode will be respected.

    Commented specifics are commented because they belong on nothing but powers, 
    and don't have a stated purpose.  Fill in the <summary/> tag, 
    and they will be automatically uncommented on build.  
      This is because many such specifics are either one-ofs, or equally unique.  
      Also, Powers don't care about the Specifics Dictionary as much as any other element type.
*/

namespace ParagonLib.RuleEngine
{
    public enum Specifics
    {
        // This is assigned to any specific that doesn't feature below.
        Unknown,

");
            foreach (var s in Specifics)
            {
                var Comment = false;
                var HasPurpose = Purposes.ContainsKey(s.Key);
                if (!HasPurpose && s.Value.Count() == 1 && s.Value.First() == "Power")
                {
                    Comment = true;
                    ClassWriter.AppendLine("/*");
                }

                if (HasPurpose)
                    ClassWriter.AppendLine(string.Format("\t\t/// <summary>{0}</summary>", Purposes[s.Key].Purpose));
                else
                {
                    ClassWriter.AppendLine("\t\t/// <summary></summary>");
                }
                //ClassWriter.AppendLine(string.Format("\t\t/// <affects>{0}</affects>", string.Join(", ", s.Value)));
                ClassWriter.AppendLine(string.Format("\t\t[Affects(\"{0}\")]", string.Join("\", \"", s.Value)));
                if (HasPurpose && !string.IsNullOrEmpty(Purposes[s.Key].Usage))
                    ClassWriter.AppendLine("\t\t[Usage(\"" + Purposes[s.Key].Usage + "\")]");

                ClassWriter.AppendLine("\t\t"+s.Key + ",").AppendLine();
                if (Comment)
                    ClassWriter.AppendLine("*/");
            }
            ClassWriter.Append(
@"
    }
    [AttributeUsage(AttributeTargets.All)]
    sealed class AffectsAttribute : Attribute
    {
        public AffectsAttribute(params string[] affects)
        {
            this.Affects = affects;
        }

        public string[] Affects { get; private set; }
    }
    [AttributeUsage(AttributeTargets.All)]
    sealed class UsageAttribute : Attribute
    {
        public UsageAttribute(string mode)
        {
            this.Mode = mode;
        }

        public string Mode { get; private set; }
    }
}
");
        }

        private static void ScanParts(Dictionary<string, IEnumerable<string>> Specifics)
        {
            foreach (var part in Directory.GetFiles(BuildTools.PartsDir))
            {
                var Part = XDocument.Load(part);
                foreach (var re in Part.Root.Descendants("RulesElement"))
                {
                    if (re.Attribute("internal-id").Value.StartsWith("ID_TIV_COMPANION"))
                    {
                        re.Attribute("type").Value += " (Tivaan's Companion Cards)";
                    }
                    var type = re.Attribute("type").Value;
                    foreach (var spec in re.Elements("specific"))
                    {
                        var name = spec.Attribute("name").Value;
                        int indexof = name.IndexOfAny(new char[] { '(', '*' });
                        if (indexof > -1)
                            name = name.Substring(0, indexof);
                        name = Regex.Replace(name, @"[ :\-')]", "_");
                        while (name.Contains("__"))
                            name = name.Replace("__", "_");
                        name = name.TrimEnd('_');
                        name = name.TrimStart('•');
                        if (!Specifics.ContainsKey(name))
                            Specifics[name] = new List<string>();
                        if (!Specifics[name].Contains(type))
                            (Specifics[name] as List<string>).Add(type);
                    }
                }
            }
        }
    }
}
