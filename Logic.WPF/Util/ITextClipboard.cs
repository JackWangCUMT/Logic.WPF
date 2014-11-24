using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public interface ITextClipboard
    {
        bool ContainsText();
        string GetText();
        void SetText(string text);
    }
}
