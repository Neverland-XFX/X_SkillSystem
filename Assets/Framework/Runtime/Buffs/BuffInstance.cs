using UnityEngine;

namespace XSkillSystem
{
    public sealed class BuffInstance
    {
        public readonly BuffDef Def;

        // 被施加者
        public readonly GameObject Owner;

        // 施加者
        public readonly GameObject Source;
        public int Stacks;

        public float TimeLeft;

        // 距离下次Tick
        public float NextTickLeft;

        // 可选：攻击方快照（DoT等用）
        public SnapshotStats Snapshot;
        public bool Expired => TimeLeft <= 0f;

        // 修正器持有，便于移除
        public StatModifier[] ActiveMods;

        // 行为树运行器
        public BuffRunner Runner;

        public BuffInstance(BuffDef def, GameObject owner, GameObject source)
        {
            Def = def;
            Owner = owner;
            Source = source;
            Stacks = 1;
            TimeLeft = def.Duration;
            NextTickLeft = def.TickInterval;
        }
    }
}