using System;
using System.Collections.Generic;

namespace XSkillSystem
{
    // 具备过滤与录制能力的实现
    public sealed class EventBus : IEventBus
    {
        // 每种事件类型一张订阅表
        readonly Dictionary<Type, Dictionary<int, HandlerBox>> _handlers = new();
        int _nextToken = 1;

        sealed class HandlerBox
        {
            // Action<T>
            public Delegate Handler;
            // Predicate<T> 或 null
            public Delegate FilterNullable;
        }

        public void Publish<TEvent>(TEvent e)
        {
            var t = typeof(TEvent);
            if (_handlers.TryGetValue(t, out var dic))
            {
                // 拷贝避免遍历中变更
                var arr = new List<HandlerBox>(dic.Values);
                for (int i = 0; i < arr.Count; i++)
                {
                    var hb = arr[i];
                    if (hb.FilterNullable is Predicate<TEvent> f && !f(e)) continue;
                    (hb.Handler as Action<TEvent>)?.Invoke(e);
                }
            }

            // 录制器：如果挂了 RecordingSink，也同步收集
            _recorder?.OnPublish(e);
        }

        public int Subscribe<TEvent>(Action<TEvent> handler)
        {
            var t = typeof(TEvent);
            if (!_handlers.TryGetValue(t, out var dic))
            {
                dic = new();
                _handlers[t] = dic;
            }

            int token = _nextToken++;
            dic[token] = new HandlerBox { Handler = handler, FilterNullable = null };
            return token;
        }

        // 扩展：带过滤条件的订阅（仅 EventBus 实现提供）
        public int Subscribe<TEvent>(Action<TEvent> handler, Predicate<TEvent> filter)
        {
            var t = typeof(TEvent);
            if (!_handlers.TryGetValue(t, out var dic))
            {
                dic = new();
                _handlers[t] = dic;
            }

            int token = _nextToken++;
            dic[token] = new HandlerBox { Handler = handler, FilterNullable = filter };
            return token;
        }

        public void Unsubscribe<TEvent>(int token)
        {
            foreach (var kv in _handlers)
            {
                if (kv.Value.Remove(token)) break;
            }
        }

        // ------------------ 录制器 ------------------
        IBusRecorder _recorder;
        public void AttachRecorder(IBusRecorder recorder) => _recorder = recorder;
        public void DetachRecorder() => _recorder = null;
    }
}