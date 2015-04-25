using NUnit.Framework;
using ParagonLib;
using ParagonLib.RuleBases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UnitTests
{
    class FullTests
    {
        [TestFixtureSetUp]
        public void setup()
        {
            // Load 4E ruleset.
            Directory.CreateDirectory(Path.Combine("DefaultRules", "4E Core"));
            if (!File.Exists(Path.Combine("DefaultRules", "4E Core", "WotC.index")))
                new WebClient().DownloadFile("https://dl.dropboxusercontent.com/u/4187827/CharBuilder/4E/WotC.index", Path.Combine("DefaultRules", "4E Core", "WotC.index"));
        }

        [TestCaseSource("Chars",Category="LoadCharacters")]
        public void LoadChar(string file)
        {
            var serializer = new Serializer();
            Character c = serializer.Load(file);
            //Assert.IsEmpty(serializer.Errors);
            c.Save(c.Name);
        }

        public IEnumerable<string> Chars()
        {
            return Directory.EnumerateFiles(".", "*.dnd4e").Select(n => Path.GetFileName(n));
        }
        public IEnumerable<string> Parts()
        {
            return Directory.EnumerateFiles(".", "*.part").Select(n => Path.GetFileName(n));
        }

        [TestCaseSource("Parts")]
        public void CompilePart(string part)
        {
            if (Directory.Exists(Path.Combine(RuleFactory.BaseFolder, "Compiled Rules")))
            foreach (var file in Directory.EnumerateFiles(Path.Combine(RuleFactory.BaseFolder, "Compiled Rules"), Path.GetFileNameWithoutExtension(part)))
            {
                File.Delete(file);
            }
            var ass = ParagonLib.Compiler.AssemblyGenerator.CompileToDll(XDocument.Load(part, LoadOptions.SetLineInfo),true, part);
            foreach (var t in ass.GetTypes())
            {
                var i = Activator.CreateInstance(t) as RulesElement;
                Assert.IsNotNullOrEmpty(i.Name);
                Assert.IsNotNull(i.Calculate);
            }
        }
    }
}
