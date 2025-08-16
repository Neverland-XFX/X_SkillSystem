namespace XSkillSystem
{
    /// <summary>
    /// 默认：使用 Unity 的 Time。
    /// </summary>
    public sealed class UnityClock : IClock
    {
        public double Time => UnityEngine.Time.timeAsDouble;
        public double DeltaTime => UnityEngine.Time.deltaTime;
    }
}