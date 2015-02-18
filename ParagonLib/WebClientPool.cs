using Kamahl.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ParagonLib
{
    [Obsolete]
	static class WebClientPool
	{
        static List<WebClient> pool = new List<WebClient>();

        public static WebClient Client 
        {
            get
            {
                lock (pool)
                {
                    var clint = pool.FirstOrDefault(c => !c.IsBusy);
                    if (clint == null)
                        pool.Add(clint = new WebClient());
                    return clint;
                }
            }
        }
	}

}