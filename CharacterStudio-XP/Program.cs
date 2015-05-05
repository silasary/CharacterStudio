using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;

namespace CharacterStudio_XP
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var exeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string frameworkDir= Path.Combine(Path.GetDirectoryName(exeLocation),"Frameworks");
            if (Directory.Exists(frameworkDir))
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var assembly = Path.Combine(frameworkDir, e.Name +".dll");
                if (File.Exists(assembly)) // Note: Case sensitivity is maybe an issue?
                    return System.Reflection.Assembly.Load(new FileInfo(assembly).FullName);
                return null;
            };
            var platform = GetPlatform ();
            if (platform.Platform == PlatformID.Win32NT)
                Application.Initialize (ToolkitType.Wpf);
            else // We'll get to Linux/GTK soon enough.
                Application.Initialize (ToolkitType.Cocoa);
            var main = new MainWindow();
            main.Show();
            Application.Run();
        }

        public static OperatingSystem GetPlatform()
        {
            switch (Environment.OSVersion.Platform) {

                case PlatformID.Unix:
                    if (System.IO.Directory.Exists ("/Library"))
                        return new OperatingSystem (PlatformID.MacOSX, new Version (10,0)); // We should try to work that out.
                    else
                        return Environment.OSVersion;
                   

                case PlatformID.MacOSX: // Wow, they actually got it!
                case PlatformID.Win32NT:
                default:
                    return Environment.OSVersion;

            }
        }
    }
}
