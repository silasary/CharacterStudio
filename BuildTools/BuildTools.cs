using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BuildTools
{
    class BuildTools
    {


        static void Main(string[] args)
        {
            foreach (var index in (Environment.GetEnvironmentVariable("Indexes") ?? "").Split(';'))
            {
                if (!string.IsNullOrEmpty(index))
                {
                    if (File.Exists(Path.GetFileName(index)))
                        File.Delete(Path.GetFileName(index));
                    new WebClient().DownloadFile(index, Path.GetFileName(index));
                }
            }
            foreach (var index in Directory.GetFiles(".", "*.index"))
            {
                LiteLoader.LoadIndex(index);
            }
            Assert.True(GenerateSpecificsEnum.Generate());
        }
    }
}
