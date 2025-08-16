namespace XSkillSystem
{
    /// <summary>
    /// 可确定性的随机接口。
    /// </summary>
    public interface IBTRandom
    {
        int Next(); // full int
        int Next(int maxExclusive);
        int Next(int minInclusive, int maxExclusive);
        double NextDouble();
        float NextFloat01(); // [0,1)
        int NextIntInclusive(int min, int max); // [min, max]
    }
}