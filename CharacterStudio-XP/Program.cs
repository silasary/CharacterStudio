using System;
using System.Collections.Generic;
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
                    break;

                case PlatformID.MacOSX: // Wow, they actually got it!
                case PlatformID.Win32NT:
                default:
                    return Environment.OSVersion;

            }
        }
    }
}
