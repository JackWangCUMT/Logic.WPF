using Logic.Core;
using Logic.Graph;
using Logic.Simulation;
using Logic.WPF.Page;
using Logic.WPF.Serialization;
using Logic.WPF.Simulation;
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

namespace Logic.WPF.Views
{
    public partial class MainView : Window
    {
        #region Properties

        public MainViewModel Model { get; set; }

        #endregion

        #region Fields

        private IStringSerializer _serializer = new Json();
        private IRenderer _renderer;
        private ITemplate _template;
        private Point _dragStartPoint;
        private System.Threading.Timer _timer = null;
        private Clock _clock = null;

        #endregion

        #region Constructor

        public MainView()
        {
            InitializeComponent();

            InitializeModel();
            InitializePage();
            InitializeBlocks();
            InitializeMEF();

            UpdateToolMenu();
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

        private void InitializeModel()
        {
            Model = new MainViewModel();

            Model.Blocks = new ObservableCollection<XBlock>();
            Model.Templates = new ObservableCollection<ITemplate>();

            Model.FileName = null;
            Model.FilePath = null;

            Model.FileNewCommand = new Command(
                (parameter) => this.New(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileOpenCommand = new Command
                ((parameter) => this.Open(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileSaveCommand = new Command(
                (parameter) => this.Save(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileSaveAsCommand = new Command(
                (parameter) => this.SaveAs(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileExitCommand = new Command(
                (parameter) => 
                {
                    if (IsSimulationRunning())
                    {
                        this.Stop();
                    }
                    this.Close();
                }, 
                (parameter) => true);

            Model.EditUndoCommand = new Command(
                (parameter) => this.Undo(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditRedoCommand = new Command
                ((parameter) => this.Redo(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditCutCommand = new Command(
                (parameter) => this.Cut(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditCopyCommand = new Command(
                (parameter) => this.Copy(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditPasteCommand = new Command(
                (parameter) => this.Paste(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditDeleteCommand = new Command(
                (parameter) => this.Delete(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditSelectAllCommand = new Command(
                (parameter) => this.SelectAll(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftBottomCommand = new Command(
                (parameter) => this.AlignLeftBottom(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignBottomCommand = new Command(
                (parameter) => this.AlignBottom(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightBottomCommand = new Command(
                (parameter) => this.AlignRightBottom(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftCommand = new Command(
                (parameter) => this.AlignLeft(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignCenterCenterCommand = new Command(
                (parameter) => this.AlignCenterCenter(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightCommand = new Command(
                (parameter) => this.AlignRight(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftTopCommand = new Command(
                (parameter) => this.AlignLeftTop(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignTopCommand = new Command(
                (parameter) => this.AlignTop(), 
                (parameter) => IsSimulationRunning() ? false : true);
           
            Model.EditAlignRightTopCommand = new Command
                ((parameter) => this.AlignRightTop(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditIncreaseTextSizeCommand = new Command(
                (parameter) => this.IncreaseTextSize(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditDecreaseTextSizeCommand = new Command(
                (parameter) => this.DecreaseTextSize(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleFillCommand = new Command(
                (parameter) => this.ToggleFill(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleSnapCommand = new Command(
                (parameter) => this.ToggleSnap(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertStartCommand = new Command(
                (parameter) => this.ToggleInvertStart(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertEndCommand = new Command(
                (parameter) => this.ToggleInvertEnd(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditCancelCommand = new Command(
                (parameter) => this.Cancel(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolNoneCommand = new Command(
                (parameter) => this.SetToolNone(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolSelectionCommand = new Command(
                (parameter) => this.SetToolSelection(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolWireCommand = new Command(
                (parameter) => this.SetToolWire(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolPinCommand = new Command(
                (parameter) => this.SetToolPin(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolLineCommand = new Command(
                (parameter) => this.SetToolLine(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolEllipseCommand = new Command(
                (parameter) => this.SetToolEllipse(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolRectangleCommand = new Command(
                (parameter) => this.SetToolRectangle(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolTextCommand = new Command(
                (parameter) => this.SetToolText(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockExportBlockCommand = new Command(
                (parameter) => this.Block(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockCreateCodeCommand = new Command(
                (parameter) => this.Code(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateImportCommand = new Command(
                (parameter) => this.ImportTemplate(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateExportCommand = new Command(
                (parameter) => this.ExportTemplate(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ApplyTemplateCommand = new Command(
                (parameter) =>
                {
                    ITemplate template = parameter as ITemplate;
                    if (template != null)
                    {
                        ApplyPageTemplate(template, _renderer);
                        _template = template;
                        InvalidatePageTemplate();
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationStartCommand = new Command(
                (parameter) => this.Start(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationStopCommand = new Command(
                (parameter) => this.Stop(), 
                (parameter) => IsSimulationRunning() ? true : false);

            Model.SimulationRestartCommand = new Command(
                (parameter) => this.Restart(), 
                (parameter) => IsSimulationRunning() ? true : false);

            Model.SimulationCreateGraphCommand = new Command(
                (parameter) => this.Graph(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationOptionsCommand = new Command(
                (parameter) => this.Options(), 
                (parameter) => IsSimulationRunning() ? false : true);
        }

        private void InitializePage()
        {
            // layers
            var layers = new NativeLayers();
            layers.Shapes = pageView.shapeLayer;
            layers.Blocks = pageView.blockLayer;
            layers.Wires = pageView.wireLayer;
            layers.Pins = pageView.pinLayer;
            layers.Overlay = pageView.overlayLayer;

            // editor
            pageView.editorLayer.Layers = layers;

            // overlay
            pageView.overlayLayer.IsOverlay = true;

            // renderer
            _renderer = new NativeRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };

            pageView.shapeLayer.Renderer = _renderer;
            pageView.blockLayer.Renderer = _renderer;
            pageView.wireLayer.Renderer = _renderer;
            pageView.pinLayer.Renderer = _renderer;
            pageView.editorLayer.Renderer = _renderer;
            pageView.overlayLayer.Renderer = _renderer;

            // template
            _template = new LogicPageTemplate();
            ApplyPageTemplate(_template, _renderer);

            // history
            pageView.editorLayer.History = new History<XPage>();

            // tool
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Selection;

            // drag & drop
            pageView.editorLayer.AllowDrop = true;

            pageView.editorLayer.DragEnter += (s, e) =>
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

            pageView.editorLayer.Drop += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                // block
                if (e.Data.GetDataPresent("Block"))
                {
                    try
                    {
                        XBlock block = e.Data.GetData("Block") as XBlock;
                        if (block != null)
                        {
                            pageView.editorLayer.History.Snapshot(
                                pageView.editorLayer.Layers.ToPage("Page", null));
                            Point point = e.GetPosition(pageView.editorLayer);
                            XBlock copy = pageView.editorLayer.Insert(block, point.X, point.Y);
                            if (copy != null)
                            {
                                pageView.editorLayer.Connect(copy);
                                e.Handled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError("{0}{1}{2}", 
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
                            string path = files[0];
                            if (!string.IsNullOrEmpty(path))
                            {
                                pageView.editorLayer.Load(path);
                                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                                Model.FilePath = path;
                                e.Handled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            };
        }

        private void InitializeBlocks()
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
        }

        private void InitializeMEF()
        {
            try
            {
                var builder = new RegistrationBuilder();
                builder.ForTypesDerivedFrom<XBlock>().Export<XBlock>();
                builder.ForTypesDerivedFrom<ITemplate>().Export<ITemplate>();

                var catalog = new AggregateCatalog();

                catalog.Catalogs.Add(
                    new AssemblyCatalog(
                        Assembly.GetExecutingAssembly(), builder));

                if (System.IO.Directory.Exists("./Blocks"))
                {
                    catalog.Catalogs.Add(
                        new DirectoryCatalog("./Blocks", builder));
                }

                if (System.IO.Directory.Exists("./Templates"))
                {
                    catalog.Catalogs.Add(
                        new DirectoryCatalog("./Templates", builder));
                }

                var container = new CompositionContainer(catalog);
                container.ComposeParts(Model);
            }
            catch (CompositionException ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }

            this.DataContext = Model;
        }

        #endregion

        #region File

        private void New()
        {
            pageView.editorLayer.New();
            Model.FileName = null;
            Model.FilePath = null;
        }

        private void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog(this) == true)
            {
                pageView.editorLayer.Load(dlg.FileName);
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                Model.FilePath = dlg.FileName;
            }
        }

        private void Save()
        {
            if (!string.IsNullOrEmpty(Model.FilePath))
            {
                pageView.editorLayer.Save(Model.FilePath);
            }
            else
            {
                SaveAs();
            }
        }

        private void SaveAs()
        {
            string fileName = string.IsNullOrEmpty(Model.FilePath) ?
                "shapes" : System.IO.Path.GetFileName(Model.FilePath);

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Json (*.json)|*.json",
                FileName = fileName
            };

            if (dlg.ShowDialog(this) == true)
            {
                pageView.editorLayer.Save(dlg.FileName);
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                Model.FilePath = dlg.FileName;
            }
        }

        #endregion

        #region Edit

        private void Undo()
        {
            pageView.editorLayer.Undo();
        }

        private void Redo()
        {
            pageView.editorLayer.Redo();
        }

        private void Cut()
        {
            pageView.editorLayer.Cut();
        }

        private void Copy()
        {
            pageView.editorLayer.Copy();
        }

        private void Paste()
        {
            pageView.editorLayer.Paste();
        }

        private void Delete()
        {
            pageView.editorLayer.SelectionDelete();
        }

        private void SelectAll()
        {
            pageView.editorLayer.SelectAll();
        }

        private void ToggleFill()
        {
            pageView.editorLayer.ToggleFill();
        }

        private void ToggleSnap()
        {
            pageView.editorLayer.EnableSnap = !pageView.editorLayer.EnableSnap;
        }

        private void ToggleInvertStart()
        {
            pageView.editorLayer.ToggleInvertStart();
        }

        private void ToggleInvertEnd()
        {
            pageView.editorLayer.ToggleInvertEnd();
        }

        private void IncreaseTextSize()
        {
            pageView.editorLayer.SetTextSizeDelta(+1.0);
        }

        private void DecreaseTextSize()
        {
            pageView.editorLayer.SetTextSizeDelta(-1.0);
        }

        private void AlignLeftBottom()
        {
            pageView.editorLayer.SetTextHAlignment(HAlignment.Left);
            pageView.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignBottom()
        {
            pageView.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignRightBottom()
        {
            pageView.editorLayer.SetTextHAlignment(HAlignment.Right);
            pageView.editorLayer.SetTextVAlignment(VAlignment.Bottom);
        }

        private void AlignLeft()
        {
            pageView.editorLayer.SetTextHAlignment(HAlignment.Left);
        }

        private void AlignCenterCenter()
        {
            pageView.editorLayer.SetTextHAlignment(HAlignment.Center);
            pageView.editorLayer.SetTextVAlignment(VAlignment.Center);
        }

        private void AlignRight()
        {
            pageView.editorLayer.SetTextHAlignment(HAlignment.Right);
        }

        private void AlignLeftTop()
        {
            pageView.editorLayer.SetTextHAlignment(HAlignment.Left);
            pageView.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignTop()
        {
            pageView.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void AlignRightTop()
        {
            pageView.editorLayer.SetTextHAlignment(HAlignment.Right);
            pageView.editorLayer.SetTextVAlignment(VAlignment.Top);
        }

        private void Cancel()
        {
            pageView.editorLayer.Cancel();
        }

        #endregion

        #region Tool

        private void SetToolNone()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.None;
            UpdateToolMenu();
        }

        private void SetToolSelection()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Selection;
            UpdateToolMenu();
        }

        private void SetToolLine()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Line;
            UpdateToolMenu();
        }

        private void SetToolEllipse()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Ellipse;
            UpdateToolMenu();
        }

        private void SetToolRectangle()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Rectangle;
            UpdateToolMenu();
        }

        private void SetToolText()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Text;
            UpdateToolMenu();
        }

        private void SetToolWire()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Wire;
            UpdateToolMenu();
        }

        private void SetToolPin()
        {
            pageView.editorLayer.CurrentTool = NativeCanvas.Tool.Pin;
            UpdateToolMenu();
        }

        private void UpdateToolMenu()
        {
            var tool = pageView.editorLayer.CurrentTool;
            toolNone.IsChecked = (tool == NativeCanvas.Tool.None);
            toolSelection.IsChecked = (tool == NativeCanvas.Tool.Selection);
            toolWire.IsChecked = (tool == NativeCanvas.Tool.Wire);
            toolPin.IsChecked = (tool == NativeCanvas.Tool.Pin);
            toolLine.IsChecked = (tool == NativeCanvas.Tool.Line);
            toolEllipse.IsChecked = (tool == NativeCanvas.Tool.Ellipse);
            toolRectangle.IsChecked = (tool == NativeCanvas.Tool.Rectangle);
            toolText.IsChecked = (tool == NativeCanvas.Tool.Text);
        }

        #endregion

        #region Block

        private void Block()
        {
            var block = pageView.editorLayer.CreateBlockFromSelected("Block");
            if (block != null)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "Json (*.json)|*.json",
                    FileName = "block"
                };

                if (dlg.ShowDialog(this) == true)
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
                var json = _serializer.Serialize(block);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                };
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void Code()
        {
            var block = pageView.editorLayer.CreateBlockFromSelected("Block");
            if (block == null)
                return;

            var view = new CodeView();
            view.Owner = this;
            view.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var vm = new CodeViewModel()
            {
                NamespaceName = "Blocks.Name",
                ClassName = "Name",
                BlockName = "NAME",
                ProjectPath = "Blocks.Name.csproj"
            };

            vm.BrowseCommand = new Command((parameter) => 
            { 
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "C# Project (*.csproj)|*.csproj",
                    FileName = vm.ProjectPath
                };

                if (dlg.ShowDialog(view) == true)
                {
                    vm.ProjectPath = dlg.FileName;
                }
            },
            (parameter) => true);

            vm.CreateCommand = new Command((parameter) => 
            {
                try
                {
                    new CSharpProjectCreator().Create(block, vm);
                }
                catch (Exception ex)
                {
                    Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }

                view.Close();
            },
            (parameter) => true);

            vm.CancelCommand = new Command((parameter) =>
            {
                view.Close();
            },
            (parameter) => true);

            view.DataContext = vm;
            view.ShowDialog();
        }

        #endregion

        #region Template

        private void ImportTemplate()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = OpenTemplate(dlg.FileName);
                ApplyPageTemplate(template, _renderer);
                _template = template;
                InvalidatePageTemplate();
            }
        }

        private void ExportTemplate()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Json (*.json)|*.json",
                FileName = _template.Name
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = new XTemplate()
                {
                    Width = _template.Width,
                    Height = _template.Height,
                    Name = _template.Name,
                    Grid = new XContainer()
                    {
                        Styles = new List<IStyle>(_template.Grid.Styles),
                        Shapes = new List<IShape>(_template.Grid.Shapes)
                    },
                    Table = new XContainer()
                    {
                        Styles = new List<IStyle>(_template.Table.Styles),
                        Shapes = new List<IShape>(_template.Table.Shapes)
                    },
                    Frame = new XContainer()
                    {
                        Styles = new List<IStyle>(_template.Frame.Styles),
                        Shapes = new List<IShape>(_template.Frame.Shapes)
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
                    var template = _serializer.Deserialize<XTemplate>(json);
                    return template;
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
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
                var json = _serializer.Serialize(template);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void ApplyPageTemplate(ITemplate template, IRenderer renderer)
        {
            pageView.Width = template.Width;
            pageView.Height = template.Height;

            pageView.gridView.Container = template.Grid;
            pageView.tableView.Container = template.Table;
            pageView.frameView.Container = template.Frame;

            pageView.gridView.Renderer = renderer;
            pageView.tableView.Renderer = renderer;
            pageView.frameView.Renderer = renderer;
        }

        private void InvalidatePageTemplate()
        {
            pageView.gridView.InvalidateVisual();
            pageView.tableView.InvalidateVisual();
            pageView.frameView.InvalidateVisual();
        }

        #endregion

        #region Simulation Overlay

        private void InitSimulationOverlay(IDictionary<XBlock, BoolSimulation> simulations)
        {
            pageView.editorLayer.SelectionReset();

            pageView.overlayLayer.EnableSimulationCache = true;
            pageView.overlayLayer.HaveSimulationCache = false;

            foreach (var simulation in simulations)
            {
                pageView.blockLayer.Hidden.Add(simulation.Key);
                pageView.overlayLayer.Shapes.Add(simulation.Key);
            }

            pageView.editorLayer.Simulations = simulations;
            pageView.overlayLayer.Simulations = simulations;

            pageView.blockLayer.InvalidateVisual();
            pageView.overlayLayer.InvalidateVisual();
        }

        private void ResetSimulationOverlay()
        {
            pageView.editorLayer.Simulations = null;
            pageView.overlayLayer.Simulations = null;

            pageView.overlayLayer.HaveSimulationCache = false;

            pageView.blockLayer.Hidden.Clear();
            pageView.overlayLayer.Shapes.Clear();
            pageView.blockLayer.InvalidateVisual();
            pageView.overlayLayer.InvalidateVisual();
        }

        #endregion

        #region Simulation Graph

        private void Graph()
        {
            try
            {
                XPage temp = pageView.editorLayer.Layers.ToPage("Page", null);
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
                Log.LogError("{0}{1}{2}",
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

            if (dlg.ShowDialog(this) == true)
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
                        Dispatcher.Invoke(() => pageView.overlayLayer.InvalidateVisual());
                    }
                    catch (Exception ex)
                    {
                        Log.LogError("{0}{1}{2}",
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

                XPage temp = pageView.editorLayer.Layers.ToPage("Page", null);
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
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
            }
            catch (Exception ex)
            {
                Log.LogError("{0}{1}{2}",
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
