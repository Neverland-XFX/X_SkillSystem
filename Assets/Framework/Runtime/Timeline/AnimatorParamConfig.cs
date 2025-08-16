using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class AnimatorParamConfig : ScriptableObject
    {
        public string Name = "Speed";

        public enum Kind
        {
            Float,
            Int,
            Bool,
            Trigger
        }

        public Kind Type = Kind.Float;
        public float Float;
        public int Int;
        public bool Bool;
    }
}