using System;

namespace XSkillSystem
{
    // 重复 ：遇到 Failure 立即失败
    internal sealed class RepeatDecoratorRT<TCtx> : RTDecorator<TCtx>
    {
        readonly int _count;
        int _done;

        public RepeatDecoratorRT(string name, IRTNode<TCtx> child, int count, IBTTracer tracer) : base(name, child,
            tracer)
        {
            _count = Math.Max(1, count);
        }

        public override void Reset(ref TCtx ctx)
        {
            _done = 0;
            base.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            while (_done < _count)
            {
                var s = Child.Tick(ref ctx, rng);
                if (s == BTStatus.Running)
                {
                    Exit(BTStatus.Running);
                    return BTStatus.Running;
                }

                if (s == BTStatus.Failure)
                {
                    Exit(BTStatus.Failure);
                    _done = 0;
                    return BTStatus.Failure;
                }

                _done++;
                Child.Reset(ref ctx);
            }

            Exit(BTStatus.Success);
            _done = 0;
            return BTStatus.Success;
        }
    }
}