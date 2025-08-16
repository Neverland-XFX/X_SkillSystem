using System.Collections.Generic;

namespace XSkillSystem
{
    public static class NodeIdCatalog
    {
        static readonly HashSet<string> _actions = new()
        {
            // 通用
            "EmitLog",
            // 数值
            "ComputeDamage", "ApplyDamage",
            // Buff
            "ApplyBuff", "DispelBuffs",
            // 碰撞
            "SpawnProjectile", "AreaQueryToBB", "AttachAura",
            // 状态
            "ApplyState", "RemoveState", "DispelState",
            // Timeline / Animator
            "PlayTimeline", "StopTimeline", "PauseOrResumeTimeline", "SetTimelineTime", "SetTimelineSpeed",
            "SetAnimParam",
        };

        static readonly HashSet<string> _conditions = new()
        {
            "HasTarget", "IsTimelinePlaying", "HasState", "ChannelFree"
        };

        public static IEnumerable<string> GetActions() => _actions;
        public static IEnumerable<string> GetConditions() => _conditions;

        public static void RegisterAction(string id)
        {
            if (!string.IsNullOrEmpty(id)) _actions.Add(id);
        }

        public static void RegisterCondition(string id)
        {
            if (!string.IsNullOrEmpty(id)) _conditions.Add(id);
        }
    }
}