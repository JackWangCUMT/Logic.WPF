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
                _main = new Main();
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
