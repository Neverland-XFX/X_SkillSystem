using System.Collections.Generic;

namespace XSkillSystem
{
    /// <summary>
    /// 在“施放时刻”将部分属性拍平，后续读快照保持一致。
    /// </summary>
    public sealed class SnapshotStats : IStatProvider
    {
        readonly Dictionary<int, float> _cache = new(64);
        public SnapshotStats(IStatProvider source, params int[] ids)
        {
            if (source == null || ids == null) return;
            for (int i = 0; i < ids.Length; i++) _cache[ids[i]] = source.GetStat(ids[i]);
        }
        public float GetStat(int id) => _cache.TryGetValue(id, out var v) ? v : 0f;
    }
}