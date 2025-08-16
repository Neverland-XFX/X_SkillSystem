using UnityEngine.Playables;

namespace XSkillSystem
{
    // TL_StopRT: 停止播放
    public sealed class RTTL_Stop<TCtx> : RTNodeBase<TCtx>
    {
        readonly StopTimelineConfig _cfg;
        bool _done;

        public RTTL_Stop(string name, StopTimelineConfig cfg, IBTTracer tracer = null) : base(name, tracer)
        {
            _cfg = cfg;
        }

        public override void Reset(ref TCtx ctx)
        {
            _done = false;
        }

        public override void Abort(ref TCtx ctx)
        {
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            if (_done) return BTStatus.Success;
            Enter();

            if (ctx is XContext x && x.Caster != null)
            {
                var tr = x.Caster.GetComponent<TimelineRunner>();
                if (tr != null)
                {
                    tr.Stop();
                    Tracer?.Enter($"[{Name}] TimelineRunner.Stop");
                    _done = true;
                    Exit(BTStatus.Success);
                    return BTStatus.Success;
                }

                var director = x.Caster.GetComponent<PlayableDirector>();
                if (director != null)
                {
                    director.Stop();
                    Tracer?.Enter($"[{Name}] PlayableDirector.Stop");
                    _done = true;
                    Exit(BTStatus.Success);
                    return BTStatus.Success;
                }
            }

            Tracer?.Enter($"[{Name}] 无 caster 或 无 TimelineRunner/PlayableDirector，Stop 操作跳过");
            _done = true;
            Exit(BTStatus.Success);
            return BTStatus.Success;
        }
    }
}