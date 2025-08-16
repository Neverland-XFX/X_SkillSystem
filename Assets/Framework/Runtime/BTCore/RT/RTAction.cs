namespace XSkillSystem
{
    // ----------- Action / Condition（轻量委托版，占位后续替换） -----------
    public sealed class RTAction<TCtx> : RTNodeBase<TCtx>
    {
        public delegate BTStatus Func(ref TCtx ctx, IBTRandom rng);

        readonly Func _fn;

        public RTAction(string name, Func fn, IBTTracer tracer = null) : base(name, tracer)
        {
            _fn = fn;
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            var s = _fn(ref ctx, rng);
            Exit(s);
            return s;
        }
    }
}