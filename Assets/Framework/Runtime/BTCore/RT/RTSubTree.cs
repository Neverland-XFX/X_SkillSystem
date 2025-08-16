namespace XSkillSystem
{
    // =================== 子树包装 ===================
    public sealed class RTSubTree<TCtx> : RTNodeBase<TCtx>
    {
        private readonly BTTree<TCtx> _tree;

        public RTSubTree(string name, BTTree<TCtx> subTree, IBTTracer tracer) : base(name, tracer)
        {
            _tree = subTree;
        }

        public override void Reset(ref TCtx ctx)
        {
            _tree?.Root?.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (_tree?.Root == null)
            {
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            var s = _tree.Root.Tick(ref ctx, rng);
            Exit(s);
            return s;
        }

        public override void Abort(ref TCtx ctx)
        {
            _tree?.Root?.Abort(ref ctx);
        }
    }
}