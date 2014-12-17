using Logic.Core;
using Logic.Graph;
using Logic.Native;
using Logic.Serialization;
using Logic.Simulation;
using Logic.Util;
using Logic.ViewModels;
using Logic.WPF.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Logic.WPF
{
    public class AppMain
    {
        #region Properties

        public bool IsContextMenuOpen { get; set; }

        #endregion

        #region Fields

        private ILog _log = null;
        private Defaults _defaults = null;
        private string _defaultsPath = "Logic.WPF.lconfig";
        private MainViewModel _model = null;
        private IStringSerializer _serializer = null;
        private System.Threading.Timer _timer = null;
        private BoolSimulationFactory _simulationFactory = null;
        private Clock _clock = null;
        private IPage _pageToPaste = null;
        private IDocument _documentToPaste = null;

        #endregion

        #region Windows

        private void Close()
        {
            for (int i = 0; i < Application.Current.Windows.Count; i++)
            {
                Application.Current.Windows[i].Close();
            }
        }

        private void Invoke(Action callback)
        {
            Application.Current.Dispatcher.Invoke(callback);
        }

        #endregion

        #region Application

        public void Start()
        {
            try
            {
                InitializeDefaults();
                InitializeModel();
                InitializeMEF();
                InitializeView();
                ProjectNew();
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        public void Exit()
        {
            // log
            if (_log != null)
            {
                _log.Close();
            }

            // defaults
            if (_defaults != null)
            {
                Save<Defaults>(_defaultsPath, _defaults);
            }
        }

        #endregion

        #region ToCommand

        private ICommand ToCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            return new NativeCommand(execute, canExecute);
        }

        #endregion

        #region Mode

        public bool IsEditMode()
        {
            return _timer == null;
        }

        public bool IsSimulationMode()
        {
            return _timer != null;
        }

        #endregion

        #region Initialize

        private void InitializeDefaults()
        {
            _serializer = new Json();
            _simulationFactory = new BoolSimulationFactory();

            if (System.IO.File.Exists(_defaultsPath))
            {
                var defaults = Open<Defaults>(_defaultsPath);
                if (defaults != null)
                {
                    _defaults = defaults;
                }
            }

            if (_defaults == null)
            {
                _defaults = new Defaults();
                _defaults.Reset();
            }

            if (_defaults.EnableLog && !string.IsNullOrEmpty(_defaults.LogPath))
            {
                _log = new TraceLog();
                _log.Initialize(_defaults.LogPath);
            }
        }

        private void InitializeModel()
        {
            _model = new MainViewModel();

            _model.Log = _log;

            _model.Blocks = new ObservableCollection<XBlock>();
            _model.Templates = new ObservableCollection<ITemplate>();

            _model.FileName = null;
            _model.FilePath = null;

            _model.Tool = new ToolMenuModel();

            // view
            _model.GridView = new ViewViewModel();
            _model.TableView = new ViewViewModel();
            _model.FrameView = new ViewViewModel();

            // layers
            _model.ShapeLayer = new CanvasViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = _defaults.EnableSnap,
                SnapSize = _defaults.SnapSize
            };
            _model.BlockLayer = new CanvasViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = _defaults.EnableSnap,
                SnapSize = _defaults.SnapSize
            };
            _model.WireLayer = new CanvasViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = _defaults.EnableSnap,
                SnapSize = _defaults.SnapSize
            };
            _model.PinLayer = new CanvasViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = _defaults.EnableSnap,
                SnapSize = _defaults.SnapSize
            };
            _model.EditorLayer = new CanvasViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = _defaults.EnableSnap,
                SnapSize = _defaults.SnapSize
            };
            _model.OverlayLayer = new CanvasViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = _defaults.EnableSnap,
                SnapSize = _defaults.SnapSize
            };

            // editor
            _model.EditorLayer.Layers = _model;
            _model.EditorLayer.GetFilePath = this.GetFilePath;

            // overlay
            _model.OverlayLayer.IsOverlay = true;

            // serializer
            _model.Serializer = _serializer;

            // renderer
            IRenderer renderer = new NativeRenderer()
            {
                Zoom = 1.0,
                InvertSize = _defaults.InvertSize,
                PinRadius = _defaults.PinRadius,
                HitTreshold = _defaults.HitTreshold,
                ShortenWire = _defaults.ShortenWire,
                ShortenSize = _defaults.ShortenSize
            };

            _model.Renderer = renderer;

            _model.ShapeLayer.Renderer = renderer;
            _model.BlockLayer.Renderer = renderer;
            _model.WireLayer.Renderer = renderer;
            _model.PinLayer.Renderer = renderer;
            _model.EditorLayer.Renderer = renderer;
            _model.OverlayLayer.Renderer = renderer;

            // clipboard
            _model.Clipboard = new NativeTextClipboard();

            // history
            _model.History = new History<IPage>(new Bson());

            // tool
            _model.Tool.CurrentTool = ToolMenuModel.Tool.Selection;

            // commands
            _model.PageAddCommand = ToCommand(
                (p) => this.PageAdd(p),
                (p) => IsEditMode());

            _model.PageInsertBeforeCommand = ToCommand(
                (p) => this.PageInsertBefore(p),
                (p) => IsEditMode());

            _model.PageInsertAfterCommand = ToCommand(
                (p) => this.PageInsertAfter(p),
                (p) => IsEditMode());

            _model.PageCutCommand = ToCommand(
                (p) => this.PageCut(p),
                (p) => IsEditMode());

            _model.PageCopyCommand = ToCommand(
                (p) => this.PageCopy(p),
                (p) => IsEditMode());

            _model.PagePasteCommand = ToCommand(
                (p) => this.PagePaste(p),
                (p) => IsEditMode() && _pageToPaste != null);

            _model.PageDeleteCommand = ToCommand(
                (p) => this.PageDelete(p),
                (p) => IsEditMode());

            _model.DocumentAddCommand = ToCommand(
                (p) => this.DocumentAdd(p),
                (p) => IsEditMode());

            _model.DocumentInsertBeforeCommand = ToCommand(
                (p) => this.DocumentInsertBefore(p),
                (p) => IsEditMode());

            _model.DocumentInsertAfterCommand = ToCommand(
                (p) => this.DocumentInsertAfter(p),
                (p) => IsEditMode());

            _model.DocumentCutCommand = ToCommand(
                (p) => this.DocumentCut(p),
                (p) => IsEditMode());

            _model.DocumentCopyCommand = ToCommand(
                (p) => this.DocumentCopy(p),
                (p) => IsEditMode());

            _model.DocumentPasteCommand = ToCommand(
                (p) => this.DocumentPaste(p),
                (p) => IsEditMode() && _documentToPaste != null);

            _model.DocumentDeleteCommand = ToCommand(
                (p) => this.DocumentDelete(p),
                (p) => IsEditMode());

            _model.ProjectAddCommand = ToCommand(
                (p) => this.ProjectAdd(p),
                (p) => IsEditMode());

            _model.ProjectCutCommand = ToCommand(
                (p) => this.ProjectCut(p),
                (p) => IsEditMode());

            _model.ProjectCopyCommand = ToCommand(
                (p) => this.ProjectCopy(p),
                (p) => IsEditMode());

            _model.ProjectPasteCommand = ToCommand(
                (p) => this.ProjectPaste(p),
                (p) => IsEditMode());

            _model.ProjectDeleteCommand = ToCommand(
                (p) => this.ProjectDelete(p),
                (p) => IsEditMode());

            _model.SelectedItemChangedCommand = ToCommand(
                (p) => this.PageUpdateView(p),
                (p) => IsEditMode());

            _model.FileNewCommand = ToCommand(
                (p) => this.FileNew(),
                (p) => IsEditMode());

            _model.FileOpenCommand = ToCommand(
                (p) => this.FileOpen(),
                (p) => IsEditMode());

            _model.FileSaveCommand = ToCommand(
                (p) => this.FileSave(),
                (p) => IsEditMode());

            _model.FileSaveAsCommand = ToCommand(
                (p) => this.FileSaveAs(),
                (p) => IsEditMode());

            _model.FileSaveAsPDFCommand = ToCommand(
                (p) => this.FileSaveAsPDF(),
                (p) => IsEditMode());

            _model.FileExitCommand = ToCommand(
                (p) =>
                {
                    if (IsSimulationMode())
                    {
                        this.SimulationStop();
                    }

                    Close();
                },
                (p) => true);

            _model.EditUndoCommand = ToCommand(
                (p) => _model.Undo(),
                (p) => IsEditMode() && _model.History.CanUndo());

            _model.EditRedoCommand = ToCommand(
                (p) => _model.Redo(),
                (p) => IsEditMode() && _model.History.CanRedo());

            _model.EditCutCommand = ToCommand(
                (p) => _model.Cut(),
                (p) => IsEditMode() && _model.CanCopy());

            _model.EditCopyCommand = ToCommand(
                (p) => _model.Copy(),
                (p) => IsEditMode() && _model.CanCopy());

            _model.EditPasteCommand = ToCommand(
                (p) =>
                {
                    _model.Paste();
                    if (IsContextMenuOpen && _model.Renderer.Selected != null)
                    {
                        double minX = _model.Page.Template.Width;
                        double minY = _model.Page.Template.Height;
                        _model.EditorLayer.Min(_model.Renderer.Selected, ref minX, ref minY);
                        double x = _model.EditorLayer.RightX - minX;
                        double y = _model.EditorLayer.RightY - minY;
                        _model.EditorLayer.Move(_model.Renderer.Selected, x, y);
                    }
                },
                (p) => IsEditMode() && _model.CanPaste());

            _model.EditDeleteCommand = ToCommand(
                (p) => _model.SelectionDelete(),
                (p) => IsEditMode() && _model.HaveSelection());

            _model.EditSelectAllCommand = ToCommand(
                (p) =>
                {
                    _model.SelectAll();
                    _model.Invalidate();
                },
                (p) => IsEditMode());

            _model.EditAlignLeftBottomCommand = ToCommand(
                (p) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom);
                },
                (p) => IsEditMode());

            _model.EditAlignBottomCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom),
                (p) => IsEditMode());

            _model.EditAlignRightBottomCommand = ToCommand(
                (p) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom);
                },
                (p) => IsEditMode());

            _model.EditAlignLeftCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left),
                (p) => IsEditMode());

            _model.EditAlignCenterCenterCommand = ToCommand(
                (p) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Center);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Center);
                },
                (p) => IsEditMode());

            _model.EditAlignRightCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right),
                (p) => IsEditMode());

            _model.EditAlignLeftTopCommand = ToCommand(
                (p) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top);
                },
                (p) => IsEditMode());

            _model.EditAlignTopCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top),
                (p) => IsEditMode());

            _model.EditAlignRightTopCommand = ToCommand(
                (p) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top);
                },
                (p) => IsEditMode());

            _model.EditIncreaseTextSizeCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeSetTextSizeDelta(+1.0),
                (p) => IsEditMode());

            _model.EditDecreaseTextSizeCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeSetTextSizeDelta(-1.0),
                (p) => IsEditMode());

            _model.EditToggleFillCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeToggleFill(),
                (p) => IsEditMode());

            _model.EditToggleSnapCommand = ToCommand(
                (p) =>
                {
                    _defaults.EnableSnap = !_defaults.EnableSnap;
                    _model.ShapeLayer.EnableSnap = _defaults.EnableSnap;
                    _model.BlockLayer.EnableSnap = _defaults.EnableSnap;
                    _model.WireLayer.EnableSnap = _defaults.EnableSnap;
                    _model.PinLayer.EnableSnap = _defaults.EnableSnap;
                    _model.EditorLayer.EnableSnap = _defaults.EnableSnap;
                    _model.OverlayLayer.EnableSnap = _defaults.EnableSnap;
                },
                (p) => IsEditMode());

            _model.EditToggleInvertStartCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeToggleInvertStart(),
                (p) => IsEditMode());

            _model.EditToggleInvertEndCommand = ToCommand(
                (p) => _model.EditorLayer.ShapeToggleInvertEnd(),
                (p) => IsEditMode());

            _model.EditToggleShortenWireCommand = ToCommand(
                (p) =>
                {
                    _defaults.ShortenWire = !_defaults.ShortenWire;
                    _model.Renderer.ShortenWire = _defaults.ShortenWire;
                    _model.WireLayer.InvalidateVisual();
                },
                (p) => IsEditMode());

            _model.EditCancelCommand = ToCommand(
                (p) => _model.EditorLayer.MouseCancel(),
                (p) => IsEditMode());

            _model.ToolNoneCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.None,
                (p) => IsEditMode());

            _model.ToolSelectionCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Selection,
                (p) => IsEditMode());

            _model.ToolWireCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Wire,
                (p) => IsEditMode());

            _model.ToolPinCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Pin,
                (p) => IsEditMode());

            _model.ToolLineCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Line,
                (p) => IsEditMode());

            _model.ToolEllipseCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Ellipse,
                (p) => IsEditMode());

            _model.ToolRectangleCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Rectangle,
                (p) => IsEditMode());

            _model.ToolTextCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Text,
                (p) => IsEditMode());

            _model.ToolImageCommand = ToCommand(
                (p) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Image,
                (p) => IsEditMode());

            _model.BlockImportCommand = ToCommand(
                (p) => this.BlockImport(),
                (p) => IsEditMode());

            _model.BlockImportCodeCommand = ToCommand(
                (p) => this.BlocksImportFromCode(),
                (p) => IsEditMode());

            _model.BlockExportCommand = ToCommand(
                (p) => this.BlockExport(),
                (p) => IsEditMode() && _model.HaveSelection());

            _model.BlockExportAsCodeCommand = ToCommand(
                (p) => this.BlockExportAsCode(),
                (p) => IsEditMode() && _model.HaveSelection());

            _model.BlockInsertCommand = ToCommand(
                (p) =>
                {
                    XBlock block = p as XBlock;
                    if (block != null)
                    {
                        double x = IsContextMenuOpen ? _model.EditorLayer.RightX : 0.0;
                        double y = IsContextMenuOpen ? _model.EditorLayer.RightY : 0.0;
                        BlockInsert(block, x, y);
                    }
                },
                (p) => IsEditMode());

            _model.BlockDeleteCommand = ToCommand(
                (p) =>
                {
                    XBlock block = p as XBlock;
                    if (block != null)
                    {
                        _model.Blocks.Remove(block);
                    }
                },
                (p) => IsEditMode());

            _model.TemplateImportCommand = ToCommand(
                (p) => this.TemplateImport(),
                (p) => IsEditMode());

            _model.TemplateImportCodeCommand = ToCommand(
                (p) => this.TemplatesImportFromCode(),
                (p) => IsEditMode());

            _model.TemplateExportCommand = ToCommand(
                (p) => this.TemplateExport(),
                (p) => IsEditMode());

            _model.ApplyTemplateCommand = ToCommand(
                (p) =>
                {
                    ITemplate template = p as ITemplate;
                    if (template != null)
                    {
                        _model.Page.Template = template;
                        TemplateApply(template, _model.Renderer);
                        TemplateInvalidate();
                    }
                },
                (p) => IsEditMode());

            _model.SimulationStartCommand = ToCommand(
                (p) => this.SimulationStart(),
                (p) => IsEditMode());

            _model.SimulationStopCommand = ToCommand(
                (p) => this.SimulationStop(),
                (p) => IsSimulationMode());

            _model.SimulationRestartCommand = ToCommand(
                (p) => this.SimulationRestart(),
                (p) => IsSimulationMode());

            _model.SimulationPauseCommand = ToCommand(
                (p) => this.SimulationPause(),
                (p) => IsSimulationMode());

            _model.SimulationTickCommand = ToCommand(
                (p) => this.SimulationTick(_model.OverlayLayer.Simulations),
                (p) => IsSimulationMode() && _model.IsSimulationPaused);

            _model.SimulationCreateGraphCommand = ToCommand(
                (p) => this.Graph(),
                (p) => IsEditMode());

            _model.SimulationImportCodeCommand = ToCommand(
                (p) => this.SimulationImportFromCode(),
                (p) => IsEditMode());

            _model.SimulationOptionsCommand = ToCommand(
                (p) => this.SimulationOptions(),
                (p) => IsEditMode());
        }

        private void InitializeMEF()
        {
            try
            {
                var builder = new ConventionBuilder();
                builder.ForTypesDerivedFrom<XBlock>().Export<XBlock>();
                builder.ForTypesDerivedFrom<ITemplate>().Export<ITemplate>();
                builder.ForTypesDerivedFrom<BoolSimulation>()
                    .Export<BoolSimulation>()
                    .SelectConstructor(selector => selector.FirstOrDefault());

                var configuration = new ContainerConfiguration()
                        .WithAssembly(Assembly.GetExecutingAssembly())
                        .WithDefaultConventions(builder);

                using (var container = configuration.CreateContainer())
                {
                    var blocks = container.GetExports<XBlock>();
                    _model.Blocks = new ObservableCollection<XBlock>(blocks);

                    var templates = container.GetExports<ITemplate>();
                    _model.Templates = new ObservableCollection<ITemplate>(templates);

                    var simulations = container.GetExports<BoolSimulation>();
                    _simulationFactory.Register(simulations);
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        private void InitializeView()
        {
            var view = new MainView();
            view.Initialize(_model, this);
            view.Show();
        }

        #endregion

        #region Serialization

        public T Open<T>(string path) where T : class
        {
            try
            {
                using (var fs = System.IO.File.OpenText(path))
                {
                    string json = fs.ReadToEnd();
                    T item = _serializer.Deserialize<T>(json);
                    return item;
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
            return null;
        }

        public void Save<T>(string path, T item) where T : class
        {
            try
            {
                string json = _serializer.Serialize<T>(item);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        #endregion

        #region File

        private void FileNew()
        {
            _model.Renderer.Dispose();

            ProjectNew();

            _model.FileName = null;
            _model.FilePath = null;
        }

        private void FileOpen()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject"
            };

            if (dlg.ShowDialog() == true)
            {
                FileOpen(dlg.FileName);
            }
        }

        public void FileOpen(string path)
        {
            var project = Open<XProject>(path);
            if (project != null)
            {
                _pageToPaste = null;
                _documentToPaste = null;

                _model.SelectionReset();
                _model.Reset();
                _model.Invalidate();
                _model.Renderer.Dispose();
                _model.Project = project;
                _model.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                _model.FilePath = path;
                ProjectUpdateStyles(project);
                ProjectLoadFirstPage(project);
            }
        }

        private void FileSave()
        {
            if (!string.IsNullOrEmpty(_model.FilePath))
            {
                Save(_model.FilePath, _model.Project);
            }
            else
            {
                FileSaveAs();
            }
        }

        private void FileSaveAs()
        {
            string fileName = string.IsNullOrEmpty(_model.FilePath) ?
                "logic" : System.IO.Path.GetFileName(_model.FilePath);

            var dlg = new SaveFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject",
                FileName = fileName
            };

            if (dlg.ShowDialog() == true)
            {
                Save(dlg.FileName, _model.Project);
                _model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                _model.FilePath = dlg.FileName;
            }
        }

        private void FileSaveAsPDF()
        {
            string fileName = string.IsNullOrEmpty(_model.FilePath) ?
                "logic" : System.IO.Path.GetFileNameWithoutExtension(_model.FilePath);

            var dlg = new SaveFileDialog()
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = fileName
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    FileSaveAsPDF(path: dlg.FileName, ignoreStyles: true);
                }
                catch (Exception ex)
                {
                    if (_log != null)
                    {
                        _log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            }
        }

        private void FileSaveAsPDF(string path, bool ignoreStyles)
        {
            var writer = new PdfWriter()
            {
                Selected = null,
                InvertSize = _model.Renderer.InvertSize,
                PinRadius = _model.Renderer.PinRadius,
                HitTreshold = _model.Renderer.HitTreshold,
                EnablePinRendering = false,
                EnableGridRendering = false,
                ShortenWire = _model.Renderer.ShortenWire,
                ShortenSize = _model.Renderer.ShortenSize
            };

            if (ignoreStyles)
            {
                writer.TemplateStyleOverride = _model.Project.Styles
                    .Where(s => s.Name == "TemplateOverride")
                    .FirstOrDefault();

                writer.LayerStyleOverride = _model.Project.Styles
                    .Where(s => s.Name == "LayerOverride")
                    .FirstOrDefault();
            }

            writer.Create(
                path,
                _model.Project.Documents.SelectMany(d => d.Pages));

            System.Diagnostics.Process.Start(path);
        }

        #endregion

        #region Project

        private void ProjectNew()
        {
            // project
            var project = _defaults.EmptyProject();

            // layer styles
            IStyle shapeStyle = new XStyle()
            {
                Name = "Shape",
                Fill = new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                Thickness = 2.0
            };
            project.Styles.Add(shapeStyle);

            IStyle selectedShapeStyle = new XStyle()
            {
                Name = "Selected",
                Fill = new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                Thickness = 2.0
            };
            project.Styles.Add(selectedShapeStyle);

            IStyle selectionStyle = new XStyle()
            {
                Name = "Selection",
                Fill = new XColor() { A = 0x1F, R = 0x00, G = 0x00, B = 0xFF },
                Stroke = new XColor() { A = 0x9F, R = 0x00, G = 0x00, B = 0xFF },
                Thickness = 1.0
            };
            project.Styles.Add(selectionStyle);

            IStyle hoverStyle = new XStyle()
            {
                Name = "Overlay",
                Fill = new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                Thickness = 2.0
            };
            project.Styles.Add(hoverStyle);

            // simulation styles
            IStyle nullStateStyle = new XStyle()
            {
                Name = "NullState",
                Fill = new XColor() { A = 0xFF, R = 0x66, G = 0x66, B = 0x66 },
                Stroke = new XColor() { A = 0xFF, R = 0x66, G = 0x66, B = 0x66 },
                Thickness = 2.0
            };
            project.Styles.Add(nullStateStyle);

            IStyle trueStateStyle = new XStyle()
            {
                Name = "TrueState",
                Fill = new XColor() { A = 0xFF, R = 0xFF, G = 0x14, B = 0x93 },
                Stroke = new XColor() { A = 0xFF, R = 0xFF, G = 0x14, B = 0x93 },
                Thickness = 2.0
            };
            project.Styles.Add(trueStateStyle);

            IStyle falseStateStyle = new XStyle()
            {
                Name = "FalseState",
                Fill = new XColor() { A = 0xFF, R = 0x00, G = 0xBF, B = 0xFF },
                Stroke = new XColor() { A = 0xFF, R = 0x00, G = 0xBF, B = 0xFF },
                Thickness = 2.0
            };
            project.Styles.Add(falseStateStyle);

            // export override styles
            IStyle templateStyle = new XStyle()
            {
                Name = "TemplateOverride",
                Fill = new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                Thickness = 0.80
            };
            project.Styles.Add(templateStyle);

            IStyle layerStyle = new XStyle()
            {
                Name = "LayerOverride",
                Fill = new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                Stroke = new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                Thickness = 1.50
            };
            project.Styles.Add(layerStyle);

            // templates
            foreach (var template in _model.Templates)
            {
                project.Templates.Add(_model.Clone(template));
            }

            _model.Project = project;
            _model.Project.Documents.Add(_defaults.EmptyDocument());
            _model.Project.Documents[0].Pages.Add(_defaults.EmptyTitlePage());

            _pageToPaste = null;
            _documentToPaste = null;

            ProjectUpdateStyles(_model.Project);
            ProjectSetDefaultTemplate(_model.Project);
            ProjectLoadFirstPage(_model.Project);
        }

        private void ProjectUpdateStyles(IProject project)
        {
            var layers = new List<CanvasViewModel>();
            layers.Add(_model.ShapeLayer);
            layers.Add(_model.BlockLayer);
            layers.Add(_model.WireLayer);
            layers.Add(_model.PinLayer);
            layers.Add(_model.EditorLayer);
            layers.Add(_model.OverlayLayer);

            foreach (var layer in layers)
            {
                layer.ShapeStyle = project.Styles.Where(s => s.Name == "Shape").FirstOrDefault();
                layer.SelectedShapeStyle = project.Styles.Where(s => s.Name == "Selected").FirstOrDefault();
                layer.SelectionStyle = project.Styles.Where(s => s.Name == "Selection").FirstOrDefault();
                layer.HoverStyle = project.Styles.Where(s => s.Name == "Overlay").FirstOrDefault();
                layer.NullStateStyle = project.Styles.Where(s => s.Name == "NullState").FirstOrDefault();
                layer.TrueStateStyle = project.Styles.Where(s => s.Name == "TrueState").FirstOrDefault();
                layer.FalseStateStyle = project.Styles.Where(s => s.Name == "FalseState").FirstOrDefault();
            }
        }

        private void ProjectSetDefaultTemplate(IProject project)
        {
            ITemplate template = project
                .Templates
                .Where(t => t.Name == project.DefaultTemplate)
                .First();

            foreach (var document in project.Documents)
            {
                foreach (var page in document.Pages)
                {
                    page.Template = template;
                }
            }
        }

        private void ProjectLoadFirstPage(IProject project)
        {
            if (project.Documents != null &&
                project.Documents.Count > 0)
            {
                var document = project.Documents.FirstOrDefault();
                if (document != null
                    && document.Pages != null
                    && document.Pages.Count > 0)
                {
                    PageLoad(document.Pages.First(), true);
                }
            }
        }

        private void ProjectAdd(object parameter)
        {
            if (parameter is MainViewModel)
            {
                DocumentAdd(parameter);
            }
            else if (parameter is IDocument)
            {
                PageAdd(parameter);
            }
            else if (parameter is IPage)
            {
                PageInsertAfter(parameter);
            }
        }

        private void ProjectCut(object parameter)
        {
            if (parameter is IDocument)
            {
                DocumentCut(parameter);
            }
            else if (parameter is IPage)
            {
                PageCut(parameter);
            }
        }

        private void ProjectCopy(object parameter)
        {
            if (parameter is IDocument)
            {
                DocumentCopy(parameter);
            }
            else if (parameter is IPage)
            {
                PageCopy(parameter);
            }
        }

        private void ProjectPaste(object parameter)
        {
            if (parameter is IDocument)
            {
                DocumentPaste(parameter);
            }
            else if (parameter is IPage)
            {
                PagePaste(parameter);
            }
        }

        private void ProjectDelete(object parameter)
        {
            if (parameter is IDocument)
            {
                DocumentDelete(parameter);
            }
            else if (parameter is IPage)
            {
                PageDelete(parameter);
            }
        }

        #endregion

        #region Document

        private void DocumentAdd(object parameter)
        {
            if (parameter is MainViewModel)
            {
                IDocument document = _defaults.EmptyDocument();
                _model.Project.Documents.Add(document);
            }
        }

        private void DocumentInsertBefore(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument before = parameter as IDocument;

                IDocument document = _defaults.EmptyDocument();
                int index = _model.Project.Documents.IndexOf(before);

                _model.Project.Documents.Insert(index, document);
            }
        }

        private void DocumentInsertAfter(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument after = parameter as IDocument;

                IDocument document = _defaults.EmptyDocument();
                int index = _model.Project.Documents.IndexOf(after);

                _model.Project.Documents.Insert(index + 1, document);
            }
        }

        private void DocumentCut(object parameter)
        {
            if (parameter is IDocument)
            {
                _documentToPaste = parameter as IDocument;

                DocumentDelete(_documentToPaste);
            }
        }

        private void DocumentCopy(object parameter)
        {
            if (parameter is IDocument)
            {
                _documentToPaste = parameter as IDocument;
            }
        }

        private void DocumentPaste(object parameter)
        {
            if (parameter is IDocument && _documentToPaste != null)
            {
                try
                {
                    IDocument document = parameter as IDocument;

                    document.Name = _documentToPaste.Name;
                    document.Pages.Clear();

                    bool haveFirstPage = false;

                    foreach (var original in _documentToPaste.Pages)
                    {
                        ITemplate template = original.Template;
                        IPage copy = _model.ToPageWithoutTemplate(original);
                        string json = _serializer.Serialize(copy);
                        IPage page = _serializer.Deserialize<XPage>(json);

                        page.Template = template;

                        document.Pages.Add(page);

                        if (!haveFirstPage)
                        {
                            haveFirstPage = true;
                            PageLoad(page, true);
                        }
                    }

                    if (!haveFirstPage)
                    {
                        PageEmptyView();
                    }
                }
                catch (Exception ex)
                {
                    if (_log != null)
                    {
                        _log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            }
        }

        private void DocumentDelete(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument document = parameter as IDocument;
                _model.Project.Documents.Remove(document);

                PageEmptyView();
            }
        }

        #endregion

        #region Page

        private void PageEmptyView()
        {
            _model.Page = null;
            _model.Clear();
            _model.Reset();
            _model.Invalidate();
            TemplateReset();
            TemplateInvalidate();
        }

        private void PageUpdateView(object parameter)
        {
            if (parameter is IPage)
            {
                PageLoad(parameter as IPage, true);
            }
        }

        private void PageLoad(IPage page, bool activate)
        {
            page.IsActive = activate;

            _model.Reset();
            _model.SelectionReset();
            _model.Page = page;
            _model.Load(page);
            _model.Invalidate();

            _model.Renderer.Database = page.Database;

            TemplateApply(page.Template, _model.Renderer);
            TemplateInvalidate();
        }

        private void PageAdd(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument document = parameter as IDocument;
                IPage page = _defaults.EmptyTitlePage();
                page.Template = _model
                    .Project
                    .Templates
                    .Where(t => t.Name == _model.Project.DefaultTemplate)
                    .First();
                document.Pages.Add(page);
                PageLoad(page, false);
            }
        }

        private void PageInsertBefore(object parameter)
        {
            if (parameter is IPage)
            {
                IPage before = parameter as IPage;

                IPage page = _defaults.EmptyTitlePage();
                page.Template = _model
                    .Project
                    .Templates
                    .Where(t => t.Name == _model.Project.DefaultTemplate)
                    .First();

                IDocument document = _model
                    .Project
                    .Documents
                    .Where(d => d.Pages.Contains(before))
                    .First();
                int index = document.Pages.IndexOf(before);

                document.Pages.Insert(index, page);
                PageLoad(page, true);
            }
        }

        private void PageInsertAfter(object parameter)
        {
            IPage after = parameter as IPage;

            IPage page = _defaults.EmptyTitlePage();
            page.Template = _model
                .Project
                .Templates
                .Where(t => t.Name == _model.Project.DefaultTemplate)
                .First();

            IDocument document = _model
                .Project
                .Documents
                .Where(d => d.Pages.Contains(after))
                .First();
            int index = document.Pages.IndexOf(after);

            document.Pages.Insert(index + 1, page);
            PageLoad(page, true);
        }

        private void PageCut(object parameter)
        {
            if (parameter is IPage)
            {
                _pageToPaste = parameter as IPage;

                PageDelete(_pageToPaste);
            }
        }

        private void PageCopy(object parameter)
        {
            if (parameter is IPage)
            {
                _pageToPaste = parameter as IPage;
            }
        }

        private void PagePaste(object parameter)
        {
            if (parameter is IPage && _pageToPaste != null)
            {
                try
                {
                    IPage destination = parameter as IPage;
                    ITemplate template = _pageToPaste.Template;
                    IPage copy = _model.ToPageWithoutTemplate(_pageToPaste);
                    string json = _serializer.Serialize(copy);
                    IPage page = _serializer.Deserialize<XPage>(json);

                    page.Template = template;

                    IDocument document = _model
                        .Project
                        .Documents
                        .Where(d => d.Pages.Contains(destination))
                        .First();
                    int index = document.Pages.IndexOf(destination);
                    document.Pages[index] = page;

                    PageLoad(page, true);
                }
                catch (Exception ex)
                {
                    if (_log != null)
                    {
                        _log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            }
        }

        private void PageDelete(object parameter)
        {
            if (parameter is IPage)
            {
                IPage page = parameter as IPage;
                IDocument document = _model
                    .Project
                    .Documents
                    .Where(d => d.Pages.Contains(page)).FirstOrDefault();
                if (document != null && document.Pages != null)
                {
                    document.Pages.Remove(page);

                    PageEmptyView();
                }
            }
        }

        #endregion

        #region Block

        public void BlockInsert(XBlock block, double x, double y)
        {
            _model.Snapshot();
            XBlock copy = _model.EditorLayer.Insert(block, x, y);
            if (copy != null)
            {
                _model.EditorLayer.Connect(copy);
            }
        }

        private void BlockImport()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Logic Block (*.lblock)|*.lblock"
            };

            if (dlg.ShowDialog() == true)
            {
                var block = Open<XBlock>(dlg.FileName);
                if (block != null)
                {
                    _model.Blocks.Add(block);
                }
            }
        }

        private void BlockExportAsCode()
        {
            try
            {
                var block = _model.EditorLayer.BlockCreateFromSelected("BLOCK");
                if (block == null)
                    return;

                // block name
                string blockName = block.Name.ToUpper();

                // class name
                char[] a = block.Name.ToLower().ToCharArray();
                a[0] = char.ToUpper(a[0]);
                string className = new string(a);

                var dlg = new SaveFileDialog()
                {
                    Filter = "C# (*.cs)|*.cs",
                    FileName = className
                };

                if (dlg.ShowDialog() == true)
                {
                    string path = dlg.FileName;

                    string code = new CSharpCodeCreator().Generate(
                        block,
                        "Logic.Blocks",
                        className,
                        blockName);

                    using (var fs = System.IO.File.CreateText(path))
                    {
                        fs.Write(code);
                    };
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        private void BlocksImportFromCode()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                BlocksImportFromCode(dlg.FileNames);
            }
        }

        private void BlocksImportFromCode(string[] paths)
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
                            BlocksImport(csharp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        private void BlocksImport(string csharp)
        {
            IEnumerable<XBlock> exports = CSharpCodeImporter.Import<XBlock>(csharp, _log);
            if (exports != null)
            {
                foreach (var block in exports)
                {
                    _model.Blocks.Add(block);
                }
            }
        }

        private void BlockExport()
        {
            var block = _model.EditorLayer.BlockCreateFromSelected("BLOCK");
            if (block != null)
            {
                var dlg = new SaveFileDialog()
                {
                    Filter = "Logic Block (*.lblock)|*.lblock",
                    FileName = "block"
                };

                if (dlg.ShowDialog() == true)
                {
                    var path = dlg.FileName;
                    Save<XBlock>(path, block);
                    System.Diagnostics.Process.Start("notepad", path);
                }
            }
        }

        #endregion

        #region Path

        public string GetFilePath()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                return dlg.FileName;
            }
            return null;
        }

        #endregion

        #region Template

        private void TemplateApply(ITemplate template, IRenderer renderer)
        {
            _model.GridView.Container = template.Grid;
            _model.TableView.Container = template.Table;
            _model.FrameView.Container = template.Frame;

            _model.GridView.Renderer = renderer;
            _model.TableView.Renderer = renderer;
            _model.FrameView.Renderer = renderer;
        }

        private void TemplateReset()
        {
            _model.GridView.Container = null;
            _model.TableView.Container = null;
            _model.FrameView.Container = null;
        }

        public void TemplateInvalidate()
        {
            if (_model.GridView.InvalidateVisual != null)
            {
                _model.GridView.InvalidateVisual();
            }

            if (_model.TableView.InvalidateVisual != null)
            {
                _model.TableView.InvalidateVisual();
            }

            if (_model.FrameView.InvalidateVisual != null)
            {
                _model.FrameView.InvalidateVisual();
            }
        }

        private void TemplateImport()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate"
            };

            if (dlg.ShowDialog() == true)
            {
                var template = Open<XTemplate>(dlg.FileName);
                if (template != null)
                {
                    _model.Project.Templates.Add(template);
                }
            }
        }

        private void TemplatesImportFromCode()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                TemplatesImportFromCode(dlg.FileNames);
            }
        }

        private void TemplatesImportFromCode(string[] paths)
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
                            TemplatesImport(csharp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        private void TemplatesImport(string csharp)
        {
            IEnumerable<ITemplate> exports = CSharpCodeImporter.Import<ITemplate>(csharp, _log);
            if (exports != null)
            {
                foreach (var template in exports)
                {
                    _model.Project.Templates.Add(_model.Clone(template));
                }
            }
        }

        private void TemplateExport()
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate",
                FileName = _model.Page.Template.Name
            };

            if (dlg.ShowDialog() == true)
            {
                var template = _model.Clone(_model.Page.Template);
                var path = dlg.FileName;
                Save<XTemplate>(path, template);
                System.Diagnostics.Process.Start("notepad", path);
            }
        }

        #endregion

        #region Overlay

        private void OverlayInit(IDictionary<XBlock, BoolSimulation> simulations)
        {
            _model.SelectionReset();

            _model.OverlayLayer.EnableSimulationCache = true;
            _model.OverlayLayer.CacheRenderer = null;

            foreach (var simulation in simulations)
            {
                _model.BlockLayer.Hidden.Add(simulation.Key);
                _model.OverlayLayer.Shapes.Add(simulation.Key);
            }

            _model.EditorLayer.Simulations = simulations;
            _model.OverlayLayer.Simulations = simulations;

            _model.OverlayLayer.CacheRenderer = new NativeBoolSimulationRenderer()
            {
                Renderer = _model.OverlayLayer.Renderer,
                NullStateStyle = _model.OverlayLayer.NullStateStyle,
                TrueStateStyle = _model.OverlayLayer.TrueStateStyle,
                FalseStateStyle = _model.OverlayLayer.FalseStateStyle,
                Shapes = _model.OverlayLayer.Shapes,
                Simulations = _model.OverlayLayer.Simulations
            };

            _model.BlockLayer.InvalidateVisual();
            _model.OverlayLayer.InvalidateVisual();
        }

        private void OverlayReset()
        {
            _model.EditorLayer.Simulations = null;
            _model.OverlayLayer.Simulations = null;
            _model.OverlayLayer.CacheRenderer = null;

            _model.BlockLayer.Hidden.Clear();
            _model.OverlayLayer.Shapes.Clear();
            _model.BlockLayer.InvalidateVisual();
            _model.OverlayLayer.InvalidateVisual();
        }

        #endregion

        #region Graph

        private void Graph()
        {
            try
            {
                IPage temp = _model.ToPageWithoutTemplate(_model.Page);
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        GraphSave(context);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        private void GraphSave(PageGraphContext context)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Graph (*.txt)|*.txt",
                FileName = "graph"
            };

            if (dlg.ShowDialog() == true)
            {
                var path = dlg.FileName;
                GraphSave(path, context);
                System.Diagnostics.Process.Start("notepad", path);
            }
        }

        private void GraphSave(string path, PageGraphContext context)
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

        #region Simulation

        private void SimulationStart(IDictionary<XBlock, BoolSimulation> simulations)
        {
            _clock = new Clock(cycle: 0L, resolution: 100);
            _model.IsSimulationPaused = false;
            _timer = new System.Threading.Timer(
                (state) =>
                {
                    try
                    {
                        if (!_model.IsSimulationPaused)
                        {
                            SimulationTick(simulations);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_log != null)
                        {
                            _log.LogError("{0}{1}{2}",
                                ex.Message,
                                Environment.NewLine,
                                ex.StackTrace);
                        }

                        Invoke(() =>
                        {
                            if (IsSimulationMode())
                            {
                                SimulationStop();
                            }
                        });
                    }
                },
                null, 0, _clock.Resolution);
        }

        private void SimulationStart()
        {
            try
            {
                if (IsSimulationMode())
                {
                    return;
                }

                IPage temp = _model.ToPageWithoutTemplate(_model.Page);
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        var simulations = _simulationFactory.Create(context);
                        if (simulations != null)
                        {
                            OverlayInit(simulations);
                            SimulationStart(simulations);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        private void SimulationPause()
        {
            try
            {
                if (IsSimulationMode())
                {
                    _model.IsSimulationPaused = !_model.IsSimulationPaused;
                }
            }
            catch (Exception ex)
            {
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void SimulationTick(IDictionary<XBlock, BoolSimulation> simulations)
        {
            try
            {
                if (IsSimulationMode())
                {
                    _simulationFactory.Run(simulations, _clock);
                    _clock.Tick();
                    Invoke(() =>
                    {
                        _model.OverlayLayer.InvalidateVisual();
                    });
                }
            }
            catch (Exception ex)
            {
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void SimulationRestart()
        {
            SimulationStop();
            SimulationStart();
        }

        private void SimulationStop()
        {
            try
            {
                OverlayReset();

                if (IsSimulationMode())
                {
                    _timer.Dispose();
                    _timer = null;
                    _model.IsSimulationPaused = false;
                }
            }
            catch (Exception ex)
            {
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void SimulationImportFromCode()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                SimulationImportFromCode(dlg.FileNames);
            }
        }

        private void SimulationImportFromCode(string[] paths)
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
                            SimulationImport(csharp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        private void SimulationImport(string csharp)
        {
            IEnumerable<BoolSimulation> exports = CSharpCodeImporter.Import<BoolSimulation>(csharp, _log);
            if (exports != null)
            {
                _simulationFactory.Register(exports);
            }
        }

        private void SimulationOptions()
        {
            // TODO: Display simulation options window
        }

        #endregion
    }
}
