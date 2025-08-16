namespace XSkillSystem
{
    // =================== 装饰器 ===================
    public sealed class RTInverter<TCtx> : RTDecorator<TCtx>
    {
        public RTInverter(string name, IRTNode<TCtx> child, IBTTracer tracer) : base(name, child, tracer)
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