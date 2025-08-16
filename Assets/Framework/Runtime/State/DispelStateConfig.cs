using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class DispelStateConfig : ScriptableObject
    {
        public StateId Mask = StateId.All;
        public bool OnTarget = true;
    }
}