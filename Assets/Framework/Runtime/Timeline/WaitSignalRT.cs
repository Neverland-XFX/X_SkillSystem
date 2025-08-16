using System;

namespace XSkillSystem
{
    public sealed class WaitSignalRT<TCtx> : RTNodeBase<TCtx>
    {
        readonly string _signalName;
        readonly int _signalHash;
        readonly string _timelineId; // 可选过滤
        readonly float _timeout;
        readonly Func<TCtx, EventBus> _getBus;
        readonly Func<TCtx, float> _getDelta;

        int _token;
        float _t;
        bool _hit;
        EventBus _bus;

        public WaitSignalRT(string name, string signalName, int signalHash, string timelineId, float timeout,
            Func<TCtx, EventBus> getBus, Func<TCtx, float> getDelta, IBTTracer tracer)
            : base(name, tracer)
        {
            _signalName = signalName ?? string.Empty;
            _signalHash = signalHash;
            _timelineId = timelineId; // 可为空
            _timeout = Math.Max(0f, timeout);
            _getBus = getBus;
            _getDelta = getDelta;
        }

        public override void Reset(ref TCtx ctx)
        {
            _t = 0f;
            _hit = false;
            if (_bus != null && _token != 0) _bus.Unsubscribe<EV_TL_Signal>(_token);
            _token = 0;
            _bus = null;
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (_bus == null)
            {
                _bus = _getBus(ctx);
                if (_bus == null)
                {
                    Exit(BTStatus.Failure);
                    return BTStatus.Failure;
                }

                _token = _bus.Subscribe<EV_TL_Signal>(OnSignal, Filter);
            }

            if (_hit)
            {
                Exit(BTStatus.Success);
                Reset(ref ctx);
                return BTStatus.Success;
            }

            if (_timeout > 0f)
            {
                _t += _getDelta(ctx);
                if (_t >= _timeout)
                {
                    Exit(BTStatus.Failure);
                    Reset(ref ctx);
                    return BTStatus.Failure;
                }
            }

            Exit(BTStatus.Running);
            return BTStatus.Running;
        }

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            if (_bus != null && _token != 0) _bus.Unsubscribe<EV_TL_Signal>(_token);
            _token = 0;
            _bus = null;
        }

        bool Filter(EV_TL_Signal e)
        {
            if (!string.IsNullOrEmpty(_timelineId) &&
                !string.Equals(e.TimelineId, _timelineId, StringComparison.Ordinal))
                return false;
            if (!string.IsNullOrEmpty(_signalName))
                return string.Equals(e.SignalName, _signalName, StringComparison.Ordinal);
            if (_signalHash != 0)
                return e.SignalHash == _signalHash;
            return true;
        }

        void OnSignal(EV_TL_Signal e)
        {
            _hit = true;
        }
    }
}