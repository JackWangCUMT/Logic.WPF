// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using Logic.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public class CSharpCodeCreator
    {
        public string Generate(XBlock block, string namespaceName, string className, string blockName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("using Logic.Core;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("");

            sb.AppendLine("namespace " + namespaceName);
            sb.AppendLine("{");

            sb.AppendLine("    public class " + className + " : XBlock");
            sb.AppendLine("    {");
            sb.AppendLine("        public " + className + "()");
            sb.AppendLine("        {");

            sb.AppendLine("            base.Database = new List<KeyValuePair<string, IProperty>>();");
            sb.AppendLine("            base.Shapes = new List<IShape>();");
            sb.AppendLine("            base.Pins = new List<XPin>();");
            sb.AppendLine("");

            sb.AppendLine("            base.Name = \"" + blockName + "\";");
            sb.AppendLine("");

            string indent = "            ";
            foreach (var shape in block.Shapes)
            {
                if (shape is XLine)
                {
                    var line = shape as XLine;
                    var value = string.Format(
                        "{0}base.Shapes.Add(new XLine() {{ X1 = {1}, Y1 = {2}, X2 = {3}, Y2 = {4} }});",
                        indent,
                        line.X1,
                        line.Y1,
                        line.X2,
                        line.Y2);
                    sb.AppendLine(value);
                }
                else if (shape is XEllipse)
                {
                    var ellipse = shape as XEllipse;
                    var value = string.Format(
                        "{0}base.Shapes.Add(new XEllipse() {{ X = {1}, Y = {2}, RadiusX = {3}, RadiusY = {4}, IsFilled = {5} }});",
                        indent,
                        ellipse.X,
                        ellipse.Y,
                        ellipse.RadiusX,
                        ellipse.RadiusY,
                        ellipse.IsFilled.ToString().ToLower());
                    sb.AppendLine(value);
                }
                else if (shape is XRectangle)
                {
                    var rectangle = shape as XRectangle;
                    var value = string.Format(
                        "{0}base.Shapes.Add(new XRectangle() {{ X = {1}, Y = {2}, Width = {3}, Height = {4}, IsFilled = {5} }});",
                        indent,
                        rectangle.X,
                        rectangle.Y,
                        rectangle.Width,
                        rectangle.Height,
                        rectangle.IsFilled.ToString().ToLower());
                    sb.AppendLine(value);
                }
                else if (shape is XText)
                {
                    var text = shape as XText;
                    sb.AppendLine(indent + "base.Shapes.Add(");
                    sb.AppendLine(indent + "    new XText()");
                    sb.AppendLine(indent + "    {");
                    sb.AppendLine(indent + "        X = " + text.X + ",");
                    sb.AppendLine(indent + "        Y = " + text.Y + ",");
                    sb.AppendLine(indent + "        Width = " + text.Width + ",");
                    sb.AppendLine(indent + "        Height = " + text.Height + ",");
                    sb.AppendLine(indent + "        HAlignment = HAlignment." + text.HAlignment + ",");
                    sb.AppendLine(indent + "        VAlignment = VAlignment." + text.VAlignment + ",");
                    sb.AppendLine(indent + "        FontName = \"" + text.FontName + "\",");
                    sb.AppendLine(indent + "        FontSize = " + text.FontSize + ",");
                    sb.AppendLine(indent + "        Text = \"" + text.Text + "\"");
                    sb.AppendLine(indent + "    });");
                }
                else if (shape is XImage)
                {
                    var image = shape as XImage;
                    var value = string.Format(
                        "{0}base.Shapes.Add(new XImage() {{ X = {1}, Y = {2}, Width = {3}, Height = {4}, Path = {5} }});",
                        indent,
                        image.X,
                        image.Y,
                        image.Width,
                        image.Height,
                        image.Path);
                    sb.AppendLine(value);
                }
                else if (shape is XWire)
                {
                    // Not supported.
                }
                else if (shape is XPin)
                {
                    // Not supported.
                }
                else if (shape is XBlock)
                {
                    // Not supported.
                }
            }

            foreach (var pin in block.Pins)
            {
                var value = string.Format(
                    "{0}base.Pins.Add(new XPin() {{ Name = \"{1}\", X = {2}, Y = {3}, PinType = PinType.{4}, Owner = null }});",
                    indent,
                    pin.Name,
                    pin.X,
                    pin.Y,
                    pin.PinType);
                sb.AppendLine(value);
            }

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
