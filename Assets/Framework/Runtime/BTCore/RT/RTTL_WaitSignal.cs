using System;

namespace XSkillSystem
{
    // TL_WaitSignalRT: 订阅 EV_TL_Signal，等待指定信号或超时
    public sealed class RTTL_WaitSignal<TCtx> : RTNodeBase<TCtx>
    {
        readonly WaitSignalConfig _cfg;
        bool _listening;
        bool _triggered;
        double _elapsed;
        IDisposable _subDisposable;
        int _subHandle = -1;

        public RTTL_WaitSignal(string name, WaitSignalConfig cfg, IBTTracer tracer = null) : base(name, tracer)
        {
            _cfg = cfg;
        }

        public override void Reset(ref TCtx ctx)
        {
            _listening = false;
            _triggered = false;
            _elapsed = 0.0;
            if (_subDisposable != null)
            {
                _subDisposable.Dispose();
                _subDisposable = null;
            }

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

                    bus.Unsubscribe<EV_TL_Signal>(_subHandle);
                    _subHandle = bus.Subscribe<EV_TL_Signal>(OnEV_TL_SignalHandle);
                    // 兼容性订阅：EventBusUtil 会返回 IDisposable 或 null（若不可退订）
                    // _subDisposable = EventBusUtil.SubscribeTo<EV_TL_Signal>(bus, ev =>
                    // {
                    //     if (!string.IsNullOrEmpty(_cfg.TimelineId) && ev.TimelineId != _cfg.TimelineId) return;
                    //     if (string.Equals(ev.SignalName, _cfg.SignalName, StringComparison.Ordinal))
                    //     {
                    //         _triggered = true;
                    //         Tracer?.Enter($"[{Name}] 收到信号 {ev.SignalName} (Timeline={ev.TimelineId})");
                    //     }
                    // }, out _subHandle);
                    Tracer?.Enter($"[{Name}] 订阅 EV_TL_Signal (handle={_subHandle}) 等待 '{_cfg.SignalName}'");
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

        private void OnEV_TL_SignalHandle(EV_TL_Signal ev)
        {
            if (!string.IsNullOrEmpty(_cfg.TimelineId) && ev.TimelineId != _cfg.TimelineId) return;
            if (string.Equals(ev.SignalName, _cfg.SignalName, StringComparison.Ordinal))
            {
                _triggered = true;
                Tracer?.Enter($"[{Name}] 收到信号 {ev.SignalName} (Timeline={ev.TimelineId})");
            }
        }
    }
}