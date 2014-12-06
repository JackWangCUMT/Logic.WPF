using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface ILog
    {
        bool IsEnabled { get; set; }
        void Initialize();
        void Close();
        void LogInformation(string message);
        void LogInformation(string format, params object[] args);
        void LogWarning(string message);
        void LogWarning(string format, params object[] args);
        void LogError(string message);
        void LogError(string format, params object[] args);
    }
}
