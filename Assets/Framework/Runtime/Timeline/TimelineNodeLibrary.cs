using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XSkillSystem
{
    public sealed class TimelineNodeLibrary : INodeLibrary<XContext>
    {
        static TimelineRunner GetRunner(GameObject go)
        {
            if (!go) return null;
            return go.GetComponent<TimelineRunner>() ?? go.AddComponent<TimelineRunner>();
        }

        static Animator GetAnimator(GameObject go) =>
            go ? (go.GetComponentInChildren<Animator>() ?? go.GetComponent<Animator>()) : null;

        public RTAction<XContext>.Func ResolveAction(string id, object ud)
        {
            var nud = ud as NodeUserData;
            switch (id)
            {
                case "PlayTimeline":
                {
                    var cfg = nud?.Payload as PlayTimelineConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        if (cfg == null || cfg.Def == null) return BTStatus.Failure;
                        var runner = GetRunner(ctx.Caster);
                        if (!runner) return BTStatus.Failure;

                        Dictionary<string, Object> map = null;
                        if (cfg.Overrides != null && cfg.Overrides.Count > 0)
                        {
                            map = new Dictionary<string, Object>(cfg.Overrides.Count);
                            foreach (var e in cfg.Overrides.Where(e => !string.IsNullOrEmpty(e.TrackName) && e.Target))
                                map[e.TrackName] = e.Target;
                        }

                        runner.Caster = ctx.Caster;
                        runner.Play(cfg.Def, map, cfg.StartTime, cfg.Speed);
                        return BTStatus.Success;
                    };
                }
                case "StopTimeline":
                {
                    var cfg = nud?.Payload as StopTimelineConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var r = GetRunner(ctx.Caster);
                        if (!r) return BTStatus.Failure;
                        if (cfg == null || cfg.StopIfPlaying)
                        {
                            if (r.IsPlaying())
                            {
                                r.Stop();
                                return BTStatus.Success;
                            }

                            return BTStatus.Failure;
                        }

                        r.Stop();
                        return BTStatus.Success;
                    };
                }
                case "PauseOrResumeTimeline":
                {
                    var cfg = nud?.Payload as PauseTimelineConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var r = GetRunner(ctx.Caster);
                        if (!r || r.Director == null) return BTStatus.Failure;
                        if (cfg != null && cfg.Pause) r.Director.Pause();
                        else r.Director.Resume();
                        return BTStatus.Success;
                    };
                }
                case "SetTimelineTime":
                {
                    var cfg = nud?.Payload as SetTLTimeConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var r = GetRunner(ctx.Caster);
                        if (!r || r.Director == null || cfg == null) return BTStatus.Failure;
                        r.Director.time = cfg.Time;
                        r.Director.Evaluate(); // 立即生效
                        return BTStatus.Success;
                    };
                }
                case "SetTimelineSpeed":
                {
                    var cfg = nud?.Payload as SetTLSpeedConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var r = GetRunner(ctx.Caster);
                        if (!r || cfg == null) return BTStatus.Failure;
                        r.SetSpeed(cfg.Speed);
                        return BTStatus.Success;
                    };
                }
                case "SetAnimParam":
                {
                    var cfg = nud?.Payload as AnimatorParamConfig;
                    return (ref XContext ctx, IBTRandom rng) =>
                    {
                        var anim = GetAnimator(ctx.Caster);
                        if (!anim || cfg == null || string.IsNullOrEmpty(cfg.Name)) return BTStatus.Failure;
                        switch (cfg.Type)
                        {
                            case AnimatorParamConfig.Kind.Float: anim.SetFloat(cfg.Name, cfg.Float); break;
                            case AnimatorParamConfig.Kind.Int: anim.SetInteger(cfg.Name, cfg.Int); break;
                            case AnimatorParamConfig.Kind.Bool: anim.SetBool(cfg.Name, cfg.Bool); break;
                            case AnimatorParamConfig.Kind.Trigger: anim.SetTrigger(cfg.Name); break;
                        }

                        return BTStatus.Success;
                    };
                }
                default: return null;
            }
        }

        public RTCondition<XContext>.Pred ResolveCondition(string id, object ud)
        {
            switch (id)
            {
                case "IsTimelinePlaying":
                    return (ref XContext ctx) =>
                    {
                        var r = GetRunner(ctx.Caster);
                        return r != null && r.IsPlaying();
                    };
                default: return null;
            }
        }

        public IRTNode<XContext> ResolveCustom(string id, object ud, IBTTracer tracer)
        {
            var nud = ud as NodeUserData;
            switch (id)
            {
                case "WaitSignalRT":
                {
                    var cfg = nud?.Payload as WaitSignalConfig;
                    string name = cfg ? cfg.SignalName : null;
                    int hash = cfg ? cfg.SignalHash : 0;
                    string tlid = cfg ? cfg.TimelineId : null;
                    float timeout = cfg ? cfg.Timeout : 0f;

                    return new WaitSignalRT<XContext>(
                        nud?.RuntimeName ?? "WaitSignal",
                        name, hash, tlid, timeout,
                        getBus: (XContext c) => c.EventBus as EventBus,
                        getDelta: (XContext c) => (float)(c.Clock?.DeltaTime ?? (1.0 / 60.0)),
                        tracer);
                }
                default: return null;
            }
        }
    }
}