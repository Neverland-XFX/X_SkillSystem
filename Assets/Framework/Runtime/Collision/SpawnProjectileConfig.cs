using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class SpawnProjectileConfig : ScriptableObject
    {
        public ProjectileDef Projectile;
        // 可空：从 Caster.forward
        public Transform Muzzle;
        public Vector3 LocalOffset;
        // 可空：从 TargetGO
        public GameObject HomingTarget;
        public string SkillId = "Skill.Projectile";
    }
}