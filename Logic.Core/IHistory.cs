using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Core
{
    public interface IHistory<T> where T : class
    {
        void Reset();
        void Hold(T obj);
        void Commit();
        void Release();
        void Snapshot(T obj);
        T Undo(T current);
        T Redo(T current);
        bool CanUndo();
        bool CanRedo();
    }
}
