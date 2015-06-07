// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.ViewModels
{
    public class ToolMenuModel : NotifyObject
    {
        public enum Tool
        {
            None,
            Selection,
            Line,
            Ellipse,
            Rectangle,
            Text,
            Image,
            Wire,
            Pin
        }

        private Tool _currentTool;
        public Tool CurrentTool
        {
            get { return _currentTool; }
            set
            {
                if (value != _currentTool)
                {
                    _currentTool = value;
                    Notify("CurrentTool");
                    Notify("IsNoneChecked");
                    Notify("IsSelectionChecked");
                    Notify("IsLineChecked");
                    Notify("IsEllipseChecked");
                    Notify("IsRectangleChecked");
                    Notify("IsTextChecked");
                    Notify("IsImageChecked");
                    Notify("IsWireChecked");
                    Notify("IsPinChecked");
                }
            }
        }

        public bool IsNoneChecked
        {
            get { return _currentTool == Tool.None; }
        }

        public bool IsSelectionChecked
        {
            get { return _currentTool == Tool.Selection; }
        }

        public bool IsLineChecked
        {
            get { return _currentTool == Tool.Line; }
        }

        public bool IsEllipseChecked
        {
            get { return _currentTool == Tool.Ellipse; }
        }

        public bool IsRectangleChecked
        {
            get { return _currentTool == Tool.Rectangle; }
        }

        public bool IsTextChecked
        {
            get { return _currentTool == Tool.Text; }
        }

        public bool IsImageChecked
        {
            get { return _currentTool == Tool.Image; }
        }

        public bool IsWireChecked
        {
            get { return _currentTool == Tool.Wire; }
        }

        public bool IsPinChecked
        {
            get { return _currentTool == Tool.Pin; }
        }
    }
}
