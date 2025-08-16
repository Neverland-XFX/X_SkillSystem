using UnityEngine;

namespace XSkillSystem
{
    public static class TargetFilter
    {
        public static bool Pass(GameObject caster, GameObject target, TargetingRule rule, Vector3 forward)
        {
            if (target == null) return false;
            if (rule == null) return true;

            // Layer
            if (((1 << target.layer) & rule.Layers) == 0) return false;

            // Team
            if (rule.Team != TeamFilter.Any)
            {
                int tc = caster.TryGetComponent<ITeamProvider>(out var ct) ? ct.TeamId : 0;
                int tt = target.TryGetComponent<ITeamProvider>(out var ttv) ? ttv.TeamId : 0;
                bool isAlly = tc == tt;
                if (rule.Team == TeamFilter.AlliesOnly && !isAlly) return false;
                if (rule.Team == TeamFilter.EnemiesOnly && (isAlly || target == caster)) return false;
                if (rule.Team == TeamFilter.SelfOnly && target != caster) return false;
            }

            if (rule.ExcludeCaster && target == caster) return false;

            if (rule.MinDot > -0.999f && forward != Vector3.zero)
            {
                var to = (target.transform.position - caster.transform.position).normalized;
                if (Vector3.Dot(forward, to) < rule.MinDot) return false;
            }

            return true;
        }
    }
}