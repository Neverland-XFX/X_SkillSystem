using UnityEngine;

namespace XSkillSystem
{
    public readonly struct EV_TL_Signal
    {
        public readonly GameObject Owner;
        public readonly string TimelineId;
        public readonly string SignalName; // 优先使用
        public readonly int SignalHash; // 兜底匹配

        public EV_TL_Signal(GameObject owner, string id, string name, int hash)
        {
            Owner = owner;
            TimelineId = id;
            SignalName = name;
            SignalHash = hash;
        }
    }
}