using System;

namespace XSkillSystem
{
    // 读条：在 Duration 内运行子树（通常为空或并行动作），遇到打断则失败
    public sealed class CastTimeRT<TCtx> : RTDecorator<TCtx>
    {
        readonly float _duration;
        readonly Channel _channel;
        readonly int _priority;
        readonly StateId _interruptStates;
        readonly Func<TCtx, StateMachine> _getSM;
        readonly Func<TCtx, float> _getDelta;
        float _t;
        bool _interrupted;
        int _handle;
        int _tokInterrupt;
        StateMachine _sm;

        public CastTimeRT(string name, IRTNode<TCtx> child, float duration, Channel channel, int priority,
            StateId interruptStates,
            Func<TCtx, StateMachine> getSM, Func<TCtx, float> getDelta, IBTTracer tracer)
            : base(name, child, tracer)
        {
            _duration = Math.Max(0f, duration);
            _channel = channel;
            _priority = priority;
            _interruptStates = interruptStates;
            _getSM = getSM;
            _getDelta = getDelta;
        }

        public override void Reset(ref TCtx ctx)
        {
            _t = 0f;
            _interrupted = false;
            _handle = 0;
            if (_tokInterrupt != 0)
            {
                _sm?.Bus.Unsubscribe<EV_Interrupt>(_tokInterrupt);
                _tokInterrupt = 0;
            }

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
                _handle = _sm.AcquireChannel(_channel, _priority, timeout: _duration, ownerName: Name);
                if (_handle == 0)
                {
                    Exit(BTStatus.Failure);
                    return BTStatus.Failure;
                }

                // 监听打断
                _tokInterrupt = _sm.Bus.Subscribe<EV_Interrupt>(e =>
                {
                    if (e.Owner == _sm.gameObject && (e.Channel == _channel || (e.State & _interruptStates) != 0))
                        _interrupted = true;
                });
            }

            // 每帧检查状态打断
            if ((_sm.ImmunityMask & StateId.Unstoppable) == 0 && _interruptStates != StateId.None)
            {
                foreach (StateId s in Enum.GetValues(typeof(StateId)))
                {
                    if (s == StateId.None || s == StateId.All) continue;
                    if ((_interruptStates & s) != 0 && _sm.Has(s))
                    {
                        _interrupted = true;
                        break;
                    }
                }
            }

            if (_interrupted)
            {
                _sm.ReleaseChannel(_channel, _handle, InterruptReason.StateApplied);
                Cleanup();
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            _t += _getDelta(ctx);
            if (_t >= _duration)
            {
                _sm.ReleaseChannel(_channel, _handle);
                Cleanup();
                Exit(BTStatus.Success);
                return BTStatus.Success;
            }

            var btStatus = Child.Tick(ref ctx, rng); // 可在读条期间并行动画/特效
            Exit(BTStatus.Running);
            return BTStatus.Running;
        }

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            _sm ??= _getSM(ctx);
            if (_sm != null && _handle != 0) _sm.ReleaseChannel(_channel, _handle, InterruptReason.ExternalCancel);
            Cleanup();
        }

        void Cleanup()
        {
            if (_tokInterrupt != 0)
            {
                _sm?.Bus.Unsubscribe<EV_Interrupt>(_tokInterrupt);
                _tokInterrupt = 0;
            }

            _handle = 0;
            _t = 0f;
            _interrupted = false;
        }
    }
}