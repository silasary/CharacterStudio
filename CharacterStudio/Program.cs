﻿using ParagonLib.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CharacterStudio
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (RuleFactory.Loading)
                Console.WriteLine("Loading");
            Application.Run(new PrimaryForm());
        }

        static void HandleUnhandledException (object sender, UnhandledExceptionEventArgs e)
        {
            ParagonLib.Logging.Log("Crashlog", System.Diagnostics.TraceEventType.Critical, e.ExceptionObject.ToString( ));
        }
    }
}
