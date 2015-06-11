using System;
using System.IO;
using System.Net;
using System.Reflection;
using NUnit.Framework;

namespace BuildTools
{
    internal class BuildTools
    {
        public static string PartsDir { get; set; }

        public static string SolutionDir { get; set; }

        private static void Main(string[] args)
        {
            if (args.Length > 0)
                SolutionDir = args[0];
            else
                SolutionDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..")).FullName;
            PartsDir = new DirectoryInfo("parts").FullName;
            if (!Directory.Exists(PartsDir))
                Directory.CreateDirectory(PartsDir);
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