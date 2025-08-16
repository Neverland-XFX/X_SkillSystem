namespace XSkillSystem
{
    /// <summary>
    /// 编辑器叶子节点通过字符串 id/参数来映射到运行时具体实现。
    /// </summary>
    public interface INodeLibrary<TCtx>
    {
        // 将 ActionId/ConditionId 解析成 BTCore 的委托
        RTAction<TCtx>.Func ResolveAction(string actionId, object userData);
        RTCondition<TCtx>.Pred ResolveCondition(string condId, object userData);

        // 可选解析自定义运行时节点（比如特殊并行/服务节点）
        IRTNode<TCtx> ResolveCustom(string customId, object userData, IBTTracer tracer);
    }
}