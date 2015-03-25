// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XPage : NotifyObject, IPage
    {
        private IList<KeyValuePair<string, IProperty>> _database;
        public IList<KeyValuePair<string, IProperty>> Database
        {
            get { return _database; }
            set
            {
                if (value != _database)
                {
                    _database = value;
                    Notify("Database");
                }
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    Notify("Name");
                }
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (value != _isActive)
                {
                    _isActive = value;
                    Notify("IsActive");
                }
            }
        }

        private ITemplate _template;
        public ITemplate Template
        {
            get { return _template; }
            set
            {
                if (value != _template)
                {
                    _template = value;
                    Notify("Template");
                }
            }
        }

        private IList<IShape> _shapes;
        public IList<IShape> Shapes
        {
            get { return _shapes; }
            set
            {
                if (value != _shapes)
                {
                    _shapes = value;
                    Notify("Shapes");
                }
            }
        }

        private IList<IShape> _blocks;
        public IList<IShape> Blocks
        {
            get { return _blocks; }
            set
            {
                if (value != _blocks)
                {
                    _blocks = value;
                    Notify("Blocks");
                }
            }
        }

        private IList<IShape> _pins;
        public IList<IShape> Pins
        {
            get { return _pins; }
            set
            {
                if (value != _pins)
                {
                    _pins = value;
                    Notify("Pins");
                }
            }
        }

        private IList<IShape> _wires;
        public IList<IShape> Wires
        {
            get { return _wires; }
            set
            {
                if (value != _wires)
                {
                    _wires = value;
                    Notify("Wires");
                }
            }
        }
    }
}
