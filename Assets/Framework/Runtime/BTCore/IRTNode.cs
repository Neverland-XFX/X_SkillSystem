namespace XSkillSystem
{
    /// <summary>
    /// 所有运行时节点的统一接口。
    /// </summary>
    public interface IRTNode<TCtx>
    {
        string Name { get; }
        BTStatus Tick(ref TCtx ctx, IBTRandom rng);
        void Reset(ref TCtx ctx);
        void Abort(ref TCtx ctx); // 接收 Hard-Stop 的传播
    }
}