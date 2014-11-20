using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Util
{
    public static class Log
    {
        public static bool IsEnabled { get; set; }

        public static void Initialize()
        {
            if (IsEnabled)
            {
                Trace.Listeners.Add(
                    new TextWriterTraceListener(
                        "Logic.WPF.log",
                        "listener"));
            }
        }

        public static void Close()
        {
            if (IsEnabled)
            {
                Trace.Flush();
            }
        }

        public static void LogInformation(string message)
        {
            if (IsEnabled)
            {
                Trace.TraceInformation(message); 
            }
        }

        public static void LogInformation(string format, params object[] args)
        {
            if (IsEnabled)
            {
                Trace.TraceInformation(format, args); 
            }
        }

        public static void LogWarning(string message)
        {
            if (IsEnabled)
            {
                Trace.TraceWarning(message); 
            }
        }

        public static void LogWarning(string format, params object[] args)
        {
            if (IsEnabled)
            {
                Trace.TraceWarning(format, args); 
            }
        }

        public static void LogError(string message)
        {
            if (IsEnabled)
            {
                Trace.TraceError(message); 
            }
        }

        public static void LogError(string format, params object[] args)
        {
            if (IsEnabled)
            {
                Trace.TraceError(format, args); 
            }
        }
    }
}
