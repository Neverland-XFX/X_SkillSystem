using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    [System.Serializable]
    public sealed class PlayTimelineConfig : ScriptableObject
    {
        public TimelineDef Def;
        public float StartTime = 0f;
        public float Speed = 1f;
        public List<Entry> Overrides = new();

        [System.Serializable]
        public sealed class Entry
        {
            public string TrackName;
            public Object Target;
        }
    }
}