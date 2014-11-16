using Logic.Core;
using Logic.Graph;
using Logic.Simulation;
using Logic.WPF.Page;
using Logic.WPF.Templates;
using Logic.WPF.Util;
using Logic.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
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

        private XJson _serializer = new XJson();
        private IRenderer _renderer;
        private ITemplate _template;
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

        #region MEF

        private void Compose(object part, string path)
        {
            var builder = new RegistrationBuilder();
            builder.ForTypesDerivedFrom<XBlock>().Export<XBlock>();

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(
                new AssemblyCatalog(
                    Assembly.GetExecutingAssembly(), builder));
            catalog.Catalogs.Add(
                new DirectoryCatalog(path, builder));

            var container = new CompositionContainer(catalog);
            container.ComposeParts(part);
        }

        #endregion

        #region Initialize

        private void InitPage()
        {
            // layers
            var layers = new XLayers();
            layers.Shapes = page.shapeLayer;
            layers.Blocks = page.blockLayer;
            layers.Wires = page.wireLayer;
            layers.Pins = page.pinLayer;
            layers.Overlay = page.overlayLayer;

            // editor
            page.editorLayer.Layers = layers;

            // overlay
            page.overlayLayer.IsOverlay = true;

            // renderer
            _renderer = new XRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };

            page.shapeLayer.Renderer = _renderer;
            page.blockLayer.Renderer = _renderer;
            page.wireLayer.Renderer = _renderer;
            page.pinLayer.Renderer = _renderer;
            page.editorLayer.Renderer = _renderer;
            page.overlayLayer.Renderer = _renderer;

            // template
            _template = new XLogicPageTemplate();
            ApplyPageTemplate(_template, _renderer);

            // history
            page.editorLayer.History = new XHistory<XPage>();

            // tool
            page.editorLayer.CurrentTool = XCanvas.Tool.Selection;

            // drag & drop
            page.editorLayer.AllowDrop = true;

            page.editorLayer.DragEnter += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                if (!e.Data.GetDataPresent("Block") || s == e.Source)
                {
                    e.Effects = DragDropEffects.None;
                }
            };

            page.editorLayer.Drop += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                Point point = e.GetPosition(page.editorLayer);

                // block
                if (e.Data.GetDataPresent("Block"))
                {
                    try
                    {
                        var block = e.Data.GetData("Block") as XBlock;
                        if (block != null)
                        {
                            page.editorLayer.History.Snapshot(
                                page.editorLayer.Create("Page"));
                            var copy = page.editorLayer.Insert(block, point.X, point.Y);
                            if (copy != null)
                            {
                                page.editorLayer.Connect(copy);
                                e.Handled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        XLog.LogError("{0}{1}{2}", 
                            ex.Message, 
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
                // files
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    try
                    {
                        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                        if (files != null && files.Length == 1)
                        {
                            if (!string.IsNullOrEmpty(files[0]))
                            {
                                page.editorLayer.Load(files[0]);
                                e.Handled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        XLog.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            };
        }

        private void InitBlocks()
        {
            blocks.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                _dragStartPoint = e.GetPosition(null);
            };

            blocks.PreviewMouseMove += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

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
                Compose(this, "./Blocks");
            }
            catch (CompositionException ex)
            {
                XLog.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
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
                    // code
                    // block
                    case Key.B:
                        if (control)
                        {
                            Code();
                        }
                        else if (none)
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

                    // start simulation
                    case Key.F5:
                        Start();
                        break;
                    // stop simulation
                    case Key.F6:
                        Stop();
                        break;
                    // restart simulation
                    case Key.F7:
                        Restart();
                        break;
                    // create graph
                    case Key.F8:
                        Graph();
                        break;
                    // simulation options
                    case Key.F9:
                        Options();
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
            editToggleSnap.Click += (s, e) => ToggleSnap();
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

            blockExportBlock.Click += (s, e) => Block();
            blockCreateCode.Click += (s, e) => Code();

            templateImport.Click += (s, e) => ImportTemplate();
            templateExport.Click += (s, e) => ExportTemplate();

            simulationStart.Click += (s, e) => Start();
            simulationRestart.Click += (s, e) => Restart();
            simulationStop.Click += (s, e) => Stop();
            simulationCreateGraph.Click += (s, e) => Graph();
            simulationOptions.Click += (s, e) => Options();

            simulationStart.IsEnabled = true;
            simulationRestart.IsEnabled = false;
            simulationStop.IsEnabled = false;
            simulationCreateGraph.IsEnabled = true;
            simulationOptions.IsEnabled = true;
        } 

        #endregion

        #region File

        private void New()
        {
            if (IsSimulationRunning())
            {
                Stop();
            }

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
                if (IsSimulationRunning())
                {
                    Stop();
                }

                page.editorLayer.Load(dlg.FileName);
                _pageFileName = dlg.FileName;
            }
        }

        private void Save()
        {
            if (IsSimulationRunning())
            {
                return;
            }

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
            if (IsSimulationRunning())
            {
                return;
            }

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
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.Undo();
        }

        private void Redo()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.Redo();
        }

        private void Cut()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.Cut();
        }

        private void Copy()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.Copy();
        }

        private void Paste()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.Paste();
        }

        private void Delete()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SelectionDelete();
        }

        private void SelectAll()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SelectAll();
        }

        private void ToggleFill()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.ToggleFill();
        }

        private void ToggleSnap()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.EnableSnap = !page.editorLayer.EnableSnap;
        }

        private void ToggleInvertStart()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.ToggleInvertStart();
        }

        private void ToggleInvertEnd()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.ToggleInvertEnd();
        }

        private void IncreaseTextSize()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextSizeDelta(+1.0);
        }

        private void DecreaseTextSize()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextSizeDelta(-1.0);
        }

        private void AlignLeftBottom()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextHAlignment(HAlignment.Left);
            page.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignBottom()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignRightBottom()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextHAlignment(HAlignment.Right);
            page.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignLeft()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextHAlignment(HAlignment.Left);
        }

        private void AlignCenterCenter()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextHAlignment(HAlignment.Center);
            page.editorLayer.SetTextVAlignment(VAlignment.Center);
        }

        private void AlignRight()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextHAlignment(HAlignment.Right);
        }

        private void AlignLeftTop()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextHAlignment(HAlignment.Left);
            page.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignTop()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignRightTop()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.SetTextHAlignment(HAlignment.Right);
            page.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void Cancel()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.Cancel();
        }

        #endregion

        #region Tool

        private void SetToolNone()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.CurrentTool = XCanvas.Tool.None;
            UpdateToolMenu();
        }

        private void SetToolSelection()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.CurrentTool = XCanvas.Tool.Selection;
            UpdateToolMenu();
        }

        private void SetToolLine()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.CurrentTool = XCanvas.Tool.Line;
            UpdateToolMenu();
        }

        private void SetToolEllipse()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.CurrentTool = XCanvas.Tool.Ellipse;
            UpdateToolMenu();
        }

        private void SetToolRectangle()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.CurrentTool = XCanvas.Tool.Rectangle;
            UpdateToolMenu();
        }

        private void SetToolText()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.CurrentTool = XCanvas.Tool.Text;
            UpdateToolMenu();
        }

        private void SetToolWire()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            page.editorLayer.CurrentTool = XCanvas.Tool.Wire;
            UpdateToolMenu();
        }

        private void SetToolPin()
        {
            if (IsSimulationRunning())
            {
                return;
            }

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
            if (IsSimulationRunning())
            {
                return;
            }

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
            try
            {
                var json = _serializer.JsonSerialize(block);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                };
            }
            catch (Exception ex)
            {
                XLog.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void Code()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            var block = page.editorLayer.CreateBlockFromSelected("Block");
            if (block == null)
                return;

            var window = new CodeWindow();
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var vm = new CodeViewModel()
            {
                NamespaceName = "Blocks.Name",
                ClassName = "Name",
                BlockName = "NAME",
                ProjectPath = "Blocks.Name.csproj"
            };

            vm.BrowseCommand = new XCommand(() => 
            { 
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "C# Project (*.csproj)|*.csproj",
                    FileName = vm.ProjectPath
                };

                if (dlg.ShowDialog(window) == true)
                {
                    vm.ProjectPath = dlg.FileName;
                }
            });

            vm.CreateCommand = new XCommand(() => 
            {
                try
                {
                    new CSharpProjectCreator().Create(block, vm);
                }
                catch (Exception ex)
                {
                    XLog.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }

                window.Close();
            });

            vm.CancelCommand = new XCommand(() =>
            {
                window.Close();
            });

            window.DataContext = vm;
            window.ShowDialog();
        }

        #endregion

        #region Template

        private void ImportTemplate()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog() == true)
            {
                var template = OpenTemplate(dlg.FileName);
                ApplyPageTemplate(template, _renderer);
                _template = template;
                InvalidatePageTemplate();
            }
        }

        private void ExportTemplate()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Json (*.json)|*.json",
                FileName = _template.Name
            };

            if (dlg.ShowDialog() == true)
            {
                var template = new XTemplate()
                {
                    Name = _template.Name,
                    Grid = new XContainer()
                    {
                        Styles = new ObservableCollection<IStyle>(_template.Grid.Styles),
                        Shapes = new ObservableCollection<IShape>(_template.Grid.Shapes)
                    },
                    Table = new XContainer()
                    {
                        Styles = new ObservableCollection<IStyle>(_template.Table.Styles),
                        Shapes = new ObservableCollection<IShape>(_template.Table.Shapes)
                    },
                    Frame = new XContainer()
                    {
                        Styles = new ObservableCollection<IStyle>(_template.Frame.Styles),
                        Shapes = new ObservableCollection<IShape>(_template.Frame.Shapes)
                    }
                };

                SaveTemplate(dlg.FileName, template);
            }
        }

        private ITemplate OpenTemplate(string path)
        {
            try
            {
                using (var fs = System.IO.File.OpenText(path))
                {
                    var json = fs.ReadToEnd();
                    var template = _serializer.JsonDeserialize<XTemplate>(json);
                    return template;
                }
            }
            catch (Exception ex)
            {
                XLog.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        private void SaveTemplate(string path, ITemplate template)
        {
            try
            {
                var json = _serializer.JsonSerialize(template);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                }
            }
            catch (Exception ex)
            {
                XLog.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void ApplyPageTemplate(ITemplate template, IRenderer renderer)
        {
            page.gridView.Container = template.Grid;
            page.tableView.Container = template.Table;
            page.frameView.Container = template.Frame;

            page.gridView.Renderer = renderer;
            page.tableView.Renderer = renderer;
            page.frameView.Renderer = renderer;
        }

        private void InvalidatePageTemplate()
        {
            page.gridView.InvalidateVisual();
            page.tableView.InvalidateVisual();
            page.frameView.InvalidateVisual();
        }

        #endregion

        #region Simulation Menu

        private void UpdateSimulationMenu()
        {
            bool isSimulationRunning = IsSimulationRunning();

            fileNew.IsEnabled = isSimulationRunning ? true : true;
            fileOpen.IsEnabled = isSimulationRunning ? true : true;
            fileSave.IsEnabled = isSimulationRunning ? false : true;
            fileSaveAs.IsEnabled = isSimulationRunning ? false : true;
            fileExit.IsEnabled = isSimulationRunning ? true : true;

            editUndo.IsEnabled = isSimulationRunning ? false : true;
            editRedo.IsEnabled = isSimulationRunning ? false : true;
            editCut.IsEnabled = isSimulationRunning ? false : true;
            editCopy.IsEnabled = isSimulationRunning ? false : true;
            editPaste.IsEnabled = isSimulationRunning ? false : true;
            editDelete.IsEnabled = isSimulationRunning ? false : true;
            editSelectAll.IsEnabled = isSimulationRunning ? false : true;

            editAlignLeftBottom.IsEnabled = isSimulationRunning ? false : true;
            editAlignBottom.IsEnabled = isSimulationRunning ? false : true;
            editAlignRightBottom.IsEnabled = isSimulationRunning ? false : true;
            editAlignLeft.IsEnabled = isSimulationRunning ? false : true;
            editAlignCenterCenter.IsEnabled = isSimulationRunning ? false : true;
            editAlignRight.IsEnabled = isSimulationRunning ? false : true;
            editAlignLeftTop.IsEnabled = isSimulationRunning ? false : true;
            editAlignTop.IsEnabled = isSimulationRunning ? false : true;
            editAlignRightTop.IsEnabled = isSimulationRunning ? false : true;

            editIncreaseTextSize.IsEnabled = isSimulationRunning ? false : true;
            editDecreaseTextSize.IsEnabled = isSimulationRunning ? false : true;

            editToggleFill.IsEnabled = isSimulationRunning ? false : true;
            editToggleSnap.IsEnabled = isSimulationRunning ? false : true;
            editToggleInvertStart.IsEnabled = isSimulationRunning ? false : true;
            editToggleInvertEnd.IsEnabled = isSimulationRunning ? false : true;

            editCancel.IsEnabled = isSimulationRunning ? false : true;

            toolNone.IsEnabled = isSimulationRunning ? false : true;
            toolSelection.IsEnabled = isSimulationRunning ? false : true;
            toolWire.IsEnabled = isSimulationRunning ? false : true;
            toolPin.IsEnabled = isSimulationRunning ? false : true;
            toolLine.IsEnabled = isSimulationRunning ? false : true;
            toolEllipse.IsEnabled = isSimulationRunning ? false : true;
            toolRectangle.IsEnabled = isSimulationRunning ? false : true;
            toolText.IsEnabled = isSimulationRunning ? false : true;

            blockExportBlock.IsEnabled = isSimulationRunning ? false : true;
            blockCreateCode.IsEnabled = isSimulationRunning ? false : true;

            templateImport.IsEnabled = isSimulationRunning ? false : true;
            templateExport.IsEnabled = isSimulationRunning ? false : true;

            simulationStart.IsEnabled = isSimulationRunning ? false : true;
            simulationRestart.IsEnabled = isSimulationRunning ? true : false;
            simulationStop.IsEnabled = isSimulationRunning ? true : false;
            simulationCreateGraph.IsEnabled = isSimulationRunning ? false : true;
            simulationOptions.IsEnabled = isSimulationRunning ? false : true;
        }

        #endregion

        #region Simulation Overlay

        private void InitSimulationOverlay(IDictionary<XBlock, BoolSimulation> simulations)
        {
            page.editorLayer.SelectionReset();

            foreach (var simulation in simulations)
            {
                page.blockLayer.Hidden.Add(simulation.Key);
                page.overlayLayer.Shapes.Add(simulation.Key);
            }

            page.editorLayer.Simulations = simulations;
            page.overlayLayer.Simulations = simulations;

            page.blockLayer.InvalidateVisual();
            page.overlayLayer.InvalidateVisual();
        }

        private void ResetSimulationOverlay()
        {
            page.editorLayer.Simulations = null;
            page.overlayLayer.Simulations = null;

            page.blockLayer.Hidden.Clear();
            page.overlayLayer.Shapes.Clear();
            page.blockLayer.InvalidateVisual();
            page.overlayLayer.InvalidateVisual();
        }

        #endregion

        #region Simulation Graph

        private void Graph()
        {
            if (IsSimulationRunning())
            {
                return;
            }

            try
            {
                var temp = page.editorLayer.Create("Page");
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        SaveGraph(context);
                    }
                }
            }
            catch (Exception ex)
            {
                XLog.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void SaveGraph(PageGraphContext context)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Graph (*.txt)|*.txt",
                FileName = "graph"
            };

            if (dlg.ShowDialog() == true)
            {
                var path = dlg.FileName;
                SaveGraph(path, context);
                System.Diagnostics.Process.Start("notepad", path);
            }
        }

        private void SaveGraph(string path, PageGraphContext context)
        {
            using (var writer = new System.IO.StringWriter())
            {
                PageGraphDebug.WriteConnections(context, writer);
                PageGraphDebug.WriteDependencies(context, writer);
                PageGraphDebug.WritePinTypes(context, writer);
                PageGraphDebug.WriteOrderedBlocks(context, writer);

                string text = writer.ToString();
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(text);
                };
            }
        }

        #endregion

        #region Simulation Mode

        private System.Threading.Timer _timer = null;
        private Clock _clock = null;

        private bool IsSimulationRunning()
        {
            return _timer != null;
        }

        private void Start(IDictionary<XBlock, BoolSimulation> simulations)
        {
            _clock = new Clock(cycle: 0L, resolution: 100);

            _timer = new System.Threading.Timer(
                (state) =>
                {
                    try
                    {
                        BoolSimulationFactory.Run(simulations, _clock);
                        _clock.Tick();
                        Dispatcher.Invoke(() => page.overlayLayer.InvalidateVisual());
                    }
                    catch (Exception ex)
                    {
                        XLog.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);

                        Dispatcher.Invoke(() =>
                        {
                            if (IsSimulationRunning())
                            {
                                Stop();
                            }
                        });
                    }
                }, 
                null, 0, _clock.Resolution);
        }

        private void Start()
        {
            try
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                var temp = page.editorLayer.Create("Page");
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        var simulations = BoolSimulationFactory.Create(context);
                        if (simulations != null)
                        {
                            SetToolSelection();
                            InitSimulationOverlay(simulations);
                            Start(simulations);
                            UpdateSimulationMenu();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XLog.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void Restart()
        {
            Stop();
            Start();
        }

        private void Stop()
        {
            try
            {
                ResetSimulationOverlay();

                if (IsSimulationRunning())
                {
                    _timer.Dispose();
                    _timer = null;
                }

                UpdateSimulationMenu();
            }
            catch (Exception ex)
            {
                XLog.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void Options()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
