using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderSettingsChangerTest
{
    public class LimitedBuffer<T>
    {
        private readonly int _maxSize;
        private readonly Queue<T> _queue;

        public LimitedBuffer(int maxSize)
        {
            _maxSize = maxSize;
            _queue = new Queue<T>();
        }

        public void Add(T item)
        {
            if (_queue.Count == _maxSize) _queue.Dequeue();
            _queue.Enqueue(item);
        }

        public IEnumerable<T> Items => _queue;
    }
}
