namespace XSkillSystem
{
    internal static class BTContextUtil<TCtx>
    {
        public static float GetDelta(ref TCtx ctx)
        {
            if (ctx is IHasClock hc && hc.Clock != null)
                return (float)hc.Clock.DeltaTime;
            // 回退：假设 60fps
            return 1f / 60f;
        }
    }
}