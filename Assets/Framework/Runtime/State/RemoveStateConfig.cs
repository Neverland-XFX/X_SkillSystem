using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class RemoveStateConfig : ScriptableObject
    {
        public StateId State = StateId.Stunned;
        public bool OnTarget = true;
        public bool AllStacks = true;
    }
}