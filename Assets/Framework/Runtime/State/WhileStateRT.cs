using System;

namespace XSkillSystem
{
    // 运行期间赋予状态（进入时添加，退出时移除，支持叠层）
    public sealed class WhileStateRT<TCtx> : RTDecorator<TCtx>
    {
        readonly StateId _state;
        readonly int _stacks;
        readonly Func<TCtx, StateMachine> _getSM;
        bool _applied;
        StateMachine _sm;

        public WhileStateRT(string name, IRTNode<TCtx> child, StateId state, int stacks,
            Func<TCtx, StateMachine> getSM,
            IBTTracer tracer)
            : base(name, child, tracer)
        {
            _state = state;
            _stacks = Math.Max(1, stacks);
            _getSM = getSM;
        }

        public override void Reset(ref TCtx ctx)
        {
            _applied = false;
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

            if (!_applied)
            {
                _sm.Apply(_state, duration: -1f, stacks: _stacks, respectImmunity: false);
                _applied = true;
            }

            var s = Child.Tick(ref ctx, rng);
            if (s != BTStatus.Running)
            {
                _sm.Remove(_state, allStacks: true);
                _applied = false;
            }

            Exit(s);
            return s;
        }

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            _sm ??= _getSM(ctx);
            if (_sm != null && _applied) _sm.Remove(_state, true);
            _applied = false;
        }
    }
}