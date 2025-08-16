using UnityEngine;

namespace XSkillSystem
{
    // 配置载体（可挂到节点 UserData）
    [System.Serializable]
    public sealed class ApplyBuffConfig : ScriptableObject
    {
        public BuffDef Buff;
        [Min(1)] public int Stacks = 1;
        public bool ToTarget = true; // true: 给目标；false: 给自己
    }
}