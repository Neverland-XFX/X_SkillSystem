namespace XSkillSystem
{
    /// <summary>
    /// 简单占位库：方便测试 通图-编译-执行链路。
    /// </summary>
    public sealed class PlaceholderNodeLibrary<TCtx> : INodeLibrary<TCtx>
    {
        public RTAction<TCtx>.Func ResolveAction(string actionId, object userData)
            => (ref TCtx ctx, IBTRandom rng) => BTStatus.Success; // 默认立即成功

        public RTCondition<TCtx>.Pred ResolveCondition(string condId, object userData)
            => (ref TCtx ctx) => true; // 默认条件恒为真

        public IRTNode<TCtx> ResolveCustom(string customId, object userData, IBTTracer tracer)
            => null; // 未实现则返回 null
    }
}