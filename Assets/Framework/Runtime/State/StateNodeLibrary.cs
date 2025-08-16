using UnityEngine;

namespace XSkillSystem
{
    public sealed class StateNodeLibrary : INodeLibrary<XContext>
    {
        static StateMachine GetSM(GameObject go) => go ? go.GetComponent<StateMachine>() : null;

        public RTAction<XContext>.Func ResolveAction(string id, object ud)
        {
            var nud = ud as NodeUserData;
            switch (id)
            {
                case "ApplyState":
                {
                    var cfg = nud?.Payload as ApplyStateConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null) return BTStatus.Failure;
                        var go = cfg.OnTarget
                            ? (ctx.BB.GetOr(new BBKey<GameObject>("TargetGO"), ctx.PrimaryTarget))
                            : ctx.Caster;
                        var sm = GetSM(go);
                        if (sm == null) return BTStatus.Failure;
                        sm.Apply(cfg.State, cfg.Duration, cfg.Stacks, respectImmunity: true, refresh: true);
                        return BTStatus.Success;
                    };
                }
                case "RemoveState":
                {
                    var cfg = nud?.Payload as RemoveStateConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null) return BTStatus.Failure;
                        var go = cfg.OnTarget
                            ? (ctx.BB.GetOr(new BBKey<GameObject>("TargetGO"), ctx.PrimaryTarget))
                            : ctx.Caster;
                        var sm = GetSM(go);
                        if (sm == null) return BTStatus.Failure;
                        sm.Remove(cfg.State, cfg.AllStacks);
                        return BTStatus.Success;
                    };
                }
                case "DispelStates":
                {
                    var cfg = nud?.Payload as DispelStateConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null) return BTStatus.Failure;
                        var go = cfg.OnTarget
                            ? (ctx.BB.GetOr(new BBKey<GameObject>("TargetGO"), ctx.PrimaryTarget))
                            : ctx.Caster;
                        var sm = GetSM(go);
                        if (sm == null) return BTStatus.Failure;
                        sm.Dispel(cfg.Mask);
                        return BTStatus.Success;
                    };
                }
                default: return null;
            }
        }

        public RTCondition<XContext>.Pred ResolveCondition(string id, object ud)
        {
            var nud = ud as NodeUserData;
            switch (id)
            {
                case "HasState":
                {
                    var cfg = nud?.Payload as RemoveStateConfig; // 只用 State 字段
                    return (ref XContext ctx) =>
                    {
                        var go = cfg != null && cfg.OnTarget
                            ? (ctx.BB.GetOr(new BBKey<GameObject>("TargetGO"), ctx.PrimaryTarget))
                            : ctx.Caster;
                        var sm = GetSM(go);
                        return sm != null && sm.Has(cfg != null ? cfg.State : StateId.None);
                    };
                }
                case "IsChannelFree":
                {
                    // UserData 中放一个自定义 ScriptableObject 指定 Channel & MinPriority，这里简化写死
                    return (ref XContext ctx) =>
                    {
                        var sm = GetSM(ctx.Caster);
                        return sm != null && sm.IsChannelFree(Channel.Cast);
                    };
                }
                default: return null;
            }
        }

        public IRTNode<XContext> ResolveCustom(string id, object ud, IBTTracer tracer) => null;
    }
}