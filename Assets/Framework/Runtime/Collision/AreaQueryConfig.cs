using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    // 节点配置
    [System.Serializable]
    public sealed class AreaQueryConfig : ScriptableObject
    {
        public ShapeKind Shape = ShapeKind.Sphere;

        public float Radius = 3f;

        // Capsule 半高 或 Box 半尺寸（单值时等比）
        public float HeightOrHalfExt = 1f;
        public Vector3 BoxHalfExt = new(2, 1, 2);
        public float ConeHalfAngle = 45f;
        public TargetingRule Rule;
        public bool WriteToBB = true;
    }
}