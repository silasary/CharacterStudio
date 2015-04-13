using NUnit.Framework;
using ParagonLib;
using ParagonLib.RuleBases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UnitTests
{
    class FullTests
    {
        [Test]
        public void LoadChar()
        {
            foreach (var file in Directory.EnumerateFiles(".", "*.dnd4e"))
            {
                var serializer = new Serializer();
                Character c = serializer.Load(file);
                //Assert.IsEmpty(serializer.Errors);
                c.Save(c.Name);
            }
        }

        [Test]
        public void CompilePart()
        {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(RuleFactory.BaseFolder, "Compiled Rules"), "PA_*.dll"))
            {
                File.Delete(file);
            }
            var ass = ParagonLib.Compiler.AssemblyGenerator.CompileToDll(XDocument.Load("PA_Classes.part", LoadOptions.SetLineInfo), "PA_Classes.part");
            var t = Activator.CreateInstance(ass.GetTypes().First()) as RulesElement;
            Assert.IsNotNullOrEmpty(t.Name);
            Assert.IsNotNull(t.Calculate);
        }
    }
}
