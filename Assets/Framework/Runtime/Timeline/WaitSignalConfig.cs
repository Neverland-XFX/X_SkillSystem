using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class WaitSignalConfig : ScriptableObject
    {
        public string TimelineId; // 可空：不过滤
        public string SignalName;
        public int SignalHash;
        public float Timeout = 0f;
    }
}