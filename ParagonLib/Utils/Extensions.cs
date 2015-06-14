using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParagonLib.Utils
{
    internal static class Extensions
    {
        public static bool IsAlive<T>(this WeakReference<T> wr) where T : class
        {
            T o;
            wr.TryGetTarget(out o);
            return o != null;
        }

        public static T GetTargetOrDefault<T>(this WeakReference<T> wr) where T : class
        {
            T o;
            wr.TryGetTarget(out o);
            return o;
        }
    }
}
