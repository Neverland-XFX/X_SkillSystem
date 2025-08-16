using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    public sealed class CollisionNodeLibrary : INodeLibrary<XContext>
    {
        readonly ProjectilePool _pool;
        readonly EventBus _bus;
        readonly DefaultNodeLibrary _baseLib;
        readonly BuffNodeLibrary _buffLib;
        readonly NumbersConfig _numbers;

        public CollisionNodeLibrary(ProjectilePool pool, EventBus bus, DefaultNodeLibrary baseLib,
            BuffNodeLibrary buffLib, NumbersConfig numbers)
        {
            _pool = pool;
            _bus = bus;
            _baseLib = baseLib;
            _buffLib = buffLib;
            _numbers = numbers;
        }

        public RTAction<XContext>.Func ResolveAction(string id, object ud)
        {
            var nud = ud as NodeUserData;
            switch (id)
            {
                case "AreaQueryToBB":
                {
                    var cfg = nud?.Payload as AreaQueryConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null) return BTStatus.Failure;
                        var outList = ctx.Targets ?? (ctx.Targets = new List<GameObject>(16));
                        var rule = cfg.Rule;
                        int count = 0;
                        var pos = ctx.Caster ? ctx.Caster.transform.position : Vector3.zero;
                        var fwd = ctx.Caster ? ctx.Caster.transform.forward : Vector3.forward;

                        switch (cfg.Shape)
                        {
                            case ShapeKind.Sphere:
                                count = AreaQuery.Sphere(pos, cfg.Radius, rule, ctx.Caster, fwd, outList); break;
                            case ShapeKind.Capsule:
                            {
                                var p0 = pos + Vector3.up * cfg.HeightOrHalfExt;
                                var p1 = pos - Vector3.up * cfg.HeightOrHalfExt;
                                count = AreaQuery.Capsule(p0, p1, cfg.Radius, rule, ctx.Caster, fwd, outList);
                            }
                                break;
                            case ShapeKind.Box:
                            {
                                var half = cfg.BoxHalfExt.sqrMagnitude > 0.01f
                                    ? cfg.BoxHalfExt
                                    : Vector3.one * cfg.HeightOrHalfExt;
                                count = AreaQuery.Box(pos, half, ctx.Caster.transform.rotation, rule, ctx.Caster, fwd,
                                    outList);
                            }
                                break;
                            case ShapeKind.Cone:
                                count = AreaQuery.Cone(pos, fwd, cfg.Radius, cfg.ConeHalfAngle, rule, ctx.Caster,
                                    outList); break;
                        }

                        ctx.EventBus?.Publish(new EV_AreaQueryResult(outList.ToArray()));
                        return count > 0 ? BTStatus.Success : BTStatus.Failure;
                    };
                }
                case "SpawnProjectile":
                {
                    var cfg = nud?.Payload as SpawnProjectileConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null || cfg.Projectile == null || _pool == null) return BTStatus.Failure;
                        var muzzle = cfg.Muzzle ? cfg.Muzzle : (ctx.Caster ? ctx.Caster.transform : null);
                        if (muzzle == null) return BTStatus.Failure;

                        var pos = muzzle.TransformPoint(cfg.LocalOffset);
                        var dir = muzzle.forward;
                        var pr = _pool.Get();
                        pr.Init(cfg.Projectile, ctx.Caster, pos, dir, _pool, _bus, _baseLib, _buffLib, _numbers,
                            cfg.SkillId,
                            cfg.HomingTarget ? cfg.HomingTarget : ctx.BB.GetOr(BBKeys.k_TargetGO, null));
                        return BTStatus.Success;
                    };
                }
                case "AttachAura":
                {
                    var cfg = nud?.Payload as AttachAuraConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null || cfg.Aura == null) return BTStatus.Failure;
                        var host = cfg.AttachTo ? cfg.AttachTo.gameObject : ctx.Caster;
                        if (host == null) return BTStatus.Failure;

                        var zone = host.GetComponent<AuraZone>() ?? host.AddComponent<AuraZone>();
                        zone.Def = cfg.Aura;
                        zone.Caster = ctx.Caster;
                        zone.Bus = _bus;
                        zone.BaseLib = _baseLib;
                        zone.BuffLib = _buffLib;
                        return BTStatus.Success;
                    };
                }
                default: return null;
            }
        }

        public RTCondition<XContext>.Pred ResolveCondition(string id, object ud)
        {
            return id switch
            {
                "HasTargetsInBB" => (ref XContext ctx) => ctx.Targets != null && ctx.Targets.Count > 0,
                _ => null
            };
        }

        public IRTNode<XContext> ResolveCustom(string id, object ud, IBTTracer tracer) => null;
    }
}