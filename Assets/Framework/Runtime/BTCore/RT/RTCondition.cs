namespace XSkillSystem
{
    public sealed class RTCondition<TCtx> : RTNodeBase<TCtx>
    {
        public delegate bool Pred(ref TCtx ctx);

        readonly Pred _pred;

        public RTCondition(string name, Pred pred, IBTTracer tracer = null) : base(name, tracer)
        {
            _pred = pred;
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            var ok = _pred(ref ctx);
            var s = ok ? BTStatus.Success : BTStatus.Failure;
            Exit(s);
            return s;
        }
    }
}