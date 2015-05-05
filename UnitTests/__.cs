using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if DEBUG
namespace UnitTests
{
    class __ // Exists as a place to decompile things into IL that I'm not sure how to emit.
    {       // Ignore me
        int sw(string c)
        {
            switch (c)
            {
                case"a":
                    return 1;
                case"b":
                    return 2;
                case"3":
                    return 3;
                case"4":
                    return 4;
                case"dtyf":
                    return 6;
                case"q":
                    return 8;
                case "w":
                    return 9;
                default:
                    return 0;
            }
        }
    }
}
#endif