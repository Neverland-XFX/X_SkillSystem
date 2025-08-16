namespace XSkillSystem
{
    public sealed class RTSucceeder<TCtx> : RTDecorator<TCtx>
    {
        public RTSucceeder(string name, IRTNode<TCtx> child, IBTTracer tracer) : base(name, child, tracer)
        {
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (Child == null)
            {
                Exit(BTStatus.Success);
                return BTStatus.Success;
            }

            var s = Child.Tick(ref ctx, rng);
            var r = (s == BTStatus.Running) ? BTStatus.Running : BTStatus.Success;
            Exit(r);
            return r;
        }
    }
}