using System.Collections.Generic;

namespace XSkillSystem
{
    public sealed class Pool<T> where T : class, new()
    {
        readonly Stack<T> _stack = new Stack<T>(32);
        public T Get() => _stack.Count > 0 ? _stack.Pop() : new T();

        public void Return(T obj)
        {
            if (obj != null) _stack.Push(obj);
        }

        public void Clear() => _stack.Clear();
    }
}