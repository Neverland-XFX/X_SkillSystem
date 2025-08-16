using UnityEngine;

namespace XSkillSystem
{
    public readonly struct EV_AreaQueryResult
    {
        public readonly GameObject[] Targets;
        public EV_AreaQueryResult(GameObject[] arr) => Targets = arr;
    }
}