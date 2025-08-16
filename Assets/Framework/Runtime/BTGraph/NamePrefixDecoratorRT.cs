namespace XSkillSystem
{
    // 给子树节点统一加前缀，方便诊断树状路径
    internal sealed class NamePrefixDecoratorRT<TCtx> : RTDecorator<TCtx>
    {
        readonly string _prefix;

        public NamePrefixDecoratorRT(string name, IRTNode<TCtx> child, string prefix, IBTTracer tracer) : base(name,
            child, tracer)
        {
            _prefix = prefix ?? string.Empty;
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng) => Child.Tick(ref ctx, rng);
    }
}