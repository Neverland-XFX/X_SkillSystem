using System;

namespace XSkillSystem
{
    public sealed class ParallelRT<TCtx> : RTNodeBase<TCtx>
    {
        private readonly IRTNode<TCtx>[] _children;
        private readonly ParallelPolicy _policy;
        private BTStatus[] _states;

        public ParallelRT(string name, IRTNode<TCtx>[] children, ParallelPolicy policy, IBTTracer tracer) : base(name,
            tracer)
        {
            _children = children ?? Array.Empty<IRTNode<TCtx>>();
            _policy = policy;
            _states = new BTStatus[_children.Length];
        }

        public override void Reset(ref TCtx ctx)
        {
            for (var i = 0; i < _children.Length; i++)
            {
                _children[i]?.Reset(ref ctx);
                _states[i] = BTStatus.Running;
            }
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (_children.Length == 0)
            {
                Exit(BTStatus.Success);
                return BTStatus.Success;
            }

            int succ = 0, fail = 0, run = 0;
            for (var i = 0; i < _children.Length; i++)
            {
                var c = _children[i];
                if (c == null)
                {
                    fail++;
                    _states[i] = BTStatus.Failure;
                    continue;
                }

                if (_states[i] == BTStatus.Running)
                {
                    var s = c.Tick(ref ctx, rng);
                    _states[i] = s;
                }

                switch (_states[i])
                {
                    case BTStatus.Success: succ++; break;
                    case BTStatus.Failure: fail++; break;
                    case BTStatus.Running: run++; break;
                }
            }

            if (_policy == ParallelPolicy.AnySuccess)
            {
                if (succ > 0)
                {
                    Exit(BTStatus.Success);
                    return BTStatus.Success;
                }

                if (run > 0)
                {
                    Exit(BTStatus.Running);
                    return BTStatus.Running;
                }

                Exit(BTStatus.Failure);
                return BTStatus.Failure; // 全部失败
            }
            else
            {
                if (fail > 0)
                {
                    Exit(BTStatus.Failure);
                    return BTStatus.Failure;
                }

                if (succ == _children.Length)
                {
                    Exit(BTStatus.Success);
                    return BTStatus.Success;
                }

                Exit(BTStatus.Running);
                return BTStatus.Running;
            }
        }

        public override void Abort(ref TCtx ctx)
        {
            foreach (var node in _children)
                node?.Abort(ref ctx);
        }
    }
}