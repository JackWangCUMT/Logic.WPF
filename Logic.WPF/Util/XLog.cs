using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.WPF.Util
{
    public static class XLog
    {
        public static void Initialize()
        {
            Trace.Listeners.Add(
                new TextWriterTraceListener(
                    "Logic.WPF.log",
                    "listener"));
        }

        public static void Close()
        {
            Trace.Flush();
        }

        public static void LogInformation(string message)
        {
            Trace.TraceInformation(message);
        }

        public static void LogInformation(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        public static void LogWarning(string message)
        {
            Trace.TraceWarning(message);
        }

        public static void LogWarning(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
        }

        public static void LogError(string message)
        {
            Trace.TraceError(message);
        }

        public static void LogError(string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }
    }
}
