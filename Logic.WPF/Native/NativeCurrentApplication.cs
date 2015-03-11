using Logic.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Logic.Native
{
    public class NativeCurrentApplication : ICurrentApplication
    {
        public void Close()
        {
            for (int i = 0; i < Application.Current.Windows.Count; i++)
            {
                Application.Current.Windows[i].Close();
            }
        }

        public void Invoke(Action callback)
        {
            Application.Current.Dispatcher.Invoke(callback);
        }
    }
}
