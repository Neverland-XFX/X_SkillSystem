using UnityEngine;

namespace XSkillSystem
{
    [CreateAssetMenu(menuName = "XSkillSystem/Collision/TargetingRule", fileName = "TargetingRule")]
    public sealed class TargetingRule : ScriptableObject
    {
        public LayerMask Layers = ~0;
        public TeamFilter Team = TeamFilter.EnemiesOnly;
        [Min(1)] public int MaxCount = 16;
        public SortMode Sort = SortMode.Nearest;

        public bool ExcludeCaster = true;

        // 方向过滤（与前向的点积），-1 不启用
        public float MinDot = -1f;
    }
}