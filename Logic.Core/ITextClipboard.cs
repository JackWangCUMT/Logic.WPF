using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface ITextClipboard
    {
        bool ContainsText();
        string GetText();
        void SetText(string text);
    }
}
