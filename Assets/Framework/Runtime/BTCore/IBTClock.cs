namespace XSkillSystem
{
    /// <summary>
    /// 可抽换的时钟；默认外部传入delta即可。
    /// </summary>
    public interface IBTClock
    {
        double Time { get; }
        double DeltaTime { get; }
    }
}