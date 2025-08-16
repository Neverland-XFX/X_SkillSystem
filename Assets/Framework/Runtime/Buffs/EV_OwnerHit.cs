using UnityEngine;

namespace XSkillSystem
{
    // 被命中（未必扣血，例如格挡/护盾等，待需求自定义扩展）
    public readonly struct EV_OwnerHit
    {
        public readonly GameObject Owner;
        public readonly GameObject Attacker;
        public readonly float Amount;
        public readonly DamageType Type;

        public EV_OwnerHit(GameObject o, GameObject a, float amt, DamageType ty)
        {
            Owner = o;
            Attacker = a;
            Amount = amt;
            Type = ty;
        }
    }
}