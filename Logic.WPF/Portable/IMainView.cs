using Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Portable
{
    public interface IMainView
    {
        void Initialize(MainViewModel model, Main main);
        void Show();
    }
}
