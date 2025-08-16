using UnityEngine;

namespace XSkillSystem
{
    [CreateAssetMenu(menuName = "XSkillSystem/Buff", fileName = "Buff")]
    public sealed class BuffDef : ScriptableObject
    {
        [Header("Identity")] public string Id = "Buff.Fire.Burn";
        public string DisplayName;
        public DispelType Dispel = DispelType.Magic;
        public StackPolicy Stacks = StackPolicy.Independent;

        [Min(1)] public int MaxStacks = 5;

        // 同一施加者是否只能有一层
        public bool UniquePerSource = false;

        [Header("Lifecycle")] [Min(0f)] public float Duration = 4f;

        // 0 表示无周期 Tick
        [Min(0f)] public float TickInterval = 1f;

        // DoT 推荐开
        public bool SnapshotAtApply = true;

        // 见下方定义
        [Header("Stat Modifiers (per stack)")] public StatModifierDef[] ModifiersPerStack;

        [Header("Behavior Graphs")] public BTGraph OnApply;

        // 每次间隔触发
        public BTGraph OnTick;

        // 挂在拥有者上：拥有者造成伤害时
        public BTGraph OnOwnerDealDamage;

        // 主人受击时
        public BTGraph OnOwnerHit;

        // 被移除/到期
        public BTGraph OnRemove;

        // 例如 DoT
        [Header("Numbers for Tick (optional)")]
        public DamageFormulaDef TickFormula;

        public NumbersConfig NumbersConfig;
    }
}