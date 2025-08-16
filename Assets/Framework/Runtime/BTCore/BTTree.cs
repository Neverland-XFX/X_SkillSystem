using System;

namespace XSkillSystem
{
    /// <summary>
    /// 行为树实例（持有根节点与诊断/配置）。
    /// </summary>
    public sealed class BTTree<TCtx>
    {
        public readonly IRTNode<TCtx> Root;
        public readonly IBTTracer Tracer;
        public int MaxNodePerTick = 1024; // 防止无限循环

        public BTTree(IRTNode<TCtx> root, IBTTracer tracer = null)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            Tracer = tracer ?? NullTracer.Instance;
        }
    }
}