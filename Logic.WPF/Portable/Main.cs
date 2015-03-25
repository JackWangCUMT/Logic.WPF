// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Native;
using Logic.Portable;
using Logic.Serialization;
using Logic.Simulation;
using Logic.Util;
using Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Logic.Portable
{
    public class Main
    {
        #region Properties

        public bool IsContextMenuOpen { get; set; }
        public Options Options { get; set; }

        #endregion

        #region Fields

        private Dependencies _dependencies = null;
        private ILog _log = null;
        private string _defaultsPath = "Logic.WPF.lconfig";
        private MainViewModel _model = null;
        private IStringSerializer _serializer = null;
        private System.Threading.Timer _timer = null;
        private BoolSimulationFactory _simulationFactory = null;
        private Clock _clock = null;
        private IPage _pageToPaste = null;
        private IDocument _documentToPaste = null;

        #endregion

        #region Constructor

        public Main(Dependencies dependencies)
        {
            _dependencies = dependencies;
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
            if (Options != null)
            {
                Save<Options>(_defaultsPath, Options);
            }
        }

        #endregion

        #region ToCommand

        private ICommand ToCommand(
            Action<object> execute, 
            Func<object, bool> canExecute)
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
                var defaults = Open<Options>(_defaultsPath);
                if (defaults != null)
                {
                    Options = defaults;
                }
            }

            if (Options == null)
            {
                Options = new Options();
                Options.Defaults();
            }

            if (Options.EnableLog && !string.IsNullOrEmpty(Options.LogPath))
            {
                _log = new TraceLog();
                _log.Initialize(Options.LogPath);
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
            _model.ShapeLayer = new LayerViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = Options.EnableSnap,
                SnapSize = Options.SnapSize
            };
            _model.BlockLayer = new LayerViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = Options.EnableSnap,
                SnapSize = Options.SnapSize
            };
            _model.WireLayer = new LayerViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = Options.EnableSnap,
                SnapSize = Options.SnapSize
            };
            _model.PinLayer = new LayerViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = Options.EnableSnap,
                SnapSize = Options.SnapSize
            };
            _model.EditorLayer = new LayerViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = Options.EnableSnap,
                SnapSize = Options.SnapSize
            };
            _model.OverlayLayer = new LayerViewModel()
            {
                Shapes = new ObservableCollection<IShape>(),
                Hidden = new HashSet<IShape>(),
                EnableSnap = Options.EnableSnap,
                SnapSize = Options.SnapSize
            };

            // editor
            _model.EditorLayer.Layers = _model;
            _model.EditorLayer.GetFilePath = this.GetFilePath;

            // overlay
            _model.OverlayLayer.IsOverlay = true;

            // serializer
            _model.Serializer = _serializer;

            // renderer
            IRenderer renderer = _dependencies.Renderer;
            renderer.Zoom = 1.0;
            renderer.InvertSize = Options.InvertSize;
            renderer.PinRadius = Options.PinRadius;
            renderer.HitTreshold = Options.HitTreshold;
            renderer.ShortenWire = Options.ShortenWire;
            renderer.ShortenSize = Options.ShortenSize;

            _model.Renderer = renderer;

            _model.ShapeLayer.Renderer = renderer;
            _model.BlockLayer.Renderer = renderer;
            _model.WireLayer.Renderer = renderer;
            _model.PinLayer.Renderer = renderer;
            _model.EditorLayer.Renderer = renderer;
            _model.OverlayLayer.Renderer = renderer;

            // clipboard
            _model.Clipboard = _dependencies.TextClipboard;

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

                    _dependencies.CurrentApplication.Close();
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
                    _model.InvalidateLayers();
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
                    Options.EnableSnap = !Options.EnableSnap;
                    _model.ShapeLayer.EnableSnap = Options.EnableSnap;
                    _model.BlockLayer.EnableSnap = Options.EnableSnap;
                    _model.WireLayer.EnableSnap = Options.EnableSnap;
                    _model.PinLayer.EnableSnap = Options.EnableSnap;
                    _model.EditorLayer.EnableSnap = Options.EnableSnap;
                    _model.OverlayLayer.EnableSnap = Options.EnableSnap;
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
                    Options.ShortenWire = !Options.ShortenWire;
                    _model.Renderer.ShortenWire = Options.ShortenWire;
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
                        if (_model.Page != null)
                        {
                            _model.Page.Template = template;
                            _model.ApplyTemplate(template, _model.Renderer);
                            _model.InvalidateTemplate();
                        }
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
            _dependencies.MainView.Initialize(_model, this);
            _dependencies.MainView.Show();
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
            var result = _dependencies.FileDialog.GetProjetFileNameToOpen();
            if (result.Success == true)
            {
                FileOpen(result.FileName);
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
                _model.InvalidateLayers();
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

            var result = _dependencies.FileDialog.GetProjetFileNameToSave(fileName);
            if (result.Success == true)
            {
                Save(result.FileName, _model.Project);
                _model.FileName = System.IO.Path.GetFileNameWithoutExtension(result.FileName);
                _model.FilePath = result.FileName;
            }
        }

        private void FileSaveAsPDF()
        {
            string fileName = string.IsNullOrEmpty(_model.FilePath) ?
                "logic" : System.IO.Path.GetFileNameWithoutExtension(_model.FilePath);

            var result = _dependencies.FileDialog.GetPdfFileNameToSave(fileName);
            if (result.Success == true)
            {
                try
                {
                    FileSaveAsPDF(path: result.FileName, ignoreStyles: true);
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
            var project = Options.EmptyProject();

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
            _model.Project.Documents.Add(Options.EmptyDocument());
            _model.Project.Documents[0].Pages.Add(Options.EmptyTitlePage());

            _pageToPaste = null;
            _documentToPaste = null;

            ProjectUpdateStyles(_model.Project);
            ProjectSetDefaultTemplate(_model.Project);
            ProjectLoadFirstPage(_model.Project);
        }

        private void ProjectUpdateStyles(IProject project)
        {
            var layers = new List<LayerViewModel>();
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
                    IPage page = document.Pages.First();
                    _model.LoadPage(page);
                    page.IsActive = true;
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
                IDocument document = Options.EmptyDocument();
                _model.Project.Documents.Add(document);
            }
        }

        private void DocumentInsertBefore(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument before = parameter as IDocument;

                IDocument document = Options.EmptyDocument();
                int index = _model.Project.Documents.IndexOf(before);

                _model.Project.Documents.Insert(index, document);
            }
        }

        private void DocumentInsertAfter(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument after = parameter as IDocument;

                IDocument document = Options.EmptyDocument();
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
                        IPage page = _model.Clone(original);
                        document.Pages.Add(page);
                        if (!haveFirstPage)
                        {
                            haveFirstPage = true;
                            _model.LoadPage(page);
                            page.IsActive = true;
                        }
                    }

                    if (!haveFirstPage)
                    {
                        _model.ClearPage();
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
                _model.ClearPage();
            }
        }

        #endregion

        #region Page

        private void PageUpdateView(object parameter)
        {
            if (parameter is IPage)
            {
                IPage page = parameter as IPage;
                _model.LoadPage(page);
                page.IsActive = true;
            }
        }

        private void PageAdd(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument document = parameter as IDocument;
                IPage page = Options.EmptyTitlePage();
                page.Template = _model
                    .Project
                    .Templates
                    .Where(t => t.Name == _model.Project.DefaultTemplate)
                    .First();
                document.Pages.Add(page);
                _model.LoadPage(page);
                page.IsActive = false;
            }
        }

        private void PageInsertBefore(object parameter)
        {
            if (parameter is IPage)
            {
                IPage before = parameter as IPage;

                IPage page = Options.EmptyTitlePage();
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
                _model.LoadPage(page);
                page.IsActive = true;
            }
        }

        private void PageInsertAfter(object parameter)
        {
            IPage after = parameter as IPage;

            IPage page = Options.EmptyTitlePage();
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
            _model.LoadPage(page);
            page.IsActive = true;
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
                    IPage page = _model.Clone(_pageToPaste);
                    IPage destination = parameter as IPage;
                    IDocument document = _model
                        .Project
                        .Documents
                        .Where(d => d.Pages.Contains(destination))
                        .First();
                    int index = document.Pages.IndexOf(destination);
                    document.Pages[index] = page;
                    _model.LoadPage(page);
                    page.IsActive = true;
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
                    _model.ClearPage();
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
            var result = _dependencies.FileDialog.GetBlockFileNameToOpen();
            if (result.Success == true)
            {
                var block = Open<XBlock>(result.FileName);
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

                var result = _dependencies.FileDialog.GetCSharpFileNameToSave(className);
                if (result.Success == true)
                {
                    string code = new CSharpCodeCreator().Generate(
                        block,
                        "Logic.Blocks",
                        className,
                        blockName);

                    using (var fs = System.IO.File.CreateText(result.FileName))
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
            var result = _dependencies.FileDialog.GetCSharpFileNamesToOpen();
            if (result.Success == true)
            {
                BlocksImportFromCode(result.FileNames);
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
                var result = _dependencies.FileDialog.GetBlockFileNameToSave("block");
                if (result.Success == true)
                {
                    Save<XBlock>(result.FileName, block);
                }
            }
        }

        #endregion

        #region Path

        public string GetFilePath()
        {
            var result = _dependencies.FileDialog.GetAllFileNameToOpen();
            if (result.Success == true)
            {
                return result.FileName;
            }

            return null;
        }

        #endregion

        #region Template

        private void TemplateImport()
        {
            var result = _dependencies.FileDialog.GetTemplateFileNameToOpen();
            if (result.Success == true)
            {
                var template = Open<XTemplate>(result.FileName);
                if (template != null)
                {
                    _model.Project.Templates.Add(template);
                }
            }
        }

        private void TemplatesImportFromCode()
        {
            var result = _dependencies.FileDialog.GetCSharpFileNamesToOpen();
            if (result.Success == true)
            {
                TemplatesImportFromCode(result.FileNames);
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
            var result = _dependencies.FileDialog.GetTemplateFileNameToSave(_model.Page.Template.Name);
            if (result.Success == true)
            {
                Save<XTemplate>(
                    result.FileName, 
                    _model.Clone(_model.Page.Template));
            }
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
            var result = _dependencies.FileDialog.GetGraphFileNameToSave("graph");
            if (result.Success == true)
            {
                GraphSave(result.FileName, context);
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

                        _dependencies.CurrentApplication.Invoke(() =>
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
                            _model.InitOverlay(simulations, new NativeBoolSimulationRenderer());
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
                    _dependencies.CurrentApplication.Invoke(() =>
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
                _model.ResetOverlay();

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
            var result = _dependencies.FileDialog.GetCSharpFileNamesToOpen();
            if (result.Success == true)
            {
                SimulationImportFromCode(result.FileNames);
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
