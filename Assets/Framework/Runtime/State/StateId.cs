// Assets/Framework/Runtime/State/StateId.cs

using System;

namespace XSkillSystem
{
    /// <summary>
    /// 统一用 int 作为底层类型，确保 Unity 可稳定序列化。
    /// 注意：位值请放在 0..30（1<<31 是负数，不建议用）。
    /// </summary>
    [Flags]
    public enum StateId : int
    {
        None = 0,

        // —— 常用控制类 —— //
        Stunned = 1 << 0, // 眩晕：禁止一切动作
        Silenced = 1 << 1, // 沉默：禁止施法
        Rooted = 1 << 2, // 定身：禁止位移
        KnockedUp = 1 << 3, // 击飞
        Disarmed = 1 << 4, // 缴械：禁普攻

        // —— 通用状态 —— //
        Unstoppable = 1 << 5, // 霸体：免疫控制（不吃 Stun/KnockUp 等）
        Shielded = 1 << 6, // 护盾
        Stealthed = 1 << 7, // 潜行
        Haste = 1 << 8, // 加速
        Slow = 1 << 9, // 减速
        Invulnerable = 1 << 10, // 无敌：免疫伤害
        All = ~0,

        // 预留位，按需扩展到 1<<30 以内
        Custom11 = 1 << 11,
        Custom12 = 1 << 12,
        Custom13 = 1 << 13,
        Custom14 = 1 << 14,
        Custom15 = 1 << 15,
        Custom16 = 1 << 16,
        Custom17 = 1 << 17,
        Custom18 = 1 << 18,
        Custom19 = 1 << 19,
        Custom20 = 1 << 20,
        Custom21 = 1 << 21,
        Custom22 = 1 << 22,
        Custom23 = 1 << 23,
        Custom24 = 1 << 24,
        Custom25 = 1 << 25,
        Custom26 = 1 << 26,
        Custom27 = 1 << 27,
        Custom28 = 1 << 28,
        Custom29 = 1 << 29,
        Custom30 = 1 << 30,
    }
}