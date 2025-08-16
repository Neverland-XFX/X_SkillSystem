namespace XSkillSystem
{
    public static class ResistModel
    {
        /// <summary>
        /// 物理减伤： reduction = Armor / (Armor + K*Level + C)
        /// </summary>
        public static float PhysicalReduction(float armor, float attackerLevel, float K, float C)
        {
            var denom = armor + K * attackerLevel + C;
            if (denom <= 1e-6f) return 0f;
            var r = armor / denom;
            // 上限保护
            if (r < 0f) r = 0f; if (r > 0.95f) r = 0.95f;
            return r;
        }

        /// <summary>
        /// 元素抗性：(-1~1)，之后 clamp，返回最终乘数 (1 - clamp(resist - penetration))
        /// </summary>
        public static float ElementalMultiplier(float resist, float pen, float min, float max)
        {
            float eff = resist - pen;
            if (eff < min) eff = min;
            if (eff > max) eff = max;
            return 1f - eff;
        }
    }
}