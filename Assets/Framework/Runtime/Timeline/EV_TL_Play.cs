using UnityEngine;

namespace XSkillSystem
{
    public readonly struct EV_TL_Play
    {
        public readonly GameObject Owner;
        public readonly string TimelineId;

        public EV_TL_Play(GameObject owner, string id)
        {
            Owner = owner;
            TimelineId = id;
        }
    }
}