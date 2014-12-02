using Logic.Core;
using Logic.Graph;
using Logic.Page;
using Logic.Serialization;
using Logic.Simulation;
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

        public ProjectViewModel Model { get; set; }

        #endregion

        #region Fields

        private IRenderer _renderer = null;
        private Point _dragStartPoint;
        private bool _isContextMenu = false;
        private System.Threading.Timer _timer = null;
        private Clock _clock = null;

        #endregion

        #region Constructor

        public MainView()
        {
            InitializeComponent();

            InitializeModel();
            InitializeView();
            InitializeBlocks();
            InitializeMEF();
            InitializeProject();
        }

        #endregion

        #region Initialize

        private void InitializeModel()
        {
            Model = new ProjectViewModel();

            Model.Blocks = new ObservableCollection<XBlock>();
            Model.Templates = new ObservableCollection<ITemplate>();

            Model.FileName = null;
            Model.FilePath = null;

            Model.Tool = new ToolMenuModel();

            Model.PageAddCommand = new NativeCommand(
                (parameter) => this.PageAdd(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.PageInsertBeforeCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.PageInsertAfterCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.PageCutCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.PageCopyCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.PagePasteCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.PageDeleteCommand = new NativeCommand(
                (parameter) => this.PageDelete(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.DocumentAddCommand = new NativeCommand(
                (parameter) => this.DocumentAdd(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.DocumentInsertBeforeCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.DocumentInsertAfterCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.DocumentCutCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.DocumentCopyCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.DocumentPasteCommand = new NativeCommand(
                (parameter) => { throw new NotImplementedException(); },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.DocumentDeleteCommand = new NativeCommand(
                (parameter) => this.DocumentDelete(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SelectedItemChangedCommand = new NativeCommand(
                (parameter) => this.PageUpdateView(parameter),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileNewCommand = new NativeCommand(
                (parameter) => this.FileNew(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileOpenCommand = new NativeCommand
                ((parameter) => this.FileOpen(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileSaveCommand = new NativeCommand(
                (parameter) => this.FileSave(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileSaveAsCommand = new NativeCommand(
                (parameter) => this.FileSaveAs(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.FileSaveAsPDFCommand = new NativeCommand(
                (parameter) => this.FileSaveAsPDF(), 
                (parameter) => IsSimulationRunning() ? false : true);
 
            Model.FileExitCommand = new NativeCommand(
                (parameter) => 
                {
                    if (IsSimulationRunning())
                    {
                        this.SimulationStop();
                    }
                    this.Close();
                }, 
                (parameter) => true);

            Model.EditUndoCommand = new NativeCommand(
                (parameter) => Model.Undo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.History.CanUndo() ? false : true;
                });

            Model.EditRedoCommand = new NativeCommand
                ((parameter) => Model.Redo(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.History.CanRedo() ? false : true;
                });

            Model.EditCutCommand = new NativeCommand(
                (parameter) => Model.Cut(),
                (parameter) => 
                {
                    return IsSimulationRunning()
                        || !Model.CanCopy() ? false : true;
                });

            Model.EditCopyCommand = new NativeCommand(
                (parameter) => Model.Copy(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.CanCopy() ? false : true;
                });

            Model.EditPasteCommand = new NativeCommand(
                (parameter) =>
                {
                    Model.Paste();
                    if (_isContextMenu && _renderer.Selected != null)
                    {
                        double minX = Model.Page.Template.Width;
                        double minY = Model.Page.Template.Height;
                        Model.EditorLayer.Min(_renderer.Selected, ref minX, ref minY);
                        double x = Model.EditorLayer.RightX - minX;
                        double y = Model.EditorLayer.RightY - minY;
                        Model.EditorLayer.Move(_renderer.Selected, x, y);
                    }
                },
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.CanPaste() ? false : true;
                });

            Model.EditDeleteCommand = new NativeCommand(
                (parameter) => Model.SelectionDelete(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.HaveSelection() ? false : true;
                });

            Model.EditSelectAllCommand = new NativeCommand(
                (parameter) => 
                {
                    Model.SelectAll();
                    Model.Invalidate();
                }, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftBottomCommand = new NativeCommand(
                (parameter) =>
                {
                    Model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left);
                    Model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignBottomCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightBottomCommand = new NativeCommand(
                (parameter) =>
                {
                    Model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right);
                    Model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Bottom);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignCenterCenterCommand = new NativeCommand(
                (parameter) =>
                {
                    Model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Center);
                    Model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Center);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignLeftTopCommand = new NativeCommand(
                (parameter) =>
                {
                    Model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Left);
                    Model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignTopCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditAlignRightTopCommand = new NativeCommand
                ((parameter) =>
                {
                    Model.EditorLayer.ShapeSetTextHAlignment(HAlignment.Right);
                    Model.EditorLayer.ShapeSetTextVAlignment(VAlignment.Top);
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditIncreaseTextSizeCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeSetTextSizeDelta(+1.0), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditDecreaseTextSizeCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeSetTextSizeDelta(-1.0), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleFillCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeToggleFill(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleSnapCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.EnableSnap = !Model.EditorLayer.EnableSnap, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertStartCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeToggleInvertStart(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditToggleInvertEndCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.ShapeToggleInvertEnd(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.EditCancelCommand = new NativeCommand(
                (parameter) => Model.EditorLayer.MouseCancel(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolNoneCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.None, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolSelectionCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Selection, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolWireCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Wire, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolPinCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Pin, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolLineCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Line, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolEllipseCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Ellipse, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolRectangleCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Rectangle, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolTextCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Text, 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ToolImageCommand = new NativeCommand(
                (parameter) => Model.Tool.CurrentTool = ToolMenuModel.Tool.Image,
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockImportCommand = new NativeCommand(
                (parameter) => this.BlockImport(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockImportCodeCommand = new NativeCommand(
                (parameter) => this.BlocksImportFromCode(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.BlockExportCommand = new NativeCommand(
                (parameter) => this.BlockExport(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.HaveSelection() ? false : true;
                });

            Model.BlockCreateProjectCommand = new NativeCommand(
                (parameter) => this.BlockCreateProject(),
                (parameter) =>
                {
                    return IsSimulationRunning()
                        || !Model.HaveSelection() ? false : true;
                });

            Model.InsertBlockCommand = new NativeCommand(
                (parameter) =>
                {
                    XBlock block = parameter as XBlock;
                    if (block != null)
                    {
                        double x = _isContextMenu ? Model.EditorLayer.RightX : 0.0;
                        double y = _isContextMenu ? Model.EditorLayer.RightY : 0.0;
                        BlockInsert(block, x, y);
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateImportCommand = new NativeCommand(
                (parameter) => this.TemplateImport(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateImportCodeCommand = new NativeCommand(
                (parameter) => this.TemplatesImportFromCode(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.TemplateExportCommand = new NativeCommand(
                (parameter) => this.TemplateExport(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.ApplyTemplateCommand = new NativeCommand(
                (parameter) =>
                {
                    ITemplate template = parameter as ITemplate;
                    if (template != null)
                    {
                        Model.Page.Template = template;
                        TemplateApply(template, _renderer);
                        TemplateInvalidate();
                    }
                },
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationStartCommand = new NativeCommand(
                (parameter) => this.SimulationStart(),
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationStopCommand = new NativeCommand(
                (parameter) => this.SimulationStop(), 
                (parameter) => IsSimulationRunning() ? true : false);

            Model.SimulationRestartCommand = new NativeCommand(
                (parameter) => this.SimulationRestart(), 
                (parameter) => IsSimulationRunning() ? true : false);

            Model.SimulationCreateGraphCommand = new NativeCommand(
                (parameter) => this.Graph(), 
                (parameter) => IsSimulationRunning() ? false : true);

            Model.SimulationOptionsCommand = new NativeCommand(
                (parameter) => this.SimulationOptions(), 
                (parameter) => IsSimulationRunning() ? false : true);
        }

        private void InitializeView()
        {
            // layers
            Model.ShapeLayer = pageView.shapeLayer.Model;
            Model.BlockLayer = pageView.blockLayer.Model;
            Model.WireLayer = pageView.wireLayer.Model;
            Model.PinLayer = pageView.pinLayer.Model;
            Model.EditorLayer = pageView.editorLayer.Model;
            Model.OverlayLayer = pageView.overlayLayer.Model;

            // editor
            Model.EditorLayer.Layers = Model;
            Model.EditorLayer.GetFilePath = this.GetFilePath;

            // overlay
            Model.OverlayLayer.IsOverlay = true;

            // serializer
            Model.Serializer = new Json();

            // renderer
            _renderer = new NativeRenderer()
            {
                InvertSize = 6.0,
                PinRadius = 4.0,
                HitTreshold = 6.0
            };

            Model.Renderer = _renderer;

            Model.ShapeLayer.Renderer = _renderer;
            Model.BlockLayer.Renderer = _renderer;
            Model.WireLayer.Renderer = _renderer;
            Model.PinLayer.Renderer = _renderer;
            Model.EditorLayer.Renderer = _renderer;
            Model.OverlayLayer.Renderer = _renderer;

            // clipboard
            Model.Clipboard = new NativeTextClipboard();

            // history
            Model.History = new History<IPage>(new Bson());

            // tool
            Model.Tool = Model.Tool;
            Model.Tool.CurrentTool = ToolMenuModel.Tool.Selection;

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
                            BlockInsert(block, point.X, point.Y);
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
                                FileOpen(path);
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
                if (Model.EditorLayer.CurrentMode != LayerViewModel.Mode.None)
                {
                    e.Handled = true;
                }
                else if (Model.EditorLayer.SkipContextMenu == true)
                {
                    Model.EditorLayer.SkipContextMenu = false;
                    e.Handled = true;
                }
                else
                {
                    if (_renderer.Selected == null 
                        && !IsSimulationRunning())
                    {
                        Point2 point = new Point2(
                            Model.EditorLayer.RightX,
                            Model.EditorLayer.RightY);
                        IShape shape = Model.HitTest(point);
                        if (shape != null && shape is XBlock)
                        {
                            Model.Selected = shape;
                            Model.HaveSelected = true;
                        }
                        else
                        {
                            Model.Selected = null;
                            Model.HaveSelected = false;
                        }
                    }
                    else
                    {
                        Model.Selected = null;
                        Model.HaveSelected = false;
                    }

                    _isContextMenu = true;
                }
            };

            pageView.ContextMenuClosing += (s, e) =>
            {
                if (Model.Selected != null)
                {
                    Model.Invalidate();
                }

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

        private void InitializeProject()
        {
            Model.Project = NewProject();

            Model.Project.Documents.Add(Defaults.EmptyDocument());
            Model.Project.Documents[0].Pages.Add(Defaults.EmptyPage());

            UpdateStyles(Model.Project);
            SetDefaultTemplate(Model.Project);
            LoadFirstPage(Model.Project);
        }

        #endregion

        #region File

        private void FileNew()
        {
            Model.Renderer.Dispose();

            InitializeProject();

            Model.FileName = null;
            Model.FilePath = null;
        }

        private void FileOpen()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject"
            };

            if (dlg.ShowDialog(this) == true)
            {
                FileOpen(dlg.FileName);
            }
        }

        private void FileOpen(string path)
        {
            var project = Model.Load(path);
            if (project != null)
            {
                Model.Renderer.Dispose();
                Model.Project = project;
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                Model.FilePath = path;
                UpdateStyles(project);
                LoadFirstPage(project);
            }
        }

        private void FileSave()
        {
            if (!string.IsNullOrEmpty(Model.FilePath))
            {
                Model.Save(Model.FilePath, Model.Project);
            }
            else
            {
                FileSaveAs();
            }
        }

        private void FileSaveAs()
        {
            string fileName = string.IsNullOrEmpty(Model.FilePath) ?
                "logic" : System.IO.Path.GetFileName(Model.FilePath);

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Logic Project (*.lproject)|*.lproject",
                FileName = fileName
            };

            if (dlg.ShowDialog(this) == true)
            {
                Model.Save(dlg.FileName, Model.Project);
                Model.FileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                Model.FilePath = dlg.FileName;
            }
        }

        private void FileSaveAsPDF()
        {
            string fileName = string.IsNullOrEmpty(Model.FilePath) ?
                "logic" : System.IO.Path.GetFileNameWithoutExtension(Model.FilePath);

            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = fileName
            };

            if (dlg.ShowDialog(this) == true)
            {
                try
                {
                    FileSaveAsPDF(path: dlg.FileName, ignoreStyles: true);
                }
                catch (Exception ex)
                {
                    Log.LogError("{0}{1}{2}",
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
                InvertSize = _renderer.InvertSize,
                PinRadius = _renderer.PinRadius,
                HitTreshold = _renderer.HitTreshold,
                EnablePinRendering = false,
                EnableGridRendering = false
            };

            if (ignoreStyles)
            {
                writer.TemplateStyleOverride = Model.Project.Styles
                    .Where(s => s.Name == "TemplateOverride")
                    .FirstOrDefault();

                writer.LayerStyleOverride = Model.Project.Styles
                    .Where(s => s.Name == "LayerOverride")
                    .FirstOrDefault();
            }

            writer.Create(
                path,
                Model.Project.Documents.SelectMany(d => d.Pages));

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
            foreach (var template in Model.Templates)
            {
                project.Templates.Add(Model.Clone(template));
            }

            return project;
        }

        private void UpdateStyles(IProject project)
        {
            var layers = new List<LayerViewModel>();
            layers.Add(Model.ShapeLayer);
            layers.Add(Model.BlockLayer);
            layers.Add(Model.WireLayer);
            layers.Add(Model.PinLayer);
            layers.Add(Model.EditorLayer);
            layers.Add(Model.OverlayLayer);

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
            if (parameter is ProjectViewModel)
            {
                IDocument document = Defaults.EmptyDocument();
                document.IsActive = true;
                Model.Project.Documents.Add(document);
            }
        }

        private void DocumentDelete(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument document = parameter as IDocument;
                Model.Project.Documents.Remove(document);

                Model.Page = null;
                Model.Clear();
                Model.Reset();
                Model.Invalidate();
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
            Model.Reset();
            Model.SelectionReset();
            Model.Page = page;
            Model.Load(page);
            Model.Invalidate();
            TemplateApply(page.Template, _renderer);
            TemplateInvalidate();
        }

        private void PageAdd(object parameter)
        {
            if (parameter is IDocument)
            {
                IDocument document = parameter as IDocument;
                IPage page = Defaults.EmptyPage();
                page.Template = Model.Project.Templates.Where(t => t.Name == "Logic Page").First();
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
                IDocument document = Model
                    .Project
                    .Documents
                    .Where(d => d.Pages.Contains(page)).FirstOrDefault();
                if (document != null && document.Pages != null)
                {
                    document.Pages.Remove(page);

                    Model.Page = null;
                    Model.Clear();
                    Model.Reset();
                    Model.Invalidate();
                    TemplateReset();
                    TemplateInvalidate();
                }
            }
        }

        #endregion

        #region Block

        private void BlockInsert(XBlock block, double x, double y)
        {
            Model.Snapshot();
            XBlock copy = Model.EditorLayer.Insert(block, x, y);
            if (copy != null)
            {
                Model.EditorLayer.Connect(copy);
            }
        }

        private void BlockImport()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Logic Block (*.lblock)|*.lblock"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var block = Model.Open<XBlock>(dlg.FileName);
                if (block != null)
                {
                    Model.Blocks.Add(block);
                }
            }
        }

        private void BlockCreateProject()
        {
            var block = Model.EditorLayer.BlockCreateFromSelected("Block");
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

            vm.BrowseCommand = new NativeCommand((parameter) => 
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

            vm.CreateCommand = new NativeCommand((parameter) => 
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

            vm.CancelCommand = new NativeCommand((parameter) =>
            {
                view.Close();
            },
            (parameter) => true);

            view.DataContext = vm;
            view.ShowDialog();
        }

        private void BlocksImportFromCode()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "CSharp (*.cs)|*.cs",
                Multiselect = true
            };

            if (dlg.ShowDialog(this) == true)
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
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void BlocksImport(string csharp)
        {
            var part = new BlockPart() { Blocks = new ObservableCollection<XBlock>() };
            bool result = CSharpCodeImporter.Import<XBlock>(csharp, part);
            if (result == true)
            {
                foreach (var block in part.Blocks)
                {
                    Model.Blocks.Add(Model.Clone(block));
                }
            }
        }

        private void BlockExport()
        {
            var block = Model.EditorLayer.BlockCreateFromSelected("Block");
            if (block != null)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "Logic Block (*.lblock)|*.lblock",
                    FileName = "block"
                };

                if (dlg.ShowDialog(this) == true)
                {
                    var path = dlg.FileName;
                    Model.Save<XBlock>(path, block);
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

            if (dlg.ShowDialog(this) == true)
            {
                return dlg.FileName;
            }
            return null;
        }

        #endregion

        #region Template

        private void TemplateApply(ITemplate template, IRenderer renderer)
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

        private void TemplateReset()
        {
            pageView.gridView.Container = null;
            pageView.tableView.Container = null;
            pageView.frameView.Container = null;
        }

        private void TemplateInvalidate()
        {
            pageView.gridView.InvalidateVisual();
            pageView.tableView.InvalidateVisual();
            pageView.frameView.InvalidateVisual();
        }

        private void TemplateImport()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = Model.Open<XTemplate>(dlg.FileName);
                if (template != null)
                {
                    Model.Project.Templates.Add(template);
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

            if (dlg.ShowDialog(this) == true)
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
                Log.LogError("{0}{1}{2}",
                    ex.Message,
                    Environment.NewLine,
                    ex.StackTrace);
            }
        }

        private void TemplatesImport(string csharp)
        {
            var part = new TemplatePart() { Templates = new ObservableCollection<ITemplate>() };
            bool result = CSharpCodeImporter.Import<ITemplate>(csharp, part);
            if (result == true)
            {
                foreach (var template in part.Templates)
                {
                    Model.Project.Templates.Add(Model.Clone(template));
                }
            }
        }

        private void TemplateExport()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "Logic Template (*.ltemplate)|*.ltemplate",
                FileName = Model.Page.Template.Name
            };

            if (dlg.ShowDialog(this) == true)
            {
                var template = Model.Clone(Model.Page.Template);
                var path = dlg.FileName;
                Model.Save<XTemplate>(path, template);
                System.Diagnostics.Process.Start("notepad", path);
            }
        }

        #endregion

        #region Overlay

        private void OverlayInit(IDictionary<XBlock, BoolSimulation> simulations)
        {
            Model.SelectionReset();

            Model.OverlayLayer.EnableSimulationCache = true;
            Model.OverlayLayer.CacheRenderer = null;

            foreach (var simulation in simulations)
            {
                Model.BlockLayer.Hidden.Add(simulation.Key);
                Model.OverlayLayer.Shapes.Add(simulation.Key);
            }

            Model.EditorLayer.Simulations = simulations;
            Model.OverlayLayer.Simulations = simulations;

            Model.BlockLayer.InvalidateVisual();
            Model.OverlayLayer.InvalidateVisual();
        }

        private void OverlayReset()
        {
            Model.EditorLayer.Simulations = null;
            Model.OverlayLayer.Simulations = null;
            Model.OverlayLayer.CacheRenderer = null;

            Model.BlockLayer.Hidden.Clear();
            Model.OverlayLayer.Shapes.Clear();
            Model.BlockLayer.InvalidateVisual();
            Model.OverlayLayer.InvalidateVisual();
        }

        #endregion

        #region Graph

        private void Graph()
        {
            try
            {
                IPage temp = Model.ToPage();
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
                Log.LogError("{0}{1}{2}",
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

            if (dlg.ShowDialog(this) == true)
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
                        Dispatcher.Invoke(() => Model.OverlayLayer.InvalidateVisual());
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

                IPage temp = Model.ToPage();
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
                Log.LogError("{0}{1}{2}",
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
                Log.LogError("{0}{1}{2}",
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
