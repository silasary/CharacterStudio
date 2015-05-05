using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.Utils
{
    public static class VersionParser
    {
        public static bool TryParse(string versionstring, out Version ver)
        {
            return Version.TryParse(versionstring, out ver) ||
                Version.TryParse(versionstring + ".0", out ver);
        }

        public static Version Parse(string versionstring)
        {
            Version ver;
            if (!TryParse(versionstring, out ver))
                throw new ArgumentException();
            return ver;
        }
    }
}
