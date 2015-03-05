using NUnit.Framework;
using ParagonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            }
        }
    }
}
