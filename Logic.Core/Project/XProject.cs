// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XProject : NotifyObject, IProject
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

        private string _defaultTemplate;
        public string DefaultTemplate
        {
            get { return _defaultTemplate; }
            set
            {
                if (value != _defaultTemplate)
                {
                    _defaultTemplate = value;
                    Notify("DefaultTemplate");
                }
            }
        }

        private IList<IStyle> _styles;
        public IList<IStyle> Styles
        {
            get { return _styles; }
            set
            {
                if (value != _styles)
                {
                    _styles = value;
                    Notify("Styles");
                }
            }
        }

        private IList<ITemplate> _templates;
        public IList<ITemplate> Templates
        {
            get { return _templates; }
            set
            {
                if (value != _templates)
                {
                    _templates = value;
                    Notify("Templates");
                }
            }
        }

        private IList<IDocument> _documents;
        public IList<IDocument> Documents
        {
            get { return _documents; }
            set
            {
                if (value != _documents)
                {
                    _documents = value;
                    Notify("Documents");
                }
            }
        }
    }
}
