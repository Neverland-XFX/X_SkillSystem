using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class PauseTimelineConfig : ScriptableObject
    {
        // true=Pause, false=Resume
        public bool Pause = true;
    } 
}