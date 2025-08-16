namespace XSkillSystem
{
    public sealed class RTFailure<TCtx> : RTDecorator<TCtx>
    {
        public RTFailure(string name, IRTNode<TCtx> child, IBTTracer tracer) : base(name, child, tracer)
        {
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (Child == null)
            {
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            var s = Child.Tick(ref ctx, rng);
            var r = (s == BTStatus.Running) ? BTStatus.Running : BTStatus.Failure;
            Exit(r);
            return r;
        }
    }
}