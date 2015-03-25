// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Logic.Native
{
    public class NativeTextClipboard : ITextClipboard
    {
        public void SetText(string text)
        {
            Clipboard.SetText(text, TextDataFormat.UnicodeText);
        }

        public string GetText()
        {
            return Clipboard.GetText(TextDataFormat.UnicodeText);
        }

        public bool ContainsText()
        {
            return Clipboard.ContainsText(TextDataFormat.UnicodeText);
        }
    }
}
