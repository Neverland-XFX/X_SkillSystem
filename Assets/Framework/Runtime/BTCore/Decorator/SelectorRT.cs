using System;

namespace XSkillSystem
{
    public sealed class SelectorRT<TCtx> : RTNodeBase<TCtx>
    {
        private readonly IRTNode<TCtx>[] _children;
        private int _i;

        public SelectorRT(string name, IRTNode<TCtx>[] children, IBTTracer tracer) : base(name, tracer)
        {
            _children = children ?? Array.Empty<IRTNode<TCtx>>();
        }

        public override void Reset(ref TCtx ctx)
        {
            _i = 0;
            foreach (var node in _children)
                node?.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            while (_i < _children.Length)
            {
                var c = _children[_i];
                var s = c.Tick(ref ctx, rng);
                if (s == BTStatus.Running) { Exit(BTStatus.Running); return BTStatus.Running; }
                if (s == BTStatus.Success) { Exit(BTStatus.Success); return BTStatus.Success; }
                _i++; // failure -> try next
            }
            Exit(BTStatus.Failure); return BTStatus.Failure;
        }
    }
}