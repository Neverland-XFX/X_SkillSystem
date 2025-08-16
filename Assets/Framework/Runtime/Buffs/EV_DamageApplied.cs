 using UnityEngine;

namespace XSkillSystem
{
    // 造成伤害后
    public readonly struct EV_DamageApplied
    {
        public readonly GameObject Attacker;
        public readonly GameObject Target;
        public readonly float Amount;
        public readonly bool IsCrit;
        public readonly DamageType Type;

        public EV_DamageApplied(GameObject a, GameObject t, float amt, bool crit, DamageType ty)
        {
            Attacker = a;
            Target = t;
            Amount = amt;
            IsCrit = crit;
            Type = ty;
        }
    }
}