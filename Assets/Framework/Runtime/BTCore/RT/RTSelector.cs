using System;
using System.Collections.Generic;

namespace XSkillSystem
{
    public sealed class RTSelector<TCtx> : RTNodeBase<TCtx>
    {
        readonly List<IRTNode<TCtx>> _children;
        int _index;

        public RTSelector(string name, List<IRTNode<TCtx>> children, IBTTracer tracer = null) : base(name, tracer)
        {
            _children = children ?? throw new ArgumentNullException(nameof(children));
            _index = 0;
        }

        public override void Reset(ref TCtx ctx)
        {
            _index = 0;
            foreach (var c in _children) c.Reset(ref ctx);
        }

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            foreach (var c in _children) c.Abort(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            while (_index < _children.Count)
            {
                var s = _children[_index].Tick(ref ctx, rng);
                if (s == BTStatus.Running)
                {
                    Exit(BTStatus.Running);
                    return BTStatus.Running;
                }

                if (s == BTStatus.Success)
                {
                    Exit(BTStatus.Success);
                    _index = 0;
                    return BTStatus.Success;
                }

                _index++;
            }

            Exit(BTStatus.Failure);
            _index = 0;
            return BTStatus.Failure;
        }
    }
}