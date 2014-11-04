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

        public XLayers Layers { get; set; }

        [ImportMany(typeof(XBlock))]
        public IList<XBlock> Blocks { get; set; }

        #endregion

        #region Fields

        private XJson _json = new XJson();
        private string _pageFileName = string.Empty;
        private IRenderer _renderer;
        private Point _dragStartPoint;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitPage();
            InitBlocks();
            InitKeys();
            InitMenu();

            Compose();
        }

        #endregion

        #region Visual Parent

        public T FindVisualParent<T>(DependencyObject child) 
            where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
            {
                return null;
            }

            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindVisualParent<T>(parentObject);
            }
        } 

        #endregion

        #region Composition

        private void Compose()
        {
            Blocks = new ObservableCollection<XBlock>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            catalog.Catalogs.Add(new DirectoryCatalog("."));
            var container = new CompositionContainer(catalog);

            try
            {
                container.ComposeParts(this);
            }
            catch (CompositionException ex)
            {
                MessageBox.Show(ex.Message);
            }

            DataContext = this;
        }

        #endregion

        #region Initialize

        private void InitPage()
        {
            Layers = new XLayers();
            Layers.Template = controller.templateLayer;
            Layers.Blocks = controller.blockLayer;
            Layers.Wires = controller.wireLayer;
            Layers.Pins = controller.pinLayer;

            _renderer = new XRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };
            controller.templateLayer.Renderer = _renderer;
            controller.blockLayer.Renderer = _renderer;
            controller.wireLayer.Renderer = _renderer;
            controller.pinLayer.Renderer = _renderer;
            controller.editorLayer.Renderer = _renderer;

            controller.editorLayer.History = new XHistory<XPage>();
            controller.editorLayer.Layers = Layers;
            controller.editorLayer.CurrentTool = XCanvas.Tool.Selection;
            controller.editorLayer.AllowDrop = true;

            controller.editorLayer.DragEnter += (s, e) =>
            {
                if (!e.Data.GetDataPresent("Block") || s == e.Source)
                {
                    e.Effects = DragDropEffects.None;
                }
            };

            controller.editorLayer.Drop += (s, e) =>
            {
                Point point = e.GetPosition(controller.editorLayer);

                // block
                if (e.Data.GetDataPresent("Block"))
                {
                    var block = e.Data.GetData("Block") as XBlock;
                    if (block != null)
                    {
                        controller.editorLayer.History.Snapshot(
                            controller.editorLayer.Create("Page"));
                        var copy = controller.editorLayer.Insert(block, point.X, point.Y);
                        controller.editorLayer.Connect(copy);
                        e.Handled = true;
                    }
                }
                // files
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files != null && files.Length == 1)
                    {
                        controller.editorLayer.Load(files[0]);
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
        }

        private void InitKeys()
        {
            PreviewKeyDown += (s, e) =>
            {
                bool control = Keyboard.Modifiers == ModifierKeys.Control;

                switch (e.Key)
                {
                    // show json
                    case Key.J:
                        if (control)
                        {
                            var path = System.IO.Path.GetTempFileName() + ".json";
                            var page = controller.editorLayer.Create("Page");
                            controller.editorLayer.Save(path, page);
                            System.Diagnostics.Process.Start("notepad", path);
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
                        {
                            Cancel();
                        }
                        break;

                    // toggle fill
                    case Key.F:
                        ToggleFill();
                        break;

                    // '[' toggle invert start
                    case Key.OemOpenBrackets:
                        ToggleInvertStart();
                        break;
                    // ']' toggle invert end
                    case Key.OemCloseBrackets:
                        ToggleInvertEnd();
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
                        else
                        {
                            SetToolNone();
                        }
                        break;
                    // tool line
                    case Key.L:
                        SetToolLine();
                        break;
                    // tool ellipse
                    case Key.E:
                        SetToolEllipse();
                        break;
                    // tool rectangle
                    case Key.R:
                        SetToolRectangle();
                        break;
                    // tool text
                    case Key.T:
                        SetToolText();
                        break;
                    // tool wire
                    case Key.W:
                        SetToolWire();
                        break;
                    // tool pin
                    case Key.P:
                        SetToolPin();
                        break;

                    // toggle snap
                    case Key.G:
                        ToggleSnap();
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
                        else
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

        #region Clipboard

        private void CopyToClipboard(IList<IShape> shapes)
        {
            try
            {
                var json = _json.JsonSerialize(shapes);
                Clipboard.SetText(json, TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        #endregion

        #region Edit

        private void Undo()
        {
            var page = controller.editorLayer.History.Undo(
                controller.editorLayer.Create("Page"));
            if (page != null)
            {
                controller.editorLayer.SelectionReset();
                controller.editorLayer.Load(page);
            }
        }

        private void Redo()
        {
            var page = controller.editorLayer.History.Redo(
                controller.editorLayer.Create("Page"));
            if (page != null)
            {
                controller.editorLayer.SelectionReset();
                controller.editorLayer.Load(page);
            }
        }

        private void Cut()
        {
            if (_renderer.Selected != null
                && _renderer.Selected.Count > 0)
            {
                CopyToClipboard(_renderer.Selected.ToList());
                Delete();
            }
        }

        private void Copy()
        {
            if (_renderer.Selected != null
                && _renderer.Selected.Count > 0)
            {
                CopyToClipboard(_renderer.Selected.ToList());
            }
        }

        private void Paste()
        {
            try
            {
                if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
                {
                    var json = Clipboard.GetText(TextDataFormat.UnicodeText);
                    if (!string.IsNullOrEmpty(json))
                    {
                        Insert(json);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void Insert(string json)
        {
            var shapes = _json.JsonDeserialize<IList<IShape>>(json);
            if (shapes.Count > 0)
            {
                controller.editorLayer.Insert(shapes);
            }
        }

        private void Delete()
        {
            controller.editorLayer.SelectionDelete();
        }

        private void SelectAll()
        {
            controller.editorLayer.SelectAll();
        }

        private void ToggleFill()
        {
            controller.editorLayer.ToggleFill();
        }

        private void ToggleSnap()
        {
            controller.editorLayer.EnableSnap = !controller.editorLayer.EnableSnap;
        }

        private void ToggleInvertStart()
        {
            controller.editorLayer.ToggleInvertStart();
        }

        private void ToggleInvertEnd()
        {
            controller.editorLayer.ToggleInvertEnd();
        }

        private void IncreaseTextSize()
        {
            controller.editorLayer.SetTextSizeDelta(+1.0);
        }

        private void DecreaseTextSize()
        {
            controller.editorLayer.SetTextSizeDelta(-1.0);
        }

        private void AlignLeftBottom()
        {
            controller.editorLayer.SetTextHAlignment(HAlignment.Left);
            controller.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignBottom()
        {
            controller.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignRightBottom()
        {
            controller.editorLayer.SetTextHAlignment(HAlignment.Right);
            controller.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignLeft()
        {
            controller.editorLayer.SetTextHAlignment(HAlignment.Left);
        }

        private void AlignCenterCenter()
        {
            controller.editorLayer.SetTextHAlignment(HAlignment.Center);
            controller.editorLayer.SetTextVAlignment(VAlignment.Center);
        }

        private void AlignRight()
        {
            controller.editorLayer.SetTextHAlignment(HAlignment.Right);
        }

        private void AlignLeftTop()
        {
            controller.editorLayer.SetTextHAlignment(HAlignment.Left);
            controller.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignTop()
        {
            controller.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignRightTop()
        {
            controller.editorLayer.SetTextHAlignment(HAlignment.Right);
            controller.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void Cancel()
        {
            controller.editorLayer.Cancel();
        }

        #endregion

        #region Tool

        private void SetToolNone()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.None;
            UpdateToolMenu();
        }

        private void SetToolSelection()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.Selection;
            UpdateToolMenu();
        }

        private void SetToolLine()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.Line;
            UpdateToolMenu();
        }

        private void SetToolEllipse()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.Ellipse;
            UpdateToolMenu();
        }

        private void SetToolRectangle()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.Rectangle;
            UpdateToolMenu();
        }

        private void SetToolText()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.Text;
            UpdateToolMenu();
        }

        private void SetToolWire()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.Wire;
            UpdateToolMenu();
        }

        private void SetToolPin()
        {
            controller.editorLayer.CurrentTool = XCanvas.Tool.Pin;
            UpdateToolMenu();
        }

        private void UpdateToolMenu()
        {
            var tool = controller.editorLayer.CurrentTool;
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

        #region File

        private void New()
        {
            controller.editorLayer.New();
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
                controller.editorLayer.Load(dlg.FileName);
                _pageFileName = dlg.FileName;
            }
        }

        private void Save()
        {
            if (!string.IsNullOrEmpty(_pageFileName))
            {
                controller.editorLayer.Save(_pageFileName);
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
                controller.editorLayer.Save(dlg.FileName);
                _pageFileName = dlg.FileName;
            }
        }

        #endregion
    }
}
