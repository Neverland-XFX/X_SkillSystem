namespace XSkillSystem
{
    [System.Serializable]
    public struct StatModifierDef
    {
        // 对应 Numbers.StatId（看需求换也可换枚举）
        public int StatId;

        // Add / Mul
        public ModOp Op;

        public float ValuePerStack;

        // 可选：唯一键
        public string UniqueKey;
    }
}