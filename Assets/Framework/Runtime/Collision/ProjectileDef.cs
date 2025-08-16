using UnityEngine;

namespace XSkillSystem
{
    [CreateAssetMenu(menuName = "XSkillSystem/Collision/Projectile", fileName = "ProjectileDef")]
    public sealed class ProjectileDef : ScriptableObject
    {
        public GameObject VisualPrefab;
        public float Speed = 20f;

        public float Accel = 0f;

        // (0,-9.8,0) 可重力
        public Vector3 Gravity = Vector3.zero;
        public float Lifetime = 8f;

        public float Radius = 0.1f;

        // 穿透次数
        public int Pierce = 0;

        // 对同一目标最多命中次数
        public int MaxHitsPerTarget = 1;

        public LayerMask HitLayers = ~0;

        // true: 连续检测；false: Overlap
        public bool UseSphereCast = true;

        public bool Homing = false;

        // 每秒最大转向
        public float HomingTurnRateDeg = 360f;

        // 过滤命中对象
        public TargetingRule TargetRule;

        // 命中回调（行为树）
        public BTGraph OnHitSubTree;
    }
}