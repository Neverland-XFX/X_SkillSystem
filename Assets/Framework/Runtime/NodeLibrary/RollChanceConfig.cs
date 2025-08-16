using System;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// Action/Condition 的配置示例（可在节点 UserData 填入）。
    /// </summary>
    [Serializable]
    public sealed class RollChanceConfig : ScriptableObject
    {
        [Range(0f, 1f)] public float Chance = 0.25f;
        // 写到哪个 BB Key
        public string OutBoolKey = "LastRoll"; 
    }
}