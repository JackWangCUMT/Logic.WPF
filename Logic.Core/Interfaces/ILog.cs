// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface ILog
    {
        string LastMessage { get; set; }
        void Initialize(string path);
        void Close();
        void LogInformation(string message);
        void LogInformation(string format, params object[] args);
        void LogWarning(string message);
        void LogWarning(string format, params object[] args);
        void LogError(string message);
        void LogError(string format, params object[] args);
    }
}
