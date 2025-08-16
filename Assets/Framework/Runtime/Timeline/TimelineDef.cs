using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XSkillSystem
{
    [CreateAssetMenu(menuName = "XSkillSystem/Timeline/TimelineDef", fileName = "TimelineDef")]
    public sealed class TimelineDef : ScriptableObject
    {
        // 用于事件与过滤
        [Header("Playable")] public string Id = "Skill.Cast";
        public TimelineAsset Asset;

        [Header("默认绑定（按 Track 名匹配）")] public List<BindingEntry> DefaultBindings = new();

        [Serializable]
        public sealed class BindingEntry
        {
            // 绑定哪个轨
            public string TrackName;
            // 默认对象（可被运行时覆盖）
            public UnityEngine.Object Default;
        }
    }
}