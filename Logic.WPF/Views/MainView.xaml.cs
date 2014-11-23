using Logic.Core;
using Logic.Graph;
using Logic.Page;
using Logic.Serialization;
using Logic.Simulation;
using Logic.Templates;
using Logic.Util;
using Logic.Util.Parts;
using Logic.ViewModels;
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

        private IStringSerializer _serializer = null;
        private IRenderer _renderer = null;
        private ITemplate _template = null;
        private Point _dragStartPoint;
        private bool _isContextMenu = false;
        private System.Threading.Timer _timer = null;
        private Clock _clock = null;

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

        #region Constructor

        public MainView()
        {
            InitializeComponent();

            InitializeModel();
            InitializePage();
            InitializeBlocks();
            InitializeMEF();
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

            Model.Tool = new ToolMenuModel();

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
                (parameter) => Model.Layers.Editor.Undo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.History.CanUndo() ? false : true;
                });

            Model.EditRedoCommand = new Command
                ((parameter) => Model.Layers.Editor.Redo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.History.CanRedo() ? false : true;
                });

            Model.EditCutCommand = new Command(
                (parameter) => Model.Layers.Editor.Cut(),
                (parameter) => 
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.CanCopy() ? false : true;
                });

            Model.EditCopyCommand = new Command(
                (parameter) => Model.Layers.Editor.Copy(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.CanCopy() ? false : true;
                });

            Model.EditPasteCommand = new Command(
                (parameter) =>
                {
                    Model.Layers.Editor.Paste();
                    if (_isContextMenu && _renderer.Selected != null)
                    {
                        double minX = pageView.editorLayer.Width;
                        double minY = pageView.editorLayer.Height;
                        Model.Layers.Editor.GetMin(_renderer.Selected, ref minX, ref minY);
                        double x = Model.Layers.Editor.RightX - minX;
                        double y = Model.Layers.Editor.RightY - minY;
                        Model.Layers.Editor.Move(_renderer.Selected, x, y);
                    }
                },
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.CanPaste() ? false : true;
                });

            Model.EditDeleteCommand = new Command(
                (parameter) => Model.Layers.Editor.SelectionDelete(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.HaveSelected() ? false : true;
                });

            Model.EditSelectAllCommand = new Command(
                (parameter) => Model.Layers.Editor.SelectAll(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftBottomCommand = new Command(
                (parameter) =>
                {
                    Model.Layers.Editor.SetTextHAlignment(HAlignment.Left);
                    Model.Layers.Editor.SetTextVAlignment(VAlignment.Bottom);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignBottomCommand = new Command(
                (parameter) => Model.Layers.Editor.SetTextVAlignment(VAlignment.Bottom), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightBottomCommand = new Command(
                (parameter) =>
                {
                    Model.Layers.Editor.SetTextHAlignment(HAlignment.Right);
                    Model.Layers.Editor.SetTextVAlignment(VAlignment.Bottom);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftCommand = new Command(
                (parameter) => Model.Layers.Editor.SetTextHAlignment(HAlignment.Left), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignCenterCenterCommand = new Command(
                (parameter) =>
                {
                    Model.Layers.Editor.SetTextHAlignment(HAlignment.Center);
                    Model.Layers.Editor.SetTextVAlignment(VAlignment.Center);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightCommand = new Command(
                (parameter) => Model.Layers.Editor.SetTextHAlignment(HAlignment.Right), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftTopCommand = new Command(
                (parameter) =>
                {
                    Model.Layers.Editor.SetTextHAlignment(HAlignment.Left);
                    Model.Layers.Editor.SetTextVAlignment(VAlignment.Top);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignTopCommand = new Command(
                (parameter) => Model.Layers.Editor.SetTextVAlignment(VAlignment.Top), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightTopCommand = new Command
                ((parameter) =>
                {
                    Model.Layers.Editor.SetTextHAlignment(HAlignment.Right);
                    Model.Layers.Editor.SetTextVAlignment(VAlignment.Top);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditIncreaseTextSizeCommand = new Command(
                (parameter) => Model.Layers.Editor.SetTextSizeDelta(+1.0), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditDecreaseTextSizeCommand = new Command(
                (parameter) => Model.Layers.Editor.SetTextSizeDelta(-1.0), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleFillCommand = new Command(
                (parameter) => Model.Layers.Editor.ToggleFill(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleSnapCommand = new Command(
                (parameter) => Model.Layers.Editor.EnableSnap = !Model.Layers.Editor.EnableSnap, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertStartCommand = new Command(
                (parameter) => Model.Layers.Editor.ToggleInvertStart(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertEndCommand = new Command(
                (parameter) => Model.Layers.Editor.ToggleInvertEnd(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditCancelCommand = new Command(
                (parameter) => Model.Layers.Editor.Cancel(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolNoneCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.None, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolSelectionCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Selection, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolWireCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Wire, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolPinCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Pin, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolLineCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Line, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolEllipseCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Ellipse, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolRectangleCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Rectangle, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolTextCommand = new Command(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Text, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockImportCommand = new Command(
                (parameter) => this.ImportBlock(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockImportCodeCommand = new Command(
                (parameter) => this.ImportBlocksFromCode(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockExportCommand = new Command(
                (parameter) => this.ExportBlock(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.HaveSelected() ? false : true;
                });

            Model.BlockCreateProjectCommand = new Command(
                (parameter) => this.CreateProject(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.Layers.Editor.HaveSelected() ? false : true;
                });

            Model.InsertBlockCommand = new Command(
                (parameter) =>
                {
                    XBlock block = parameter as XBlock;
                    if (block != null)
                    {
                        double x = _isContextMenu ? Model.Layers.Editor.RightX : 0.0;
                        double y = _isContextMenu ? Model.Layers.Editor.RightY : 0.0;
                        InsertBlock(block, x, y);
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateImportCommand = new Command(
                (parameter) => this.ImportTemplate(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateImportCodeCommand = new Command(
                (parameter) => this.ImportTemplatesFromCode(),
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
                        ApplyTemplate(template, _renderer);
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

        public IProject Project { get; set; }
        public IDocument Document { get; set; }

        private void InitializeProject()
        {
            // project
            Project = new XProject()
            {
                Name = "Project",
                Styles = new ObservableCollection<IStyle>(),
                Templates = new ObservableCollection<ITemplate>(),
                Documents = new ObservableCollection<IDocument>()
            };

            // styles
            IStyle shapeStyle = new XStyle(
                name: "Shape",
                fill: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                thickness: 2.0);
            Project.Styles.Add(shapeStyle);

            IStyle selectedShapeStyle = new XStyle(
                name: "Selected",
                fill: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                thickness: 2.0);
            Project.Styles.Add(selectedShapeStyle);

            IStyle selectionStyle = new XStyle(
                name: "Selection",
                fill: new XColor() { A = 0x1F, R = 0x00, G = 0x00, B = 0xFF },
                stroke: new XColor() { A = 0x9F, R = 0x00, G = 0x00, B = 0xFF },
                thickness: 1.0);
            Project.Styles.Add(selectionStyle);

            IStyle hoverStyle = new XStyle(
                name: "Overlay",
                fill: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                thickness: 2.0);
            Project.Styles.Add(hoverStyle);

            IStyle nullStateStyle = new XStyle(
                name: "NullState",
                fill: new XColor() { A = 0xFF, R = 0x66, G = 0x66, B = 0x66 },
                stroke: new XColor() { A = 0xFF, R = 0x66, G = 0x66, B = 0x66 },
                thickness: 2.0);
            Project.Styles.Add(nullStateStyle);

            IStyle trueStateStyle = new XStyle(
                name: "TrueState",
                fill: new XColor() { A = 0xFF, R = 0xFF, G = 0x14, B = 0x93 },
                stroke: new XColor() { A = 0xFF, R = 0xFF, G = 0x14, B = 0x93 },
                thickness: 2.0);
            Project.Styles.Add(trueStateStyle);

            IStyle falseStateStyle = new XStyle(
                name: "FalseState",
                fill: new XColor() { A = 0xFF, R = 0x00, G = 0xBF, B = 0xFF },
                stroke: new XColor() { A = 0xFF, R = 0x00, G = 0xBF, B = 0xFF },
                thickness: 2.0);
            Project.Styles.Add(falseStateStyle);

            // templates
            Project.Templates.Add(new LogicPageTemplate());
            Project.Templates.Add(new ScratchpadPageTemplate());

            // documents
            Document = new XDocument()
            {
                Name = "Document",
                Pages = new ObservableCollection<IPage>()
            };
            Project.Documents.Add(Document);

            // pages
            Document.Pages.Add(
                new XPage()
                {
                    Name = "Page",
                    Shapes = new List<IShape>(),
                    Blocks = new List<IShape>(),
                    Pins = new List<IShape>(),
                    Wires = new List<IShape>(),
                    Template = null
                });

            // layers
            var layers = new List<XLayer>();
            layers.Add(Model.Layers.Shapes);
            layers.Add(Model.Layers.Blocks);
            layers.Add(Model.Layers.Wires);
            layers.Add(Model.Layers.Pins);
            layers.Add(Model.Layers.Editor);
            layers.Add(Model.Layers.Overlay);

            foreach (var layer in layers)
            {
                layer.ShapeStyle = shapeStyle;
                layer.SelectedShapeStyle = selectedShapeStyle;
                layer.SelectionStyle = selectionStyle;
                layer.HoverStyle = hoverStyle;
                layer.NullStateStyle = nullStateStyle;
                layer.TrueStateStyle = trueStateStyle;
                layer.FalseStateStyle = falseStateStyle;
            }
        }

        private void InitializePage()
        {
            // layers
            Model.Layers = new XLayers();
            Model.Layers.Shapes = pageView.shapeLayer.Layer;
            Model.Layers.Blocks = pageView.blockLayer.Layer;
            Model.Layers.Wires = pageView.wireLayer.Layer;
            Model.Layers.Pins = pageView.pinLayer.Layer;
            Model.Layers.Editor = pageView.editorLayer.Layer;
            Model.Layers.Overlay = pageView.overlayLayer.Layer;

            // project
            InitializeProject();

            // editor
            Model.Layers.Editor.Layers = Model.Layers;

            // overlay
            Model.Layers.Overlay.IsOverlay = true;

            // serializer
            _serializer = new Json();

            Model.Layers.Shapes.Serializer = _serializer;
            Model.Layers.Blocks.Serializer = _serializer;
            Model.Layers.Wires.Serializer = _serializer;
            Model.Layers.Pins.Serializer = _serializer;
            Model.Layers.Editor.Serializer = _serializer;
            Model.Layers.Overlay.Serializer = _serializer;

            // renderer
            _renderer = new XRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };

            Model.Layers.Shapes.Renderer = _renderer;
            Model.Layers.Blocks.Renderer = _renderer;
            Model.Layers.Wires.Renderer = _renderer;
            Model.Layers.Pins.Renderer = _renderer;
            Model.Layers.Editor.Renderer = _renderer;
            Model.Layers.Overlay.Renderer = _renderer;

            // template
            ApplyTemplate(new LogicPageTemplate(), _renderer);

            // history
            Model.Layers.Editor.History = new History<XPage>();

            // tool
            Model.Layers.Editor.Tool = Model.Tool;
            Model.Layers.Editor.Tool.CurrentTool = ToolMenuModel.Tool.Selection;

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
                            Point point = e.GetPosition(pageView.editorLayer);
                            InsertBlock(block, point.X, point.Y);
                            e.Handled = true;
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
                                Model.Layers.Editor.Load(path);
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

            // context menu
            pageView.ContextMenuOpening += (s, e) =>
            {
                if (Model.Layers.Editor.CurrentMode != XLayer.Mode.None)
                {
                    e.Handled = true;
                }
                else if (Model.Layers.Editor.SkipContextMenu == true)
                {
                    Model.Layers.Editor.SkipContextMenu = false;
                    e.Handled = true;
                }
                else
                {
                    _isContextMenu = true;
                }
            };

            pageView.ContextMenuClosing += (s, e) =>
            {
                _isContextMenu = false;
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
            Model.Layers.Editor.New();
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
                Model.Layers.Editor.Load(dlg.FileName);
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                Model.FilePath = dlg.FileName;
            }
        }

        private void Save()
        {
            if (!string.IsNullOrEmpty(Model.FilePath))
            {
                Model.Layers.Editor.Save(Model.FilePath);
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
                Model.Layers.Editor.Save(dlg.FileName);
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                Model.FilePath = dlg.FileName;
            }
        }

        #endregion

        #region Block

        private void InsertBlock(XBlock block, double x, double y)
        {
            Model.Layers.Editor.History.Snapshot(
                Model.Layers.Editor.Layers.ToPage("Page", null));
            XBlock copy = Model.Layers.Editor.Insert(block, x, y);
            if (copy != null)
            {
                Model.Layers.Editor.Connect(copy);
            }
        }

        private void ImportBlock()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var block = OpenBlock(dlg.FileName);
                if (block != null)
                {
                    Model.Blocks.Add(block);
                }
            }
        }

        private void CreateProject()
        {
            var block = Model.Layers.Editor.CreateBlockFromSelected("Block");
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

        private void ImportBlocksFromCode()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                ImportBlocksFromCode(dlg.FileNames);
            }
        }

        private void ImportBlocksFromCode(string[] paths)
        {
            try
            {
                foreach (var path in paths)
                {
                    using (var fs = System.IO.File.OpenText(path))
                    {
                        var csharp = fs.ReadToEnd();
                        if (!string.IsNullOrEmpty(csharp))
                        {
                            ImportBlocks(csharp);
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

        private void ImportBlocks(string csharp)
        {
            var part = new BlockPart() { Blocks = new List<XBlock>() };
            bool result = CSharpCodeImporter.Import<XBlock>(csharp, part);
            if (result == true)
            {
                foreach (var block in part.Blocks)
                {
                    Model.Blocks.Add(block);
                }
            }
        }

        private XBlock OpenBlock(string path)
        {
            try
            {
                using (var fs = System.IO.File.OpenText(path))
                {
                    var json = fs.ReadToEnd();
                    var block = _serializer.Deserialize<XBlock>(json);
                    return block;
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

        private void ExportBlock()
        {
            var block = Model.Layers.Editor.CreateBlockFromSelected("Block");
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
                    SaveBlock(block, path);
                    System.Diagnostics.Process.Start("notepad", path);
                }
            }
        }

        private void SaveBlock(XBlock block, string path)
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

        #endregion

        #region Template

        private void ApplyTemplate(ITemplate template, IRenderer renderer)
        {
            pageView.Width = template.Width;
            pageView.Height = template.Height;

            pageView.gridView.Container = template.Grid;
            pageView.tableView.Container = template.Table;
            pageView.frameView.Container = template.Frame;

            pageView.gridView.Renderer = renderer;
            pageView.tableView.Renderer = renderer;
            pageView.frameView.Renderer = renderer;

            _template = template;

            pageView.gridView.InvalidateVisual();
            pageView.tableView.InvalidateVisual();
            pageView.frameView.InvalidateVisual();
        }

        private void ImportTemplate()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Json (*.json)|*.json"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = OpenTemplate(dlg.FileName);
                if (template != null)
                {
                    Model.Templates.Add(template);
                }
            }
        }

        private void ImportTemplatesFromCode()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                ImportTemplatesFromCode(dlg.FileNames);
            }
        }

        private void ImportTemplatesFromCode(string[] paths)
        {
            try
            {
                foreach (var path in paths)
                {
                    using (var fs = System.IO.File.OpenText(path))
                    {
                        var csharp = fs.ReadToEnd();
                        if (!string.IsNullOrEmpty(csharp))
                        {
                            ImportTemplates(csharp);
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

        private void ImportTemplates(string csharp)
        {
            var part = new TemplatePart() { Templates = new List<ITemplate>() };
            bool result = CSharpCodeImporter.Import<ITemplate>(csharp, part);
            if (result == true)
            {
                foreach (var template in part.Templates)
                {
                    Model.Templates.Add(template);
                }
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

        private void ExportTemplate()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Json (*.json)|*.json",
                FileName = _template.Name
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = CreateTemplate(_template);
                var path = dlg.FileName;
                SaveTemplate(path, template);
                System.Diagnostics.Process.Start("notepad", path);
            }
        }

        private XTemplate CreateTemplate(ITemplate template)
        {
            return new XTemplate()
            {
                Width = template.Width,
                Height = template.Height,
                Name = template.Name,
                Grid = new XContainer()
                {
                    Styles = new List<IStyle>(template.Grid.Styles),
                    Shapes = new List<IShape>(template.Grid.Shapes)
                },
                Table = new XContainer()
                {
                    Styles = new List<IStyle>(template.Table.Styles),
                    Shapes = new List<IShape>(template.Table.Shapes)
                },
                Frame = new XContainer()
                {
                    Styles = new List<IStyle>(template.Frame.Styles),
                    Shapes = new List<IShape>(template.Frame.Shapes)
                }
            };
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

        #endregion

        #region Simulation Overlay

        private void InitSimulationOverlay(IDictionary<XBlock, BoolSimulation> simulations)
        {
            Model.Layers.Editor.SelectionReset();

            Model.Layers.Overlay.EnableSimulationCache = true;
            Model.Layers.Overlay.HaveSimulationCache = false;

            foreach (var simulation in simulations)
            {
                Model.Layers.Blocks.Hidden.Add(simulation.Key);
                Model.Layers.Overlay.Shapes.Add(simulation.Key);
            }

            Model.Layers.Editor.Simulations = simulations;
            Model.Layers.Overlay.Simulations = simulations;

            Model.Layers.Blocks.InvalidateVisual();
            Model.Layers.Overlay.InvalidateVisual();
        }

        private void ResetSimulationOverlay()
        {
            Model.Layers.Editor.Simulations = null;
            Model.Layers.Overlay.Simulations = null;

            Model.Layers.Overlay.HaveSimulationCache = false;

            Model.Layers.Blocks.Hidden.Clear();
            Model.Layers.Overlay.Shapes.Clear();
            Model.Layers.Blocks.InvalidateVisual();
            Model.Layers.Overlay.InvalidateVisual();
        }

        #endregion

        #region Simulation Graph

        private void Graph()
        {
            try
            {
                XPage temp = Model.Layers.Editor.Layers.ToPage("Page", null);
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
                        Dispatcher.Invoke(() => Model.Layers.Overlay.InvalidateVisual());
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

                XPage temp = Model.Layers.Editor.Layers.ToPage("Page", null);
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        var simulations = BoolSimulationFactory.Create(context);
                        if (simulations != null)
                        {

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
