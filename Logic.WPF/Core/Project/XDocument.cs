// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XDocument : NotifyObject, IDocument
    {
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

        private IList<IPage> _pages;
        public IList<IPage> Pages
        {
            get { return _pages; }
            set
            {
                if (value != _pages)
                {
                    _pages = value;
                    Notify("Pages");
                }
            }
        }
    }
}
