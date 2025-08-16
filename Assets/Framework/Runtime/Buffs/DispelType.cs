using System;

namespace XSkillSystem
{
    [Flags]
    public enum DispelType : uint
    {
        None = 0,
        Magic = 1 << 0,
        Curse = 1 << 1,
        Bleed = 1 << 2,
        Poison = 1 << 3,
        Control = 1 << 4,
        All = 0xFFFFFFFF
    }
}