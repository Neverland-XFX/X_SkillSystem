using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class ApplyStateConfig : ScriptableObject
    {
        public StateId State = StateId.Stunned;
        [Min(0f)] public float Duration = 1f;
        [Min(1)] public int Stacks = 1;
        public bool OnTarget = true;
    }
}