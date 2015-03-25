// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Logic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Util
{
    public class History<T> : IHistory<T> where T : class
    {
        private IBinarySerializer _serializer;
        private Stack<byte[]> _undos = new Stack<byte[]>();
        private Stack<byte[]> _redos = new Stack<byte[]>();
        private byte[] _hold = null;

        public History(IBinarySerializer serializer)
        {
            this._serializer = serializer;
        }

        public void Reset()
        {
            if (_undos != null && _undos.Count > 0)
            {
                _undos.Clear();
            }

            if (_redos != null && _redos.Count > 0)
            {
                _redos.Clear();
            }
        }

        public void Hold(T obj)
        {
            _hold = _serializer.Serialize(obj);
        }

        public void Commit()
        {
            Snapshot(_hold);
        }

        public void Release()
        {
            _hold = null;
        }

        public void Snapshot(T obj)
        {
            Snapshot(_serializer.Serialize(obj));
        }

        private void Snapshot(byte[] bson)
        {
            if (bson != null)
            {
                if (_redos.Count > 0)
                {
                    _redos.Clear();
                }
                _undos.Push(bson);
            }
        }

        public T Undo(T current)
        {
            if (CanUndo())
            {
                var bson = _serializer.Serialize(current);
                if (bson != null)
                {
                    _redos.Push(bson);
                    return _serializer.Deserialize<T>(_undos.Pop());
                }
            }
            return null;
        }

        public T Redo(T current)
        {
            if (CanRedo())
            {
                var bson = _serializer.Serialize(current);
                if (bson != null)
                {
                    _undos.Push(bson);
                    return _serializer.Deserialize<T>(_redos.Pop()); 
                }
            }
            return null;
        }

        public bool CanUndo()
        {
            return _undos != null && _undos.Count > 0;
        }

        public bool CanRedo()
        {
            return _redos != null && _redos.Count > 0;
        }
    }
}
