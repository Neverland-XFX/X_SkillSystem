using System;
using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 资产层默认值。在 Graph Asset 上挂一个 BlackboardAsset。
    /// </summary>
    [CreateAssetMenu(menuName = "XSkillSystem/BlackboardAsset", fileName = "Blackboard")]
    public sealed class BlackboardAsset : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string Key;
            public BlackboardValue Value;
        }

        public List<Entry> Defaults = new();
    }
}