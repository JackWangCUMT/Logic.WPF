using Logic.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public class TraceLog : ILog
    {
        public void Initialize(string path)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(path, "listener"));
        }

        public void Close()
        {
            Trace.Flush();
        }

        public void LogInformation(string message)
        {
            Trace.TraceInformation(message); 
        }

        public void LogInformation(string format, params object[] args)
        {
            Trace.TraceInformation(format, args); 
        }

        public void LogWarning(string message)
        {
            Trace.TraceWarning(message); 
        }

        public void LogWarning(string format, params object[] args)
        {
            Trace.TraceWarning(format, args); 
        }

        public void LogError(string message)
        {
            Trace.TraceError(message); 
        }

        public void LogError(string format, params object[] args)
        {
            Trace.TraceError(format, args); 
        }
    }
}
