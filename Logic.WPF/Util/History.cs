using Logic.Core;
using Logic.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Util
{
    public class History<T> where T : class
    {
        private IBinarySerializer _bson = new Bson();
        private Stack<byte[]> _undos = new Stack<byte[]>();
        private Stack<byte[]> _redos = new Stack<byte[]>();

        private byte[] _hold = null;

        public void Reset()
        {
            if (_undos.Count > 0)
            {
                _undos.Clear();
            }

            if (_redos.Count > 0)
            {
                _redos.Clear();
            }
        }

        public void Hold(T obj)
        {
            _hold = _bson.Serialize(obj);
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
            Snapshot(_bson.Serialize(obj));
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
                var bson = _bson.Serialize(current);
                if (bson != null)
                {
                    _redos.Push(bson);
                    return _bson.Deserialize<T>(_undos.Pop());
                }
            }
            return null;
        }

        public T Redo(T current)
        {
            if (CanRedo())
            {
                var bson = _bson.Serialize(current);
                if (bson != null)
                {
                    _undos.Push(bson);
                    return _bson.Deserialize<T>(_redos.Pop()); 
                }
            }
            return null;
        }

        public bool CanUndo()
        {
            return _undos.Count > 0;
        }

        public bool CanRedo()
        {
            return _redos.Count > 0;
        }
    }
}
