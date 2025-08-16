using System;
using UnityEngine;
using UnityEngine.Playables;

namespace XSkillSystem
{
    // 占用/守护通道：进入时 Acquire，退出/中断时 Release；若被抢占将使子树失败
    public sealed class ChannelGuardRT<TCtx> : RTDecorator<TCtx>
    {
        readonly Channel _ch;
        readonly int _priority;
        readonly float _timeout;
        int _handle;
        int _tokPreempt;
        StateMachine _sm;
        EventBus _subBus; //记录实际用于订阅的总线
        Func<TCtx, StateMachine> _getSM;
        EventBus _bus;

        public ChannelGuardRT(string name, IRTNode<TCtx> child, Channel ch, int priority, float timeout,
            Func<TCtx, StateMachine> getSM, EventBus bus, IBTTracer tracer)
            : base(name, child, tracer)
        {
            _ch = ch;
            _priority = priority;
            _timeout = timeout;
            _getSM = getSM;
            _bus = bus;
        }

        public override void Reset(ref TCtx ctx)
        {
            _handle = 0;
            if (_tokPreempt != 0 && _subBus != null)
            {
                _subBus.Unsubscribe<EV_ChannelPreempt>(_tokPreempt);
            }

            _tokPreempt = 0;
            _subBus = null;
            base.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            _sm ??= _getSM(ctx);
            if (_sm == null)
            {
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            if (_handle == 0)
            {
                _handle = _sm.AcquireChannel(_ch, _priority, _timeout, Name);
                if (_handle == 0)
                {
                    Exit(BTStatus.Failure);
                    return BTStatus.Failure;
                }

                // 订阅：用 StateMachine 的 Bus，并记录
                _subBus = _sm.Bus;
                if (_subBus != null)
                {
                    _tokPreempt = _subBus.Subscribe<EV_ChannelPreempt>(e =>
                    {
                        if (e.Owner == _sm.gameObject && e.Channel == _ch) _handle = 0;
                    });
                }
            }

            Exit(BTStatus.Success);
            if (Child == null) return BTStatus.Success;
            
            var s = Child.Tick(ref ctx, rng);
            if (s != BTStatus.Running)
            {
                _sm.ReleaseChannel(_ch, _handle);
                if (_tokPreempt != 0 && _subBus != null) _subBus.Unsubscribe<EV_ChannelPreempt>(_tokPreempt);
                _tokPreempt = 0;
                _subBus = null;
                _handle = 0;
            }

            Exit(s);
            return s;
        }

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            _sm ??= _getSM(ctx);
            if (_sm != null && _handle != 0) _sm.ReleaseChannel(_ch, _handle, InterruptReason.ExternalCancel);
            if (_tokPreempt != 0 && _subBus != null) _subBus.Unsubscribe<EV_ChannelPreempt>(_tokPreempt);
            _tokPreempt = 0;
            _subBus = null;
            _handle = 0;
        }
    }
    
    
        public sealed class TL_PlayRT<TCtx> : RTNodeBase<TCtx>
    {
        readonly PlayTimelineConfig _cfg;
        bool _played;

        public TL_PlayRT(string name, PlayTimelineConfig cfg, IBTTracer tracer = null) : base(name, tracer)
        {
            _cfg = cfg;
        }

        public override void Reset(ref TCtx ctx)
        {
            _played = false;
        }

        public override void Abort(ref TCtx ctx) { }

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

    // TL_StopRT: 停止播放
    public sealed class TL_StopRT<TCtx> : RTNodeBase<TCtx>
    {
        readonly StopTimelineConfig _cfg;
        bool _done;

        public TL_StopRT(string name, StopTimelineConfig cfg, IBTTracer tracer = null) : base(name, tracer)
        {
            _cfg = cfg;
        }

        public override void Reset(ref TCtx ctx) { _done = false; }
        public override void Abort(ref TCtx ctx) { }

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

    // TL_WaitSignalRT: 订阅 EV_TL_Signal，等待指定信号或超时
    public sealed class TL_WaitSignalRT<TCtx> : RTNodeBase<TCtx>
    {
        readonly WaitSignalConfig _cfg;
        bool _listening;
        bool _triggered;
        double _elapsed;
        IDisposable _subDisposable;
        int _subHandle = -1;

        public TL_WaitSignalRT(string name, WaitSignalConfig cfg, IBTTracer tracer = null) : base(name, tracer)
        {
            _cfg = cfg;
        }

        public override void Reset(ref TCtx ctx)
        {
            _listening = false;
            _triggered = false;
            _elapsed = 0.0;
            if (_subDisposable != null) { _subDisposable.Dispose(); _subDisposable = null; }
            _subHandle = -1;
        }

        public override void Abort(ref TCtx ctx)
        {
            Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            if (_triggered)
            {
                Exit(BTStatus.Success);
                return BTStatus.Success;
            }

            if (!_listening)
            {
                Enter();
                _listening = true;
                if (ctx is XContext x)
                {
                    var bus = x.EventBus as EventBus;
                    if (bus == null)
                    {
                        Tracer?.Enter($"[{Name}] 无 EventBus");
                        return BTStatus.Failure;
                    }

                    // 兼容性订阅：EventBusUtil 会返回 IDisposable 或 null（若不可退订）
                    _subDisposable = EventBusUtil.SubscribeTo<EV_TL_Signal>(bus, ev =>
                    {
                        if (!string.IsNullOrEmpty(_cfg.TimelineId) && ev.TimelineId != _cfg.TimelineId) return;
                        if (string.Equals(ev.SignalName, _cfg.SignalName, StringComparison.Ordinal))
                        {
                            _triggered = true;
                            Tracer?.Enter($"[{Name}] 收到信号 {ev.SignalName} (Timeline={ev.TimelineId})");
                        }
                    }, out _subHandle);
                    Tracer?.Enter($"[{Name}] 订阅 EV_TL_Signal (handle={_subHandle}) 等待 '{_cfg.SignalName}'");
                    if (_subDisposable == null)
                    {
                        _triggered = true;
                    }
                }
            }

            // 超时逻辑
            if (_cfg != null && _cfg.Timeout > 0.0f)
            {
                _elapsed += RTNodeBase<TCtx>.GetDeltaTime(ref ctx);
                if (_elapsed >= _cfg.Timeout)
                {
                    Tracer?.Enter($"[{Name}] 等待信号超时 {_cfg.Timeout}s");
                    _subDisposable?.Dispose();
                    _subDisposable = null;
                    Exit(BTStatus.Failure);
                    return BTStatus.Failure;
                }
            }

            return BTStatus.Running;
        }
    }
}