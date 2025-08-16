using System.Collections.Generic;
using System.Linq;

namespace XSkillSystem
{
    /// <summary>
    /// 运行时黑板：支持父级（资产层）覆盖。
    /// </summary>
    public sealed class Blackboard
    {
        readonly Dictionary<int, object> _map = new(64);
        readonly BlackboardAsset _defaults; // 可空

        public Blackboard(BlackboardAsset defaults = null)
        {
            _defaults = defaults;
        }

        public void Clear() => _map.Clear();

        public void Set<T>(BBKey<T> key, T value) => _map[key.Id] = value;

        public bool TryGet<T>(BBKey<T> key, out T value)
        {
            if (_map.TryGetValue(key.Id, out var obj) && obj is T t)
            {
                value = t;
                return true;
            }

            // 资产层默认值
            if (_defaults != null)
            {
                foreach (var boxed in from e in _defaults.Defaults where e.Key == key.Name select e.Value.Boxed)
                {
                    switch (boxed)
                    {
                        case T tv:
                            value = tv;
                            return true;
                        // 允许 int->float 的轻量转换
                        case int iv when typeof(T) == typeof(float):
                            value = (T)(((float)iv) as object);
                            return true;
                    }

                    break;
                }
            }

            value = default;
            return false;
        }

        public T GetOr<T>(BBKey<T> key, T fallback = default) => TryGet(key, out T v) ? v : fallback;

        public bool Contains<T>(BBKey<T> key) => _map.ContainsKey(key.Id);

        // 节点局部命名空间：自动拼 nodeGuid 前缀，避免键冲突
        public static BBKey<T> Local<T>(string nodeGuid, string name) => new BBKey<T>($"{nodeGuid}:{name}");
    }
}