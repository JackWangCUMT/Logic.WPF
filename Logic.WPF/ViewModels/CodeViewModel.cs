using Logic.WPF.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Logic.WPF.ViewModels
{
    public class CodeViewModel : ViewModel
    {
        private string _namespaceName;
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

        private string _className;
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

        private string _blockName;
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

        private string _projectPath;
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
    }
}
