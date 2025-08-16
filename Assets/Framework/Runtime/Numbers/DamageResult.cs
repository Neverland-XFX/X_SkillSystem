namespace XSkillSystem
{
    public readonly struct DamageResult
    {
        public readonly float Amount;
        public readonly bool IsCrit;
        public readonly DamageType Type;
        public readonly DamageBreakdown Breakdown;

        public DamageResult(float amount, bool crit, DamageType type, DamageBreakdown bd)
        {
            Amount = amount;
            IsCrit = crit;
            Type = type;
            Breakdown = bd;
        }
    }
}