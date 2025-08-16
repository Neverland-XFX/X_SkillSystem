namespace XSkillSystem
{
    /// <summary>
    /// 让 BTCore 能从泛型 ctx 中拿到时钟。
    /// </summary>
    public interface IHasClock
    {
        IClock Clock { get; }
    }
}