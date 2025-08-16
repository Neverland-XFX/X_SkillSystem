namespace XSkillSystem
{
    [System.Flags]
    public enum StateMask
    {
        None = 0,
        Stunned = 1 << 0,
        Silenced = 1 << 1,
        KnockedUp = 1 << 2,
        Rooted = 1 << 3,
        Invulnerable = 1 << 4
    }
}