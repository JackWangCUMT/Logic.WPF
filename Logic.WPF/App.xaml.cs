// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Native;
using Logic.Portable;
using Logic.WPF.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Logic.WPF
{
    public partial class App : Application
    {
        private Main _main;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (_main == null)
            {
                var dependencies = new Dependencies()
                {
                    TextClipboard = new NativeTextClipboard(),
                    Renderer = new NativeRenderer(),
                    CurrentApplication = new NativeCurrentApplication(),
                    FileDialog = new NativeFileDialog(),
                    MainView = new MainView()
                };

                _main = new Main(dependencies);
                _main.Start();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_main != null)
            {
                _main.Exit();
            }
        } 
    }
}
