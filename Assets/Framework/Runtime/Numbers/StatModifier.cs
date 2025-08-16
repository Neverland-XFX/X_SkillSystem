namespace XSkillSystem
{
    /// <summary>
    /// 修正器：支持唯一键（防重复叠加）、层数、过期时间（-1 常驻）。
    /// </summary>
    public sealed class StatModifier
    {
        public readonly int StatId;
        public readonly ModOp Op;
        public float Value; // Add: 直接相加；Mul: 累加到乘区（0.2 = +20%）
        public int Stacks; // 叠层数（Value * Stacks）
        public readonly string UniqueKey;
        public float TimeLeft; // 秒；由外部系统驱动减少

        public StatModifier(int statId, ModOp op, float value, int stacks = 1, string uniqueKey = null,
            float timeLeft = -1f)
        {
            StatId = statId;
            Op = op;
            Value = value;
            Stacks = stacks;
            UniqueKey = uniqueKey;
            TimeLeft = timeLeft;
        }

        public float Effective => Value * Stacks;
    }
}