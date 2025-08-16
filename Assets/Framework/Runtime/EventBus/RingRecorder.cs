using System;
using System.Collections.Generic;

namespace XSkillSystem
{
    // 环形缓冲录制（最近 N 条，调试面板）
    public sealed class RingRecorder : IBusRecorder
    {
        struct Item
        {
            public object Payload;
            public Type Type;
        }

        readonly Item[] _buf;
        int _head;
        public int Count { get; private set; }

        public RingRecorder(int capacity = 256)
        {
            _buf = new Item[Math.Max(16, capacity)];
            _head = 0;
            Count = 0;
        }

        public void OnPublish<TEvent>(TEvent e)
        {
            _buf[_head] = new Item { Payload = e, Type = typeof(TEvent) };
            _head = (_head + 1) % _buf.Length;
            Count = Math.Min(Count + 1, _buf.Length);
        }

        public void Clear()
        {
            _head = 0;
            Count = 0;
        }

        public IEnumerable<(object payload, Type type)> Enumerate()
        {
            int n = Count, cap = _buf.Length;
            for (int i = cap - n; i < cap; i++)
            {
                int idx = (i + _head) % cap;
                var it = _buf[idx];
                if (it.Payload != null) yield return (it.Payload, it.Type);
            }
        }
    }
}