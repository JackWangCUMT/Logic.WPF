using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Logic.WPF.ViewModels
{
    public class CodeViewModel : INotifyPropertyChanged
    {
        private string _namespaceName;
        private string _className;
        private string _blockName;
        private string _projectPath;

        public string NamespaceName
        {
            get { return _namespaceName; }
            set
            {
                if (value != _namespaceName)
                {
                    _namespaceName = value;
                    Notify("NamespaceName");
                }
            }
        }

        public string ClassName
        {
            get { return _className; }
            set
            {
                if (value != _className)
                {
                    _className = value;
                    Notify("ClassName");
                }
            }
        }

        public string BlockName
        {
            get { return _blockName; }
            set
            {
                if (value != _blockName)
                {
                    _blockName = value;
                    Notify("BlockName");
                }
            }
        }

        public string ProjectPath
        {
            get { return _projectPath; }
            set
            {
                if (value != _projectPath)
                {
                    _projectPath = value;
                    Notify("ProjectPath");
                }
            }
        }

        public ICommand BrowseCommand { get; set; }
        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
