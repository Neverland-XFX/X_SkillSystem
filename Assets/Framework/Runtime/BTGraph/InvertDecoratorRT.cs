using System;

namespace XSkillSystem
{
    // 反转：Success <-> Failure, Running 原样
    internal sealed class InvertDecoratorRT<TCtx> : RTDecorator<TCtx>
    {
        public InvertDecoratorRT(string name, IRTNode<TCtx> child, IBTTracer tracer) : base(name, child, tracer)
        {
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            var s = Child.Tick(ref ctx, rng);
            var r = s switch
            {
                BTStatus.Success => BTStatus.Failure,
                BTStatus.Failure => BTStatus.Success,
                _ => BTStatus.Running
            };
            Exit(r);
            return r;
        }
    }
}