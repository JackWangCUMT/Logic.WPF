using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Logic.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Trace.Listeners.Add(
                new TextWriterTraceListener(
                    "Logic.WPF.log", 
                    "listener"));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Trace.Flush();
        }
    }
}
