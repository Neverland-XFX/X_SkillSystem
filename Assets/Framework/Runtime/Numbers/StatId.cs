namespace XSkillSystem
{
    /// <summary>
    /// 常用属性枚举（仍保留 IStatProvider 的 int 访问）。
    /// </summary>
    public enum StatId : int
    {
        // 进攻
        Attack = 100, // 物攻
        SpellPower = 101, // 法强
        CritChance = 110, // 0~1
        CritDamage = 111, // 暴伤倍数（例如 1.5, 2.0）
        AdditiveDamageBonus = 120, // 组A：全局加成（+%）
        ElementalBonus_Fire = 121, // 组B：元素/标签加成（+%）
        ElementalBonus_Ice = 122,
        SkillBonus = 123, // 某技能/标签加成（+%）

        // 防御
        Armor = 200, // 物理减伤曲线
        Resist_Fire = 210, // 元素抗性（-1~1），最终 clamp 到 NumbersConfig
        Resist_Ice = 211,
        DamageReduction = 220, // 组：减伤%（0~1）
        Shield = 221, // 可选：护盾（按最终伤害吃）

        // 穿透/易伤
        ArmorPenetration = 300, // 固定或比例穿透（比例放到 301）
        ArmorPenetrationPct = 301, // 0~1
        ResistPenetration_Fire = 310,
        Vulnerability = 320, // 受伤增加%（加入防御后乘区，通常放到 OffenseB/DefenseB）

        // 其它
        PvpScaling = 400, // PVP 衰减（0~1）
        Level = 401, // 用于护甲曲线与伤害成长
    }
}