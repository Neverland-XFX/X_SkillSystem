namespace XSkillSystem
{
    public enum StackPolicy : byte
    {
        // 每次应用生成新层，独立计时
        Independent,

        // 叠数+1，刷新持续
        RefreshTime,

        // 叠数+1，共享持续，不刷新（或按需刷新）
        AdditiveStacks
    }
}