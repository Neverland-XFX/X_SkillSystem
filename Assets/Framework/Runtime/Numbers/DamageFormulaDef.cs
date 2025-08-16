using UnityEngine;

namespace XSkillSystem
{
    [CreateAssetMenu(menuName = "XSkillSystem/DamageFormula", fileName = "DamageFormula")]
    public sealed class DamageFormulaDef : ScriptableObject
    {
        public string Id = "Fireball";
        public DamageType Type = DamageType.Fire;

        [Header("Base & Scaling")] public float Base = 10f;

        // 与 Attack 相乘
        public float AttackCoeff = 0f;

        // 与 SpellPower 相乘
        public float SpellPowerCoeff = 1f;

        // 与 Level 相乘
        public float LevelCoeff = 0.0f;

        [Header("Crit (optional override)")] public bool OverrideCrit;
        [Range(0, 1)] public float CritChance = 0.2f;
        [Min(1f)] public float CritDamage = 1.5f;

        // e.g. ["Fire","AoE","Projectile"]
        [Header("Grouping Tags (for OffenseB)")]
        public string[] Tags;

        // DoT/Projectile 推荐 true
        [Header("Snapshot")] public bool SnapshotAtCast = false;
    }
}