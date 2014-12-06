using Logic.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public class Log : ILog
    {
        public bool IsEnabled { get; set; }

        public void Initialize()
        {
            if (IsEnabled)
            {
                Trace.Listeners.Add(
                    new TextWriterTraceListener(
                        "Logic.WPF.log",
                        "listener"));
            }
        }

        public void Close()
        {
            if (IsEnabled)
            {
                Trace.Flush();
            }
        }

        public void LogInformation(string message)
        {
            if (IsEnabled)
            {
                Trace.TraceInformation(message); 
            }
        }

        public void LogInformation(string format, params object[] args)
        {
            if (IsEnabled)
            {
                Trace.TraceInformation(format, args); 
            }
        }

        public void LogWarning(string message)
        {
            if (IsEnabled)
            {
                Trace.TraceWarning(message); 
            }
        }

        public void LogWarning(string format, params object[] args)
        {
            if (IsEnabled)
            {
                Trace.TraceWarning(format, args); 
            }
        }

        public void LogError(string message)
        {
            if (IsEnabled)
            {
                Trace.TraceError(message); 
            }
        }

        public void LogError(string format, params object[] args)
        {
            if (IsEnabled)
            {
                Trace.TraceError(format, args); 
            }
        }
    }
}
