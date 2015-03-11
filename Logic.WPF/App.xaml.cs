using Logic.WPF.Native;
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
        private AppMain _main;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (_main == null)
            {
                var dependencies = new AppDependencies()
                {
                    CurrentApplication = new NativeCurrentApplication(),
                    FileDialog = new NativeFileDialog()
                };

                _main = new AppMain(dependencies);
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
