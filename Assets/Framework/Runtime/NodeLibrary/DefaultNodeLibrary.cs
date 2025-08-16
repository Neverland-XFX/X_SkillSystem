using UnityEngine;

namespace XSkillSystem
{
    public sealed class DefaultNodeLibrary : INodeLibrary<XContext>
    {
        public RTAction<XContext>.Func ResolveAction(string actionId, object userData)
        {
            var nud = userData as NodeUserData; // 可能为 null
            switch (actionId)
            {
                case "EmitLog":
                {
                    var cfg = nud?.Payload as LogConfig;
                    var msg = cfg != null ? cfg.Message : $"[EmitLog] {nud?.RuntimeName}";
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        ctx.EventBus?.Publish(new EV_Log(msg));
                        return BTStatus.Success;
                    };
                }

                case "RollChance":
                {
                    var cfg = nud?.Payload as RollChanceConfig;
                    float p = cfg != null ? Mathf.Clamp01(cfg.Chance) : 0.5f;
                    string outKey = cfg != null ? cfg.OutBoolKey : "LastRoll";
                    var key = new BBKey<bool>(outKey);
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        bool ok = rng.NextDouble() < p;
                        ctx.BB.Set(key, ok);
                        return ok ? BTStatus.Success : BTStatus.Failure;
                    };
                }

                case "SetBBFloat":
                {
                    var cfg = nud?.Payload as SetBBFloatConfig;
                    if (cfg == null) return (ref XContext ctx, IBTRandom rng) => BTStatus.Success;

                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var key = cfg.LocalToNode
                            ? Blackboard.Local<float>(nud.NodeGuid, cfg.Key)
                            : new BBKey<float>(cfg.Key);
                        ctx.BB.Set(key, cfg.Value);
                        return BTStatus.Success;
                    };
                }

                case "CopyTargetToBB":
                {
                    // 将 PrimaryTarget 写入 BB(TargetGO) 并标记 HasTarget
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (ctx.PrimaryTarget != null)
                        {
                            ctx.BB.Set(BBKeys.k_TargetGO, ctx.PrimaryTarget);
                            ctx.BB.Set(BBKeys.k_HasTarget, true);
                            return BTStatus.Success;
                        }

                        ctx.BB.Set(BBKeys.k_HasTarget, false);
                        return BTStatus.Failure;
                    };
                }
                case "ComputeDamage":
                {
                    var cfg = (nud?.Payload as ComputeDamageConfig);
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var targetGo = ctx.BB.GetOr(BBKeys.k_TargetGO, null);
                        if (targetGo == null) return BTStatus.Failure;

                        var targetStatsProvider = targetGo.GetComponent<IStatProvider>(); // 你可用适配器组件实现
                        var attacker = ctx.Stats;
                        if (attacker == null || targetStatsProvider == null) return BTStatus.Failure;

                        var formula = cfg?.Formula ?? ctx.BB.GetOr(BBKeys.k_Formula, null);
                        var numCfg = cfg?.Numbers ?? ctx.BB.GetOr(BBKeys.k_NumCfg, null);
                        if (formula == null || numCfg == null) return BTStatus.Failure;

                        var res = DamagePipeline.Compute(attacker, targetStatsProvider, formula, numCfg, rng, cfg?.Tags,
                            ctx.EventBus, default);
                        if (cfg == null || cfg.WriteToBB)
                        {
                            ctx.BB.Set(BBKeys.k_LastDamage, res.Amount);
                            ctx.BB.Set(BBKeys.k_LastCrit, res.IsCrit);
                        }

                        ctx.EventBus?.Publish(new EV_Log($"[Compute] {res.Breakdown}"));
                        return BTStatus.Success;
                    };
                }

                case "ApplyDamage":
                {
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var targetGo = ctx.BB.GetOr(BBKeys.k_TargetGO, null);
                        if (targetGo == null) return BTStatus.Failure;

                        var hp = targetGo.GetComponent<IHealth>();
                        if (hp == null) return BTStatus.Failure;

                        float dmg = ctx.BB.GetOr(BBKeys.k_LastDamage, 0f);
                        bool cr = ctx.BB.GetOr(BBKeys.k_LastCrit, false);
                        var formula = ctx.BB.GetOr(BBKeys.k_Formula, null);
                        var type = formula != null ? formula.Type : DamageType.True;

                        if (dmg <= 0f) return BTStatus.Failure;

                        hp.TakeDamage(dmg, type, cr);
                        // 发布战斗事件 → BuffSystem 订阅触发
                        ctx.EventBus?.Publish(new EV_DamageApplied(ctx.Caster, targetGo, dmg, cr, type));
                        ctx.EventBus?.Publish(new EV_Log($"[Apply] {targetGo.name} -{dmg:F0}{(cr ? " (CRIT)" : "")}"));
                        return BTStatus.Success;
                    };
                }
                default:
                    return null; // 交给上层其他库去解析（Buff/碰撞/Timeline 后续会补）
            }
        }

        public RTCondition<XContext>.Pred ResolveCondition(string condId, object userData)
        {
            var nud = userData as NodeUserData;
            switch (condId)
            {
                case "HasTarget":
                    return (ref XContext ctx) => ctx.PrimaryTarget != null;

                case "BB_Bool":
                {
                    // 读取 userData 中的 Key（默认为 LastRoll）
                    string keyName = "LastRoll";
                    if (nud?.Payload is RollChanceConfig rc && !string.IsNullOrEmpty(rc.OutBoolKey))
                        keyName = rc.OutBoolKey;
                    var key = new BBKey<bool>(keyName);
                    return (ref XContext ctx) => ctx.BB.GetOr(key, false);
                }

                default:
                    return null;
            }
        }

        public IRTNode<XContext> ResolveCustom(string customId, object userData, IBTTracer tracer)
            => null; // 本库暂不实现
    }
}