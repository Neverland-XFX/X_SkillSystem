using System;

namespace XSkillSystem
{
    public readonly struct DamageBreakdown
    {
        public readonly float Base;
        public readonly float OffenseA;
        public readonly float CritMul;
        public readonly float OffenseB;
        public readonly float DefenseA;
        public readonly float DefenseB;
        public readonly float PvP;
        public readonly float Final;
        public readonly bool IsCrit;

        public DamageBreakdown(float @base, float offA, float crit, float offB, float defA, float defB, float pvp,
            float final, bool isCrit)
        {
            Base = @base;
            OffenseA = offA;
            CritMul = crit;
            OffenseB = offB;
            DefenseA = defA;
            DefenseB = defB;
            PvP = pvp;
            Final = final;
            IsCrit = isCrit;
        }

        public override string ToString()
            =>
                $"Base={Base:F1} *A({OffenseA:F3}) *Crit({CritMul:F2}) *B({OffenseB:F3}) *DefA({DefenseA:F3}) *DefB({DefenseB:F3}) *PvP({PvP:F3}) => {Final:F1}";
    }
}