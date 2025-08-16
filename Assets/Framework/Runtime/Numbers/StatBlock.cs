using System.Collections.Generic;

namespace XSkillSystem
{
    /// <summary>
    /// 基础属性 + 运行时修正聚合。
    /// </summary>
    public sealed class StatBlock : IStatWritable
    {
        readonly Dictionary<int, float> _base = new(64);
        readonly Dictionary<int, float> _adds = new(128);
        readonly Dictionary<int, float> _muls = new(128);
        readonly Dictionary<string, StatModifier> _unique = new(64);

        public float GetStat(int id)
        {
            _base.TryGetValue(id, out var b);
            _adds.TryGetValue(id, out var a);
            _muls.TryGetValue(id, out var m);
            return (b + a) * (1f + m);
        }

        public void SetBase(int id, float v) => _base[id] = v;

        public void AddModifier(StatModifier mod)
        {
            if (mod == null) return;
            if (!string.IsNullOrEmpty(mod.UniqueKey))
            {
                if (_unique.TryGetValue(mod.UniqueKey, out var old))
                {
                    // 替换策略：取更大值/刷新时长（可按需调整）
                    old.Value = System.MathF.Max(old.Value, mod.Value);
                    old.Stacks = System.Math.Max(old.Stacks, mod.Stacks);
                    old.TimeLeft = System.MathF.Max(old.TimeLeft, mod.TimeLeft);
                    return;
                }
                _unique[mod.UniqueKey] = mod;
            }

            var dict = mod.Op == ModOp.Add ? _adds : _muls;
            dict.TryGetValue(mod.StatId, out var cur);
            dict[mod.StatId] = cur + mod.Effective;
        }

        public void RemoveModifier(StatModifier mod)
        {
            if (mod == null) return;
            if (!string.IsNullOrEmpty(mod.UniqueKey)) _unique.Remove(mod.UniqueKey);

            var dict = mod.Op == ModOp.Add ? _adds : _muls;
            if (dict.TryGetValue(mod.StatId, out var cur))
            {
                cur -= mod.Effective;
                if (System.MathF.Abs(cur) < 1e-6f) dict.Remove(mod.StatId);
                else dict[mod.StatId] = cur;
            }
        }

        public void ClearModifiers()
        {
            _adds.Clear(); _muls.Clear(); _unique.Clear();
        }
    }
}