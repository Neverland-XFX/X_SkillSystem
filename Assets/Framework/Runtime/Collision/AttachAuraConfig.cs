using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class AttachAuraConfig : ScriptableObject
    {
        public AuraDef Aura;
        // 默认挂在 Caster
        public Transform AttachTo;
    }
}