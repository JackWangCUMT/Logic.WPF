// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public class TraceLog : NotifyObject, ILog
    {
        private string _lastMessage;
        public string LastMessage
        {
            get { return _lastMessage; }
            set
            {
                _lastMessage = value;
                Notify("LastMessage");
            }
        }

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
            LastMessage = "Information: " + message;
        }

        public void LogInformation(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
            LastMessage = "Information: " + string.Format(format, args);
        }

        public void LogWarning(string message)
        {
            Trace.TraceWarning(message);
            LastMessage = "Warning: " + message;
        }

        public void LogWarning(string format, params object[] args)
        {
            Trace.TraceWarning(format, args);
            LastMessage = "Warning: " + string.Format(format, args);
        }

        public void LogError(string message)
        {
            Trace.TraceError(message);
            LastMessage = "Error: " + message;
        }

        public void LogError(string format, params object[] args)
        {
            Trace.TraceError(format, args);
            LastMessage = "Error: " + string.Format(format, args);
        }
    }
}
