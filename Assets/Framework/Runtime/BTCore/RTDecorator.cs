namespace XSkillSystem
{
    // ----------------------- Decorator 基类 -----------------------
    public abstract class RTDecorator<TCtx> : RTNodeBase<TCtx>
    {
        protected IRTNode<TCtx> Child;

        protected RTDecorator(string name, IRTNode<TCtx> child, IBTTracer tracer = null) : base(name, tracer)
        {
            Child = child;
        }

        public override void Reset(ref TCtx ctx) => Child?.Reset(ref ctx);

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            Child?.Abort(ref ctx);
        }
    }
}