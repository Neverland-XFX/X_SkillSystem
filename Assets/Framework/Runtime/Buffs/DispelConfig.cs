using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class DispelConfig : ScriptableObject
    {
        public DispelType Mask = DispelType.Magic;
        [Min(1)] public int Count = 1;
        public bool OnTarget = true;
    }
}