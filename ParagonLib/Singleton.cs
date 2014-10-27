using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kamahl.Common
{
    /// <summary>
    /// Represents a Singleton of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Singleton<T> where T : class, new()
    {
        private static T instance = default(T);
        private static readonly object lockObj = new object();
        /// <summary>
        /// Returns a Singleton instance of <typeparam name="T"/>.
        /// </summary>
        public static T Instance
        {
            get
            {
                lock (Singleton<T>.lockObj)
                {
                    T generated;
                    if ((generated = Singleton<T>.instance) == null)
                    {
                        generated = (Singleton<T>.instance = Activator.CreateInstance<T>());
                    }
                    return generated;
                }
            }
        }
    }
}
