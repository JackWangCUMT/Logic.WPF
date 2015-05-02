// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public class XText : IShape
    {
        public IProperty[] Properties { get; set; }
        public IStyle Style { get; set; }
        public string Text { get; set; }
        public string TextBinding { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }
        public HAlignment HAlignment { get; set; }
        public VAlignment VAlignment { get; set; }
        public double FontSize { get; set; }
        public string FontName { get; set; }

        public string Bind(IList<KeyValuePair<string, IProperty>> db)
        {
            if (db != null && !string.IsNullOrEmpty(this.TextBinding))
            {
                // try to bind to database using TextBinding key
                var result = db.Where(kvp => kvp.Key == this.TextBinding).FirstOrDefault();
                if (result.Value != null)
                {
                    return result.Value.Data.ToString();
                }
            }

            // try to bind to Properties using Text as formatting
            return (this.Properties != null) ?
                string.Format(this.Text, this.Properties) : this.Text;
        }

        public void Render(object dc, IRenderer renderer, IStyle style)
        {
            renderer.DrawText(dc, style, this);
        }
    }
}
