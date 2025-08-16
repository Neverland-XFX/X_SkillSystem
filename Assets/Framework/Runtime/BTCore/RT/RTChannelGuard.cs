using System;
using UnityEngine;
using UnityEngine.Playables;

namespace XSkillSystem
{
    // 占用/守护通道：进入时 Acquire，退出/中断时 Release；若被抢占将使子树失败
    public sealed class RTChannelGuard<TCtx> : RTDecorator<TCtx>
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

        public RTChannelGuard(string name, IRTNode<TCtx> child, Channel ch, int priority, float timeout,
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
}