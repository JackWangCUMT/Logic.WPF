// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
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
