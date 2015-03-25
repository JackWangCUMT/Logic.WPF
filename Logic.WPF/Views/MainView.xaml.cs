// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.Portable;
using Logic.Util;
using Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class MainView : Window, IMainView
    {
        private Point _dragStartPoint;

        public MainView()
        {
            InitializeComponent();
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                item.BringIntoView();
                e.Handled = true;
            }
        }

        public void Initialize(MainViewModel model, Main main)
        {
            // status
            this.status.DataContext = model.Log;

            // zoom
            this.page.EnableAutoFit = main.Options.EnableAutoFit;

            this.zoom.InvalidateChild = (zoom) =>
            {
                model.Renderer.Zoom = zoom;
                model.InvalidateTemplate();
                model.InvalidateLayers();
            };

            this.zoom.AutoFitChild = (width, height) =>
            {
                if (this.zoom != null
                    && model != null 
                    && model.Page != null 
                    && model.Page.Template != null)
                {
                    this.zoom.AutoFit(
                        width, 
                        height, 
                        model.Page.Template.Width, 
                        model.Page.Template.Height);
                }
            };

            // drag & drop
            this.pageView.AllowDrop = true;

            this.pageView.DragEnter += (s, e) =>
            {
                if (main.IsSimulationMode())
                {
                    return;
                }

                if (!e.Data.GetDataPresent("Block") || s == e.Source)
                {
                    e.Effects = DragDropEffects.None;
                }
            };

            this.pageView.Drop += (s, e) =>
            {
                if (main.IsSimulationMode())
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
                            Point point = e.GetPosition(this.pageView);
                            main.BlockInsert(block, point.X, point.Y);
                            e.Handled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (model.Log != null)
                        {
                            model.Log.LogError("{0}{1}{2}",
                                ex.Message,
                                Environment.NewLine,
                                ex.StackTrace);
                        }
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
                                main.FileOpen(path);
                                e.Handled = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (model.Log != null)
                        {
                            model.Log.LogError("{0}{1}{2}",
                                ex.Message,
                                Environment.NewLine,
                                ex.StackTrace);
                        }
                    }
                }
            };

            // context menu
            this.pageView.ContextMenuOpening += (s, e) =>
            {
                if (model.EditorLayer.CurrentMode != LayerViewModel.Mode.None)
                {
                    e.Handled = true;
                }
                else if (model.EditorLayer.SkipContextMenu == true)
                {
                    model.EditorLayer.SkipContextMenu = false;
                    e.Handled = true;
                }
                else
                {
                    if (model.Renderer.Selected == null
                        && !main.IsSimulationMode())
                    {
                        Point2 point = new Point2(
                            model.EditorLayer.RightX,
                            model.EditorLayer.RightY);
                        IShape shape = model.HitTest(point);
                        if (shape != null && shape is XBlock)
                        {
                            model.Selected = shape;
                            model.HaveSelected = true;
                        }
                        else
                        {
                            model.Selected = null;
                            model.HaveSelected = false;
                        }
                    }
                    else
                    {
                        model.Selected = null;
                        model.HaveSelected = false;
                    }

                    main.IsContextMenuOpen = true;
                }
            };

            this.pageView.ContextMenuClosing += (s, e) =>
            {
                if (model.Selected != null)
                {
                    model.InvalidateLayers();
                }

                main.IsContextMenuOpen = false;
            };

            // blocks
            this.blocks.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (main.IsSimulationMode())
                {
                    return;
                }

                _dragStartPoint = e.GetPosition(null);
            };

            this.blocks.PreviewMouseMove += (s, e) =>
            {
                if (main.IsSimulationMode())
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

            // show
            this.DataContext = model;
        }
    }
}
