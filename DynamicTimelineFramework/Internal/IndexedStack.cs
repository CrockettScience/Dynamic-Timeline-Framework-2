using System.Collections.Generic;

namespace DynamicTimelineFramework.Internal
{
    
    //TODO - Quick implementation using standard utils, optimize
    internal class IndexedStack<T>
    {
        private readonly Stack<T> _stack;
        private readonly HashSet<T> _index;

        public IndexedStack()
        {
            _stack = new Stack<T>();
            _index = new HashSet<T>();
        }

        public void Push(T item)
        {
            _stack.Push(item);
            _index.Add(item);
        }

        public T Pop()
        {
            var item = _stack.Pop();
            _index.Remove(item);

            return item;
        }

        public T Top()
        {
            return _stack.Peek();
        }

        public bool Contains(T item)
        {
            return _index.Contains(item);
        }
            
    }
}