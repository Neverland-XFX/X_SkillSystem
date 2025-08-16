using UnityEngine;

namespace XSkillSystem
{
    public sealed class BuffNodeLibrary : INodeLibrary<XContext>
    {
        readonly NumbersConfig _numbers;

        public BuffNodeLibrary(NumbersConfig numbers)
        {
            _numbers = numbers;
        }

        public RTAction<XContext>.Func ResolveAction(string id, object ud)
        {
            var nud = ud as NodeUserData;
            switch (id)
            {
                case "ApplyBuff":
                {
                    var cfg = nud?.Payload as ApplyBuffConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null || cfg.Buff == null) return BTStatus.Failure;
                        var targetGo = cfg.ToTarget
                            ? (ctx.BB.GetOr(new BBKey<GameObject>("TargetGO"), ctx.PrimaryTarget))
                            : ctx.Caster;
                        if (targetGo == null) return BTStatus.Failure;
                        var bs = targetGo.GetComponent<BuffSystem>();
                        if (bs == null) return BTStatus.Failure;
                        bs.Numbers = bs.Numbers ?? _numbers;
                        bs.Apply(cfg.Buff, ctx.Caster, cfg.Stacks);
                        return BTStatus.Success;
                    };
                }

                case "Dispel":
                {
                    var cfg = nud?.Payload as DispelConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var targetGo = (cfg != null && cfg.OnTarget)
                            ? (ctx.BB.GetOr(new BBKey<GameObject>("TargetGO"), ctx.PrimaryTarget))
                            : ctx.Caster;
                        if (targetGo == null) return BTStatus.Failure;
                        var bs = targetGo.GetComponent<BuffSystem>();
                        if (bs == null) return BTStatus.Failure;
                        var mask = cfg != null ? cfg.Mask : DispelType.All;
                        var count = cfg != null ? cfg.Count : 1;
                        bs.Dispel(mask, count);
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
                case "HasBuff":
                {
                    // 复用载体取 Buff
                    var cfg = nud?.Payload as ApplyBuffConfig;
                    return (ref XContext ctx) =>
                    {
                        var targetGo = cfg != null && cfg.ToTarget
                            ? (ctx.BB.GetOr(new BBKey<GameObject>("TargetGO"), ctx.PrimaryTarget))
                            : ctx.Caster;
                        if (targetGo == null || cfg == null || cfg.Buff == null) return false;
                        var bs = targetGo.GetComponent<BuffSystem>();
                        return bs != null && bs.Has(cfg.Buff);
                    };
                }
                default: return null;
            }
        }

        public IRTNode<XContext> ResolveCustom(string id, object ud, IBTTracer t) => null;
    }
}