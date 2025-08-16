using UnityEngine;

namespace XSkillSystem
{
    [CreateAssetMenu(menuName = "XSkillSystem/NumbersConfig", fileName = "NumbersConfig")]
    public sealed class NumbersConfig : ScriptableObject
    {
        // 等级项系数
        [Header("Armor Curve (Physical)")] public float ArmorK = 50f;

        // 常数项
        public float ArmorC = 400f;

        [Header("Resist Clamp & Defaults")] [Range(-1f, 1f)]
        public float ResistMin = -0.75f;

        [Range(0f, 1f)] public float ResistMax = 0.75f;

        // PVP 衰减
        [Header("Global")] public float PvPScaling = 1.0f;
        public float MinDamage = 1f;
    }
}