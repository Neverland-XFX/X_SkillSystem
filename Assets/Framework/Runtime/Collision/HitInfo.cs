using UnityEngine;

namespace XSkillSystem
{
    public readonly struct HitInfo
    {
        public readonly GameObject Caster, Target;
        public readonly Vector3 Point, Normal, Direction;
        public readonly float Time;
        public readonly string SkillId;

        public HitInfo(GameObject c, GameObject t, Vector3 p, Vector3 n, Vector3 dir, float time, string sid)
        {
            Caster = c;
            Target = t;
            Point = p;
            Normal = n;
            Direction = dir;
            Time = time;
            SkillId = sid;
        }
    }
}