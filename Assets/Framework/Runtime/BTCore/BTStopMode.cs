namespace XSkillSystem
{
    public enum BTStopMode : byte
    {
        // Soft: 等当前节点返回；
        Soft,
        // Hard: 递归Abort
        Hard
    } 
}