using Logic.Core;
using Logic.WPF.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Logic.WPF.ViewModels
{
    public class MainViewModel : NotifyObject
    {
        #region Properties

        private IList<XBlock> _blocks;

        [ImportMany(typeof(XBlock))]
        public IList<XBlock> Blocks
        {
            get { return _blocks; }
            set
            {
                if (value != _blocks)
                {
                    _blocks = value;
                    Notify("Blocks");
                }
            }
        }

        private IList<ITemplate> _templates;

        [ImportMany(typeof(ITemplate))]
        public IList<ITemplate> Templates
        {
            get { return _templates; }
            set
            {
                if (value != _templates)
                {
                    _templates = value;
                    Notify("Templates");
                }
            }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    Notify("FileName");
                }
            }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value != _filePath)
                {
                    _filePath = value;
                    Notify("FilePath");
                }
            }
        }

        private ToolMenuModel _tool;
        public ToolMenuModel Tool
        {
            get { return _tool; }
            set
            {
                if (value != _tool)
                {
                    _tool = value;
                    Notify("Tool");
                }
            }
        }

        #endregion

        #region Commands

        public ICommand FileNewCommand { get; set; }
        public ICommand FileOpenCommand { get; set; }
        public ICommand FileSaveCommand { get; set; }
        public ICommand FileSaveAsCommand { get; set; }
        public ICommand FileExitCommand { get; set; }

        public ICommand EditUndoCommand { get; set; }
        public ICommand EditRedoCommand { get; set; }
        public ICommand EditCutCommand { get; set; }
        public ICommand EditCopyCommand { get; set; }
        public ICommand EditPasteCommand { get; set; }
        public ICommand EditDeleteCommand { get; set; }
        public ICommand EditSelectAllCommand { get; set; }

        public ICommand EditAlignLeftBottomCommand { get; set; }
        public ICommand EditAlignBottomCommand { get; set; }
        public ICommand EditAlignRightBottomCommand { get; set; }
        public ICommand EditAlignLeftCommand { get; set; }
        public ICommand EditAlignCenterCenterCommand { get; set; }
        public ICommand EditAlignRightCommand { get; set; }
        public ICommand EditAlignLeftTopCommand { get; set; }
        public ICommand EditAlignTopCommand { get; set; }
        public ICommand EditAlignRightTopCommand { get; set; }

        public ICommand EditIncreaseTextSizeCommand { get; set; }
        public ICommand EditDecreaseTextSizeCommand { get; set; }
        public ICommand EditToggleFillCommand { get; set; }
        public ICommand EditToggleSnapCommand { get; set; }
        public ICommand EditToggleInvertStartCommand { get; set; }
        public ICommand EditToggleInvertEndCommand { get; set; }

        public ICommand EditCancelCommand { get; set; }

        public ICommand ToolNoneCommand { get; set; }
        public ICommand ToolSelectionCommand { get; set; }
        public ICommand ToolWireCommand { get; set; }
        public ICommand ToolPinCommand { get; set; }
        public ICommand ToolLineCommand { get; set; }
        public ICommand ToolEllipseCommand { get; set; }
        public ICommand ToolRectangleCommand { get; set; }
        public ICommand ToolTextCommand { get; set; }

        public ICommand BlockImportCommand { get; set; }
        public ICommand BlockImportCodeCommand { get; set; }
        public ICommand BlockExportCommand { get; set; }
        public ICommand BlockCreateProjectCommand { get; set; }
        public ICommand InsertBlockCommand { get; set; }

        public ICommand TemplateImportCommand { get; set; }
        public ICommand TemplateImportCodeCommand { get; set; }
        public ICommand TemplateExportCommand { get; set; }
        public ICommand ApplyTemplateCommand { get; set; }

        public ICommand SimulationStartCommand { get; set; }
        public ICommand SimulationStopCommand { get; set; }
        public ICommand SimulationRestartCommand { get; set; }
        public ICommand SimulationCreateGraphCommand { get; set; }
        public ICommand SimulationOptionsCommand { get; set; }

        #endregion
    }
}
