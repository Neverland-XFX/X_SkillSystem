using UnityEngine;

namespace XSkillSystem
{
    public sealed class RTRepeat<TCtx> : RTDecorator<TCtx>
    {
        readonly int _count;
        readonly bool _breakOnFailure;
        int _done;

        public RTRepeat(string name, IRTNode<TCtx> child, int count, bool breakOnFailure, IBTTracer tracer)
            : base(name, child, tracer)
        {
            _count = Mathf.Max(1, count);
            _breakOnFailure = breakOnFailure;
        }

        public override void Reset(ref TCtx ctx)
        {
            _done = 0;
            base.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (Child == null)
            {
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            while (_done < _count)
            {
                var s = Child.Tick(ref ctx, rng);
                switch (s)
                {
                    case BTStatus.Running:
                        Exit(BTStatus.Running);
                        return BTStatus.Running;
                    case BTStatus.Failure when _breakOnFailure:
                        Exit(BTStatus.Failure);
                        return BTStatus.Failure;
                }

                _done++;
                if (_done < _count) Child.Reset(ref ctx); // 下次循环继续
            }

            Exit(BTStatus.Success);
            return BTStatus.Success;
        }

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            _done = 0;
        }
    }
}