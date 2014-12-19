using Logic.Util;
using Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Logic.Native
{
    public class NativeCanvas : Canvas, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Properties

        private LayerViewModel _model;
        public LayerViewModel Model
        {
            get { return _model; }
            set
            {
                if (value != _model)
                {
                    _model = value;
                    InitializeModel(_model);
                    Notify("Model");
                }
            }
        } 

        #endregion

        #region Constructor

        public NativeCanvas()
            : base()
        {
            InitializeEvents();
            RenderOptions.SetBitmapScalingMode(
                this, 
                BitmapScalingMode.HighQuality);
        }

        #endregion

        #region Initialize

        private void InitializeEvents()
        {
            base.DataContextChanged += (s, e) =>
            {
                if (base.DataContext != null
                    && base.DataContext is LayerViewModel)
                {
                    Model = base.DataContext as LayerViewModel;
                }
            };

            base.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (_model != null)
                {
                    Focus();
                    _model.MouseLeftButtonDown(e.GetPosition(this).ToPoint1());
                }
            };

            base.PreviewMouseLeftButtonUp += (s, e) =>
            {
                if (_model != null)
                {
                    _model.MouseLeftButtonUp(e.GetPosition(this).ToPoint1());
                }
            };

            base.PreviewMouseMove += (s, e) =>
            {
                if (_model != null)
                {
                    _model.MouseMove(e.GetPosition(this).ToPoint1());
                }
            };

            base.PreviewMouseRightButtonDown += (s, e) =>
            {
                if (_model != null)
                {
                    Focus();
                    _model.MouseRightButtonDown(e.GetPosition(this).ToPoint1());
                }
            };
        }

        public void InitializeModel(LayerViewModel model)
        {
            model.IsMouseCaptured = () =>
            {
                return this.IsMouseCaptured;
            };

            model.CaptureMouse = () =>
            {
                this.CaptureMouse();
            };

            model.ReleaseMouseCapture = () =>
            {
                this.ReleaseMouseCapture();
            };

            model.InvalidateVisual = () =>
            {
                this.InvalidateVisual();
            };
        } 

        #endregion

        #region OnRender

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_model != null)
            {
                _model.OnRender(dc);
            }
        } 

        #endregion
    }
}
