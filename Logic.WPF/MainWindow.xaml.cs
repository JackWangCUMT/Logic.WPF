using Logic.Core;
using Logic.WPF.Page;
using Logic.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Logic.WPF
{
    public partial class MainWindow : Window
    {
        #region Properties

        [ImportMany(typeof(XBlock))]
        public IList<XBlock> Blocks { get; set; }

        #endregion

        #region Fields

        private string _pageFileName = string.Empty;
        private Point _dragStartPoint;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitPage();
            InitKeys();
            InitMenu();
            InitBlocks();
        }

        #endregion

        #region Visual Parent

        public T FindVisualParent<T>(DependencyObject child) 
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            return FindVisualParent<T>(parentObject);
        } 

        #endregion

        #region Initialize

        private void InitPage()
        {
            var layers = new XLayers();
            layers.Template = page.templateLayer;
            layers.Blocks = page.blockLayer;
            layers.Wires = page.wireLayer;
            layers.Pins = page.pinLayer;

            var renderer = new XRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };
            page.templateLayer.Renderer = renderer;
            page.blockLayer.Renderer = renderer;
            page.wireLayer.Renderer = renderer;
            page.pinLayer.Renderer = renderer;
            page.editorLayer.Renderer = renderer;

            page.editorLayer.History = new XHistory<XPage>();
            page.editorLayer.Layers = layers;
            page.editorLayer.CurrentTool = XCanvas.Tool.Selection;
            page.editorLayer.AllowDrop = true;

            page.editorLayer.DragEnter += (s, e) =>
            {
                if (!e.Data.GetDataPresent("Block") || s == e.Source)
                {
                    e.Effects = DragDropEffects.None;
                }
            };

            page.editorLayer.Drop += (s, e) =>
            {
                Point point = e.GetPosition(page.editorLayer);

                // block
                if (e.Data.GetDataPresent("Block"))
                {
                    var block = e.Data.GetData("Block") as XBlock;
                    if (block != null)
                    {
                        page.editorLayer.History.Snapshot(
                            page.editorLayer.Create("Page"));
                        var copy = page.editorLayer.Insert(block, point.X, point.Y);
                        page.editorLayer.Connect(copy);
                        e.Handled = true;
                    }
                }
                // files
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length == 1)
                    {
                        page.editorLayer.Load(files[0]);
                    }
                }
            };
        }

        private void InitBlocks()
        {
            blocks.PreviewMouseLeftButtonDown += (s, e) =>
            {
                _dragStartPoint = e.GetPosition(null);
            };

            blocks.PreviewMouseMove += (s, e) =>
            {
                Point point = e.GetPosition(null);
                Vector diff = _dragStartPoint - point;
                if (e.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    var listBox = s as ListBox;
                    var listBoxItem = FindVisualParent<ListBoxItem>(
                        (DependencyObject)e.OriginalSource);
                    if (listBoxItem != null)
                    {
                        var block = (XBlock)listBox
                            .ItemContainerGenerator
                            .ItemFromContainer(listBoxItem);
                        DataObject dragData = new DataObject("Block", block);
                        DragDrop.DoDragDrop(
                            listBoxItem, 
                            dragData, 
                            DragDropEffects.Move);
                    }
                }
            };

            Blocks = new ObservableCollection<XBlock>();

            try
            {
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                catalog.Catalogs.Add(new DirectoryCatalog("."));

                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);
            }
            catch (CompositionException ex)
            {
                MessageBox.Show(ex.Message);
            }

            DataContext = this;
        }

        private void InitKeys()
        {
            PreviewKeyDown += (s, e) =>
            {
                bool control = Keyboard.Modifiers == ModifierKeys.Control;
                bool none = Keyboard.Modifiers == ModifierKeys.None;

                switch (e.Key)
                {
                    // block
                    case Key.B:
                        if (none)
                        {
                            Block();
                        }
                        break;

                    // undo
                    case Key.Z:
                        if (control)
                        {
                            Undo();
                        }
                        break;
                    // redo
                    case Key.Y:
                        if (control)
                        {
                            Redo();
                        }
                        break;

                    // cut
                    case Key.X:
                        if (control)
                        {
                            Cut();
                        }
                        break;
                    // copy
                    case Key.C:
                        if (control)
                        {
                            Copy();
                        }
                        break;
                    // paste
                    case Key.V:
                        if (control)
                        {
                            Paste();
                        }
                        break;
                    // delete
                    case Key.Delete:
                        if (none)
                        {
                            Delete();
                        }
                        break;

                    // select all
                    case Key.A:
                        if (control)
                        {
                            SelectAll();
                        }
                        break;

                    // cancel
                    case Key.Escape:
                        if (none)
                        {
                            Cancel();
                        }
                        break;

                    // toggle fill
                    case Key.F:
                        if (none)
                        {
                            ToggleFill();
                        }
                        break;

                    // '[' toggle invert start
                    case Key.OemOpenBrackets:
                        if (none)
                        {
                            ToggleInvertStart();
                        }
                        break;
                    // ']' toggle invert end
                    case Key.OemCloseBrackets:
                        if (none)
                        {
                            ToggleInvertEnd();
                        }
                        break;

                    // text size
                    case Key.Add:
                    case Key.OemPlus:
                        if (control)
                        {
                            IncreaseTextSize();
                        }
                        break;
                    case Key.Subtract:
                    case Key.OemMinus:
                        if (control)
                        {
                            DecreaseTextSize();
                        }
                        break;

                    // text alignment
                    case Key.NumPad1:
                    case Key.D1:
                        if (control)
                        {
                            AlignLeftBottom();
                        }
                        break;
                    case Key.NumPad2:
                    case Key.D2:
                        if (control)
                        {
                            AlignBottom();
                        }
                        break;
                    case Key.NumPad3:
                    case Key.D3:
                        if (control)
                        {
                            AlignRightBottom();
                        }
                        break;
                    case Key.NumPad4:
                    case Key.D4:
                        if (control)
                        {
                            AlignLeft();
                        }
                        break;
                    case Key.NumPad5:
                    case Key.D5:
                        if (control)
                        {
                            AlignCenterCenter();
                        }
                        break;
                    case Key.NumPad6:
                    case Key.D6:
                        if (control)
                        {
                            AlignRight();
                        }
                        break;
                    case Key.NumPad7:
                    case Key.D7:
                        if (control)
                        {
                            AlignLeftTop();
                        }
                        break;
                    case Key.NumPad8:
                    case Key.D8:
                        if (control)
                        {
                            AlignTop();
                        }
                        break;
                    case Key.NumPad9:
                    case Key.D9:
                        if (control)
                        {
                            AlignRightTop();
                        }
                        break;

                    // new
                    // tool none
                    case Key.N:
                        if (control)
                        {
                            New();
                        }
                        else if (none)
                        {
                            SetToolNone();
                        }
                        break;
                    // tool line
                    case Key.L:
                        if (none)
                        {
                            SetToolLine();
                        }
                        break;
                    // tool ellipse
                    case Key.E:
                        if (none)
                        {
                            SetToolEllipse();
                        }
                        break;
                    // tool rectangle
                    case Key.R:
                        if (none)
                        {
                            SetToolRectangle();
                        }
                        break;
                    // tool text
                    case Key.T:
                        if (none)
                        {
                            SetToolText();
                        }
                        break;
                    // tool wire
                    case Key.W:
                        if (none)
                        {
                            SetToolWire();
                        }
                        break;
                    // tool pin
                    case Key.P:
                        if (none)
                        {
                            SetToolPin();
                        }
                        break;

                    // toggle snap
                    case Key.G:
                        if (none)
                        {
                            ToggleSnap();
                        }
                        break;

                    // open
                    case Key.O:
                        if (control)
                        {
                            Open();
                        }
                        break;
                    // save
                    // tool selection
                    case Key.S:
                        if (control)
                        {
                            Save();
                        }
                        else if (none)
                        {
                            SetToolSelection();
                        }
                        break;
                }
            };
        }

        private void InitMenu()
        {
            fileNew.Click += (s, e) => New();
            fileOpen.Click += (s, e) => Open();
            fileSave.Click += (s, e) => Save();
            fileSaveAs.Click += (s, e) => SaveAs();
            fileExit.Click += (s, e) => Close();

            editUndo.Click += (s, e) => Undo();
            editRedo.Click += (s, e) => Redo();
            editCut.Click += (s, e) => Cut();
            editCopy.Click += (s, e) => Copy();
            editPaste.Click += (s, e) => Paste();
            editDelete.Click += (s, e) => Delete();
            editSelectAll.Click += (s, e) => SelectAll();

            editAlignLeftBottom.Click += (s, e) => AlignLeftBottom();
            editAlignBottom.Click += (s, e) => AlignBottom();
            editAlignRightBottom.Click += (s, e) => AlignRightBottom();
            editAlignLeft.Click += (s, e) => AlignLeft();
            editAlignCenterCenter.Click += (s, e) => AlignCenterCenter();
            editAlignRight.Click += (s, e) => AlignRight();
            editAlignLeftTop.Click += (s, e) => AlignLeftTop();
            editAlignTop.Click += (s, e) => AlignTop();
            editAlignRightTop.Click += (s, e) => AlignRightTop();

            editIncreaseTextSize.Click += (s, e) => IncreaseTextSize();
            editDecreaseTextSize.Click += (s, e) => DecreaseTextSize();

            editToggleFill.Click += (s, e) => ToggleFill();
            editToggleInvertStart.Click += (s, e) => ToggleInvertStart();
            editToggleInvertEnd.Click += (s, e) => ToggleInvertEnd();

            editCancel.Click += (s, e) => Cancel();

            toolNone.Click += (s, e) => SetToolNone();
            toolSelection.Click += (s, e) => SetToolSelection();
            toolWire.Click += (s, e) => SetToolWire();
            toolPin.Click += (s, e) => SetToolPin();
            toolLine.Click += (s, e) => SetToolLine();
            toolEllipse.Click += (s, e) => SetToolEllipse();
            toolRectangle.Click += (s, e) => SetToolRectangle();
            toolText.Click += (s, e) => SetToolText();

            UpdateToolMenu();
        } 

        #endregion

        #region File

        private void New()
        {
            page.editorLayer.New();
            _pageFileName = string.Empty;
        }

        private void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                page.editorLayer.Load(dlg.FileName);
                _pageFileName = dlg.FileName;
            }
        }

        private void Save()
        {
            if (!string.IsNullOrEmpty(_pageFileName))
            {
                page.editorLayer.Save(_pageFileName);
            }
            else
            {
                SaveAs();
            }
        }

        private void SaveAs()
        {
            string fileName = string.IsNullOrEmpty(_pageFileName) ?
                "shapes" : System.IO.Path.GetFileName(_pageFileName);

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Json (*.json)|*.json",
                FileName = fileName
            };

            if (dlg.ShowDialog() == true)
            {
                page.editorLayer.Save(dlg.FileName);
                _pageFileName = dlg.FileName;
            }
        }

        #endregion

        #region Edit

        private void Undo()
        {
            page.editorLayer.Undo();
        }

        private void Redo()
        {
            page.editorLayer.Redo();
        }

        private void Cut()
        {
            page.editorLayer.Cut();
        }

        private void Copy()
        {
            page.editorLayer.Copy();
        }

        private void Paste()
        {
            page.editorLayer.Paste();
        }

        private void Delete()
        {
            page.editorLayer.SelectionDelete();
        }

        private void SelectAll()
        {
            page.editorLayer.SelectAll();
        }

        private void ToggleFill()
        {
            page.editorLayer.ToggleFill();
        }

        private void ToggleSnap()
        {
            page.editorLayer.EnableSnap = !page.editorLayer.EnableSnap;
        }

        private void ToggleInvertStart()
        {
            page.editorLayer.ToggleInvertStart();
        }

        private void ToggleInvertEnd()
        {
            page.editorLayer.ToggleInvertEnd();
        }

        private void IncreaseTextSize()
        {
            page.editorLayer.SetTextSizeDelta(+1.0);
        }

        private void DecreaseTextSize()
        {
            page.editorLayer.SetTextSizeDelta(-1.0);
        }

        private void AlignLeftBottom()
        {
            page.editorLayer.SetTextHAlignment(HAlignment.Left);
            page.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignBottom()
        {
            page.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignRightBottom()
        {
            page.editorLayer.SetTextHAlignment(HAlignment.Right);
            page.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignLeft()
        {
            page.editorLayer.SetTextHAlignment(HAlignment.Left);
        }

        private void AlignCenterCenter()
        {
            page.editorLayer.SetTextHAlignment(HAlignment.Center);
            page.editorLayer.SetTextVAlignment(VAlignment.Center);
        }

        private void AlignRight()
        {
            page.editorLayer.SetTextHAlignment(HAlignment.Right);
        }

        private void AlignLeftTop()
        {
            page.editorLayer.SetTextHAlignment(HAlignment.Left);
            page.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignTop()
        {
            page.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignRightTop()
        {
            page.editorLayer.SetTextHAlignment(HAlignment.Right);
            page.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void Cancel()
        {
            page.editorLayer.Cancel();
        }

        #endregion

        #region Tool

        private void SetToolNone()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.None;
            UpdateToolMenu();
        }

        private void SetToolSelection()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.Selection;
            UpdateToolMenu();
        }

        private void SetToolLine()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.Line;
            UpdateToolMenu();
        }

        private void SetToolEllipse()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.Ellipse;
            UpdateToolMenu();
        }

        private void SetToolRectangle()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.Rectangle;
            UpdateToolMenu();
        }

        private void SetToolText()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.Text;
            UpdateToolMenu();
        }

        private void SetToolWire()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.Wire;
            UpdateToolMenu();
        }

        private void SetToolPin()
        {
            page.editorLayer.CurrentTool = XCanvas.Tool.Pin;
            UpdateToolMenu();
        }

        private void UpdateToolMenu()
        {
            var tool = page.editorLayer.CurrentTool;
            toolNone.IsChecked = (tool == XCanvas.Tool.None);
            toolSelection.IsChecked = (tool == XCanvas.Tool.Selection);
            toolWire.IsChecked = (tool == XCanvas.Tool.Wire);
            toolPin.IsChecked = (tool == XCanvas.Tool.Pin);
            toolLine.IsChecked = (tool == XCanvas.Tool.Line);
            toolEllipse.IsChecked = (tool == XCanvas.Tool.Ellipse);
            toolRectangle.IsChecked = (tool == XCanvas.Tool.Rectangle);
            toolText.IsChecked = (tool == XCanvas.Tool.Text);
        }

        #endregion

        #region Block

        private void Block()
        {
            var block = page.editorLayer.CreateBlockFromSelected("Block");
            if (block != null)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "Json (*.json)|*.json",
                    FileName = "block"
                };

                if (dlg.ShowDialog() == true)
                {
                    var path = dlg.FileName;
                    Block(block, path);
                    System.Diagnostics.Process.Start("notepad", path);
                }
            }
        }

        private void Block(XBlock block, string path)
        {
            var serializer = new XJson();
            var json = serializer.JsonSerialize(block);
            using (var fs = System.IO.File.CreateText(path))
            {
                fs.Write(json);
            };
        }

        #endregion
    }
}
