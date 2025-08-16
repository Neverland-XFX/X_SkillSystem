using UnityEngine.Playables;

namespace XSkillSystem
{
    public sealed class RTTL_Play<TCtx> : RTNodeBase<TCtx>
    {
        readonly PlayTimelineConfig _cfg;
        bool _played;

        public RTTL_Play(string name, PlayTimelineConfig cfg, IBTTracer tracer = null) : base(name, tracer)
        {
            _cfg = cfg;
        }

        public override void Reset(ref TCtx ctx)
        {
            _played = false;
        }

        public override void Abort(ref TCtx ctx)
        {
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            if (_played) return BTStatus.Success;

            Enter();

            if (ctx is XContext x)
            {
                var def = _cfg?.Def;
                var start = _cfg != null ? (double)_cfg.StartTime : 0.0;
                var speed = _cfg != null ? (double)_cfg.Speed : 1.0;

                if (x.Caster != null)
                {
                    var tr = x.Caster.GetComponent<TimelineRunner>();
                    if (tr != null)
                    {
                        // Play(def, overrideBindings:null, startTime, timeScale)
                        tr.Play(def, null, start, speed);
                        Tracer?.Enter($"[{Name}] TimelineRunner.Play def={def?.name} start={start} speed={speed}");
                        _played = true;
                        Exit(BTStatus.Success);
                        return BTStatus.Success;
                    }

                    // fallback to PlayableDirector if TimelineRunner not present
                    var director = x.Caster.GetComponent<PlayableDirector>();
                    if (director != null && def?.Asset != null)
                    {
                        director.playableAsset = def.Asset;
                        director.time = start;
                        director.Play();
                        // timeScale: PlayableDirector requires setting speed via director.playableGraph or director.timeScale (varies). Try SetSpeed via TimelineRunner abstraction is preferred.
                        Tracer?.Enter($"[{Name}] PlayableDirector.Play {def?.name} start={start}");
                        _played = true;
                        Exit(BTStatus.Success);
                        return BTStatus.Success;
                    }
                }

                Tracer?.Enter($"[{Name}] 无 caster 或 无 TimelineRunner/PlayableDirector，无法播放 Timeline.");
            }
            else
            {
                Tracer?.Enter($"[{Name}] context 非 XContext，无法播放 Timeline.");
            }

            _played = true; // 仍标记为已尝试播放，避免无限重试
            Exit(BTStatus.Failure);
            return BTStatus.Failure;
        }
    }
}