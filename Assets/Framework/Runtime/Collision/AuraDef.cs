using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// Aura（持续域）定义：形状、筛选规则与触发子图。
    /// AuraZone 组件会按 Interval 定期扫描，并在进入/离开/每次扫描时分别跑对应 BT 子图。
    /// </summary>
    [CreateAssetMenu(menuName = "XSkillSystem/Collision/Aura", fileName = "AuraDef")]
    public sealed class AuraDef : ScriptableObject
    {
        [Header("Shape")] public ShapeKind Shape = ShapeKind.Sphere;

        [Tooltip("Sphere/Capsule 半径；Box 使用 BoxHalfExt。")] [Min(0f)]
        public float Radius = 5f;

        [Tooltip("Box 一半尺寸（世界空间）。")] public Vector3 BoxHalfExt = new Vector3(3f, 1f, 3f);

        [Tooltip("Capsule 半高度（从中心到端盖的距离）。")] [Min(0f)]
        public float CapsuleHalfHeight = 2f;

        [Header("Filter & Scan")] [Tooltip("用于层、阵营、数量、排序、视锥等的过滤规则。")]
        public TargetingRule Rule;

        [Tooltip("扫描间隔（秒）。数值越小越实时但开销更高。")] [Min(0.05f)]
        public float Interval = 0.5f;

        [Header("Behavior (BT Graphs)")] [Tooltip("目标进入 Aura 时运行的子图（Target = 进入者）。")]
        public BTGraph OnEnter;

        [Tooltip("目标离开 Aura 时运行的子图（Target = 离开者）。")]
        public BTGraph OnExit;

        [Tooltip("每次扫描对每个在域内的目标运行一次（Target = 该目标）。")]
        public BTGraph OnTickEach;

        [Header("Notes")] [TextArea] public string Description;
    }

}