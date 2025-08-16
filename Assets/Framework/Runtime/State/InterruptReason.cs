namespace XSkillSystem
{
    public enum InterruptReason : byte
    {
        StateApplied,
        ChannelPreempt,
        ExternalCancel,
        OwnerDeath,
        DamageThreshold
    }
}