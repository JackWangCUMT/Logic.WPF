using Logic.Core;
using Logic.Graph;
using Logic.Native;
using Logic.Serialization;
using Logic.Simulation;
using Logic.Util;
using Logic.ViewModels;
using Logic.WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Logic.WPF
{
    public partial class App : Application
    {
        #region Fields

        private ILog _log = null;
        private MainViewModel _model = null;
        private MainView _view = null;
        private IStringSerializer _serializer = null;
        private System.Threading.Timer _timer = null;
        private Clock _clock = null;
        private Point _dragStartPoint;
        private bool _isContextMenu = false;

        #endregion

        #region OnStartup

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _log = new Log();
            _log.IsEnabled = true;
            _log.Initialize();

            try
            {
                _view = new MainView();

                InitializeModel();
                InitializeView();
                InitializeBlocks();
                InitializeMEF();
                InitializeProject();

                _view.DataContext = _model;
                _view.Show();
            }
            catch (Exception ex)
            {
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        } 

        #endregion

        #region OnExit

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _log.Close();
        } 

        #endregion

        #region Initialize

        private void InitializeModel()
        {
            _model = new MainViewModel();

            _model.Log = _log;

            _model.Blocks = new ObservableCollection<XBlock>();
            _model.Templates = new ObservableCollection<ITemplate>();

            _model.FileName = null;
            _model.FilePath = null;

            _model.Tool = new ToolMenuModel();

            _model.PageAddCommand = new NativeCommand(
                (parameter) => this.PageAdd(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.PageInsertBeforeCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.PageInsertAfterCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.PageCutCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.PageCopyCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.PagePasteCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.PageDeleteCommand = new NativeCommand(
                (parameter) => this.PageDelete(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.DocumentAddCommand = new NativeCommand(
                (parameter) => this.DocumentAdd(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.DocumentInsertBeforeCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.DocumentInsertAfterCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.DocumentCutCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.DocumentCopyCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.DocumentPasteCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.DocumentDeleteCommand = new NativeCommand(
                (parameter) => this.DocumentDelete(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.SelectedItemChangedCommand = new NativeCommand(
                (parameter) => this.PageUpdateView(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.FileNewCommand = new NativeCommand(
                (parameter) => this.FileNew(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.FileOpenCommand = new NativeCommand
                ((parameter) => this.FileOpen(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.FileSaveCommand = new NativeCommand(
                (parameter) => this.FileSave(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.FileSaveAsCommand = new NativeCommand(
                (parameter) => this.FileSaveAs(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.FileSaveAsPDFCommand = new NativeCommand(
                (parameter) => this.FileSaveAsPDF(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.FileExitCommand = new NativeCommand(
                (parameter) =>
                {
                    if (IsSimulationRunning())
                    {
                        this.SimulationStop();
                    }
                    _view.Close();
                },
                (parameter) => true);

            _model.EditUndoCommand = new NativeCommand(
                (parameter) => _model.Undo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.History.CanUndo() ? false : true;
                });

            _model.EditRedoCommand = new NativeCommand(
                (parameter) => _model.Redo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.History.CanRedo() ? false : true;
                });

            _model.EditCutCommand = new NativeCommand(
                (parameter) => _model.Cut(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.CanCopy() ? false : true;
                });

            _model.EditCopyCommand = new NativeCommand(
                (parameter) => _model.Copy(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.CanCopy() ? false : true;
                });

            _model.EditPasteCommand = new NativeCommand(
                (parameter) =>
                {
                    _model.Paste();
                    if (_isContextMenu && _model.Renderer.Selected != null)
                    {
                        double minX = _model.Page.Template.Width;
                        double minY = _model.Page.Template.Height;
                        _model.EditorLayer.Min(_model.Renderer.Selected, ref minX, ref minY);
                        double x = _model.EditorLayer.RightX - minX;
                        double y = _model.EditorLayer.RightY - minY;
                        _model.EditorLayer.Move(_model.Renderer.Selected, x, y);
                    }
                },
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.CanPaste() ? false : true;
                });

            _model.EditDeleteCommand = new NativeCommand(
                (parameter) => _model.SelectionDelete(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.HaveSelection() ? false : true;
                });

            _model.EditSelectAllCommand = new NativeCommand(
                (parameter) =>
                {
                    _model.SelectAll();
                    _model.Invalidate();
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignLeftBottomCommand = new NativeCommand(
                (parameter) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignBottomCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignRightBottomCommand = new NativeCommand(
                (parameter) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignLeftCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignCenterCenterCommand = new NativeCommand(
                (parameter) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Center);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Center);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignRightCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignLeftTopCommand = new NativeCommand(
                (parameter) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignTopCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditAlignRightTopCommand = new NativeCommand
                ((parameter) =>
                {
                    _model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right);
                    _model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditIncreaseTextSizeCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeSetTextSizeDelta(+1.0),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditDecreaseTextSizeCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeSetTextSizeDelta(-1.0),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditToggleFillCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeToggleFill(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditToggleSnapCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.EnableSnap = !_model.EditorLayer.EnableSnap,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditToggleInvertStartCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeToggleInvertStart(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditToggleInvertEndCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.ShapeToggleInvertEnd(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.EditCancelCommand = new NativeCommand(
                (parameter) => _model.EditorLayer.MouseCancel(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolNoneCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.None,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolSelectionCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Selection,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolWireCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Wire,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolPinCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Pin,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolLineCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Line,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolEllipseCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Ellipse,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolRectangleCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Rectangle,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolTextCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Text,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ToolImageCommand = new NativeCommand(
                (parameter) => _model.Tool.CurrentTool = ToolMenuModel.Tool.Image,
                (parameter) => IsSimulationRunning() ? false : true);

            _model.BlockImportCommand = new NativeCommand(
                (parameter) => this.BlockImport(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.BlockImportCodeCommand = new NativeCommand(
                (parameter) => this.BlocksImportFromCode(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.BlockExportCommand = new NativeCommand(
                (parameter) => this.BlockExport(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.HaveSelection() ? false : true;
                });

            _model.BlockExportAsCodeCommand = new NativeCommand(
                (parameter) => this.BlockExportAsCode(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !_model.HaveSelection() ? false : true;
                });

            _model.InsertBlockCommand = new NativeCommand(
                (parameter) =>
                {
                    XBlock block = parameter as XBlock;
                    if (block != null)
                    {
                        double x = _isContextMenu ? _model.EditorLayer.RightX : 0.0;
                        double y = _isContextMenu ? _model.EditorLayer.RightY : 0.0;
                        BlockInsert(block, x, y);
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.TemplateImportCommand = new NativeCommand(
                (parameter) => this.TemplateImport(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.TemplateImportCodeCommand = new NativeCommand(
                (parameter) => this.TemplatesImportFromCode(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.TemplateExportCommand = new NativeCommand(
                (parameter) => this.TemplateExport(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.ApplyTemplateCommand = new NativeCommand(
                (parameter) =>
                {
                    ITemplate template = parameter as ITemplate;
                    if (template != null)
                    {
                        _model.Page.Template = template;
                        TemplateApply(template, _model.Renderer);
                        TemplateInvalidate();
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            _model.SimulationStartCommand = new NativeCommand(
                (parameter) => this.SimulationStart(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.SimulationStopCommand = new NativeCommand(
                (parameter) => this.SimulationStop(),
                (parameter) => IsSimulationRunning() ? true : false);

            _model.SimulationRestartCommand = new NativeCommand(
                (parameter) => this.SimulationRestart(),
                (parameter) => IsSimulationRunning() ? true : false);

            _model.SimulationCreateGraphCommand = new NativeCommand(
                (parameter) => this.Graph(),
                (parameter) => IsSimulationRunning() ? false : true);

            _model.SimulationOptionsCommand = new NativeCommand(
                (parameter) => this.SimulationOptions(),
                (parameter) => IsSimulationRunning() ? false : true);
        }

        private void InitializeView()
        {
            // view
            _model.GridView = new ViewViewModel();
            _model.TableView = new ViewViewModel();
            _model.FrameView = new ViewViewModel();

            // layers
            _model.ShapeLayer = new CanvasViewModel();
            _model.BlockLayer = new CanvasViewModel();
            _model.WireLayer = new CanvasViewModel();
            _model.PinLayer = new CanvasViewModel();
            _model.EditorLayer = new CanvasViewModel();
            _model.OverlayLayer = new CanvasViewModel();

            // log
            _model.GridView.Log = _log;
            _model.TableView.Log = _log;
            _model.FrameView.Log = _log;

            _model.ShapeLayer.Log = _log;
            _model.BlockLayer.Log = _log;
            _model.WireLayer.Log = _log;
            _model.PinLayer.Log = _log;
            _model.EditorLayer.Log = _log;
            _model.OverlayLayer.Log = _log;

            // editor
            _model.EditorLayer.Layers = _model;
            _model.EditorLayer.GetFilePath = this.GetFilePath;

            // overlay
            _model.OverlayLayer.IsOverlay = true;

            // serializer
            _serializer = new Json(_log);
            _model.Serializer = _serializer;

            // renderer
            IRenderer renderer = new NativeRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
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
            _model.History = new History<IPage>(new Bson(_log));

            // tool
            _model.Tool = _model.Tool;
            _model.Tool.CurrentTool = ToolMenuModel.Tool.Selection;

            // drag & drop
            _view.pageView.AllowDrop = true;

            _view.pageView.DragEnter += (s, e) =>
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

            _view.pageView.Drop += (s, e) =>
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
                            Point point = e.GetPosition(_view.pageView);
                            BlockInsert(block, point.X, point.Y);
                            e.Handled = true;
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
                                FileOpen(path);
                                e.Handled = true;
                            }
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
            };

            // context menu
            _view.pageView.ContextMenuOpening += (s, e) =>
            {
                if (_model.EditorLayer.CurrentMode != CanvasViewModel.Mode.None)
                {
                    e.Handled = true;
                }
                else if (_model.EditorLayer.SkipContextMenu == true)
                {
                    _model.EditorLayer.SkipContextMenu = false;
                    e.Handled = true;
                }
                else
                {
                    if (_model.Renderer.Selected == null
                        && !IsSimulationRunning())
                    {
                        Point2 point = new Point2(
                            _model.EditorLayer.RightX,
                            _model.EditorLayer.RightY);
                        IShape shape = _model.HitTest(point);
                        if (shape != null && shape is XBlock)
                        {
                            _model.Selected = shape;
                            _model.HaveSelected = true;
                        }
                        else
                        {
                            _model.Selected = null;
                            _model.HaveSelected = false;
                        }
                    }
                    else
                    {
                        _model.Selected = null;
                        _model.HaveSelected = false;
                    }

                    _isContextMenu = true;
                }
            };

            _view.pageView.ContextMenuClosing += (s, e) =>
            {
                if (_model.Selected != null)
                {
                    _model.Invalidate();
                }

                _isContextMenu = false;
            };
        }

        private void InitializeBlocks()
        {
            _view.blocks.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (IsSimulationRunning())
                {
                    return;
                }

                _dragStartPoint = e.GetPosition(null);
            };

            _view.blocks.PreviewMouseMove += (s, e) =>
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
                    var listBoxItem = ((DependencyObject)e.OriginalSource).FindVisualParent<ListBoxItem>();
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
                var builder = new ConventionBuilder();
                builder.ForTypesDerivedFrom<XBlock>().Export<XBlock>();
                builder.ForTypesDerivedFrom<ITemplate>().Export<ITemplate>();

                var configuration = new ContainerConfiguration()
                        .WithAssembly(Assembly.GetExecutingAssembly())
                        .WithDefaultConventions(builder);

                using (var container = configuration.CreateContainer())
                {
                    var blocks = container.GetExports<XBlock>();
                    _model.Blocks = new ObservableCollection<XBlock>(blocks);

                    var templates = container.GetExports<ITemplate>();
                    _model.Templates = new ObservableCollection<ITemplate>(templates);
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

        private void InitializeProject()
        {
            _model.Project = NewProject();
            _model.Project.Documents.Add(Defaults.EmptyDocument());
            _model.Project.Documents[0].Pages.Add(Defaults.EmptyTitlePage());

            UpdateStyles(_model.Project);
            SetDefaultTemplate(_model.Project);
            LoadFirstPage(_model.Project);
        }

        #endregion

        #region Serialization

        public T Open<T>(string path) where T : class
        {
            try
            {
                using (var fs = System.IO.File.OpenText(path))
                {
                    var json = fs.ReadToEnd();
                    var project = _serializer.Deserialize<T>(json);
                    return project;
                }
            }
            catch (Exception ex)
            {
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
            return null;
        }

        public void Save<T>(string path, T project) where T : class
        {
            try
            {
                var json = _serializer.Serialize<T>(project);
                using (var fs = System.IO.File.CreateText(path))
                {
                    fs.Write(json);
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

        #endregion

        #region File

        private void FileNew()
        {
            _model.Renderer.Dispose();

            InitializeProject();

            _model.FileName = null;
            _model.FilePath = null;
        }

        private void FileOpen()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject"
            };

            if (dlg.ShowDialog(_view) == true)
            {
                FileOpen(dlg.FileName);
            }
        }

        private void FileOpen(string path)
        {
            var project = Open<XProject>(path);
            if (project != null)
            {
                _model.SelectionReset();
                _model.Reset();
                _model.Invalidate();
                _model.Renderer.Dispose();
                _model.Project = project;
                _model.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                _model.FilePath = path;
                UpdateStyles(project);
                LoadFirstPage(project);
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

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject",
                FileName = fileName
            };

            if (dlg.ShowDialog(_view) == true)
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

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = fileName
            };

            if (dlg.ShowDialog(_view) == true)
            {
                try
                {
                    FileSaveAsPDF(path: dlg.FileName, ignoreStyles: true);
                }
                catch (Exception ex)
                {
                    _log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
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
                EnableGridRendering = false
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

        private IProject NewProject()
        {
            // project
            var project = Defaults.EmptyProject();

            // layer styles
            IStyle shapeStyle = new NativeStyle(
                name: "Shape",
                fill: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                thickness: 2.0);
            project.Styles.Add(shapeStyle);

            IStyle selectedShapeStyle = new NativeStyle(
                name: "Selected",
                fill: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                thickness: 2.0);
            project.Styles.Add(selectedShapeStyle);

            IStyle selectionStyle = new NativeStyle(
                name: "Selection",
                fill: new XColor() { A = 0x1F, R = 0x00, G = 0x00, B = 0xFF },
                stroke: new XColor() { A = 0x9F, R = 0x00, G = 0x00, B = 0xFF },
                thickness: 1.0);
            project.Styles.Add(selectionStyle);

            IStyle hoverStyle = new NativeStyle(
                name: "Overlay",
                fill: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0xFF, G = 0x00, B = 0x00 },
                thickness: 2.0);
            project.Styles.Add(hoverStyle);

            // simulation styles
            IStyle nullStateStyle = new NativeStyle(
                name: "NullState",
                fill: new XColor() { A = 0xFF, R = 0x66, G = 0x66, B = 0x66 },
                stroke: new XColor() { A = 0xFF, R = 0x66, G = 0x66, B = 0x66 },
                thickness: 2.0);
            project.Styles.Add(nullStateStyle);

            IStyle trueStateStyle = new NativeStyle(
                name: "TrueState",
                fill: new XColor() { A = 0xFF, R = 0xFF, G = 0x14, B = 0x93 },
                stroke: new XColor() { A = 0xFF, R = 0xFF, G = 0x14, B = 0x93 },
                thickness: 2.0);
            project.Styles.Add(trueStateStyle);

            IStyle falseStateStyle = new NativeStyle(
                name: "FalseState",
                fill: new XColor() { A = 0xFF, R = 0x00, G = 0xBF, B = 0xFF },
                stroke: new XColor() { A = 0xFF, R = 0x00, G = 0xBF, B = 0xFF },
                thickness: 2.0);
            project.Styles.Add(falseStateStyle);

            // export override styles
            IStyle templateStyle = new XPdfStyle(
                name: "TemplateOverride",
                fill: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                thickness: 0.80);
            project.Styles.Add(templateStyle);

            IStyle layerStyle = new XPdfStyle(
                name: "LayerOverride",
                fill: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                stroke: new XColor() { A = 0xFF, R = 0x00, G = 0x00, B = 0x00 },
                thickness: 1.50);
            project.Styles.Add(layerStyle);

            // templates
            foreach (var template in _model.Templates)
            {
                project.Templates.Add(_model.Clone(template));
            }

            return project;
        }

        private void UpdateStyles(IProject project)
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

        private void SetDefaultTemplate(IProject project)
        {
            ITemplate template = project.Templates.Where(t => t.Name == "Logic Page").First();
            foreach (var document in project.Documents)
            {
                foreach (var page in document.Pages)
                {
                    page.Template = template;
                }
            }
        }

        private void LoadFirstPage(IProject project)
        {
            if (project.Documents != null &&
                project.Documents.Count > 0)
            {
                var document = project.Documents.FirstOrDefault();
                if (document != null
                    && document.Pages != null
                    && document.Pages.Count > 0)
                {
                    PageLoad(document.Pages.First());
                }
            }
        }

        #endregion

        #region Document

        private void DocumentAdd(object parameter)
        {
            if (parameter is MainViewModel)
            {
                IDocument document = Defaults.EmptyDocument();
                document.IsActive = true;
                _model.Project.Documents.Add(document);
            }
        }

        private void DocumentDelete(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument document = parameter as IDocument;
                _model.Project.Documents.Remove(document);

                _model.Page = null;
                _model.Clear();
                _model.Reset();
                _model.Invalidate();
                TemplateReset();
                TemplateInvalidate();
            }
        }

        #endregion

        #region Page

        private void PageUpdateView(object parameter)
        {
            if (parameter is IPage)
            {
                PageLoad(parameter as IPage);
            }
        }

        private void PageLoad(IPage page)
        {
            page.IsActive = true;

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
                IPage page = Defaults.EmptyTitlePage();
                page.Template = _model.Project.Templates.Where(t => t.Name == "Logic Page").First();
                page.IsActive = true;
                document.Pages.Add(page);
                PageLoad(page);
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

                    _model.Page = null;
                    _model.Clear();
                    _model.Reset();
                    _model.Invalidate();
                    TemplateReset();
                    TemplateInvalidate();
                }
            }
        }

        #endregion

        #region Block

        private void BlockInsert(XBlock block, double x, double y)
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
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Logic Block (*.lblock)|*.lblock"
            };

            if (dlg.ShowDialog(_view) == true)
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

                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "C# (*.cs)|*.cs",
                    FileName = className
                };

                if (dlg.ShowDialog(_view) == true)
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
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void BlocksImportFromCode()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog(_view) == true)
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
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
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
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "Logic Block (*.lblock)|*.lblock",
                    FileName = "block"
                };

                if (dlg.ShowDialog(_view) == true)
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
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog(_view) == true)
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

        private void TemplateInvalidate()
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
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate"
            };

            if (dlg.ShowDialog(_view) == true)
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
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog(_view) == true)
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
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
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
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate",
                FileName = _model.Page.Template.Name
            };

            if (dlg.ShowDialog(_view) == true)
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
                IPage temp = _model.ToPage();
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
                _log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void GraphSave(PageGraphContext context)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Graph (*.txt)|*.txt",
                FileName = "graph"
            };

            if (dlg.ShowDialog(_view) == true)
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

        private bool IsSimulationRunning()
        {
            return _timer != null;
        }

        private void SimulationStart(IDictionary<XBlock, BoolSimulation> simulations)
        {
            _clock = new Clock(cycle: 0L, resolution: 100);

            _timer = new System.Threading.Timer(
                (state) =>
                {
                    try
                    {
                        BoolSimulationFactory.Run(simulations, _clock);
                        _clock.Tick();
                        Dispatcher.Invoke(() => _model.OverlayLayer.InvalidateVisual());
                    }
                    catch (Exception ex)
                    {
                        _log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);

                        Dispatcher.Invoke(() =>
                        {
                            if (IsSimulationRunning())
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
                if (IsSimulationRunning())
                {
                    return;
                }

                IPage temp = _model.ToPage();
                if (temp != null)
                {
                    var context = PageGraph.Create(temp);
                    if (context != null)
                    {
                        var simulations = BoolSimulationFactory.Create(context);
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

                if (IsSimulationRunning())
                {
                    _timer.Dispose();
                    _timer = null;
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

        private void SimulationOptions()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
