using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XSkillSystem
{
    public static class BTCompiler
    {
        // ====== 公开：标准 Build ====== //
        public static BTTree<TCtx> Build<TCtx>(BTGraph graph, INodeLibrary<TCtx> lib, IBTTracer tracer)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (lib == null) throw new ArgumentNullException(nameof(lib));

            var root = graph.Root;
            if (root == null) throw new Exception($"Graph `{graph.name}` 没有 Root 节点。");

            var ctx = new BuildCtx<TCtx>(graph, lib, tracer);
            var rootRT = CompileNode(ctx, root);
            if (rootRT == null) throw new Exception("编译 Root 失败。");

            return new BTTree<TCtx>(rootRT, tracer ?? new ConsoleTracer());
        }

        static NodePort FirstValidConnection(NodePort port)
        {
            if (port == null) return null;
            // 逐个检查，跳过为 null 的连接
            for (int i = 0; i < port.ConnectionCount; i++)
            {
                var c = port.GetConnection(i);
                if (c != null) return c;
            }

            return null;
        }

        // ====== 公开：带报告的 Build ====== //
        public static BTCompileReport BuildWithReport<TCtx>(BTGraph graph, INodeLibrary<TCtx> lib, IBTTracer tracer)
        {
            var report = new BTCompileReport();
            var issues = new List<string>();
            var ok = ValidateGraph(graph, issues);
            if (!ok)
            {
                report.Success = false;
                report.Errors.AddRange(issues);
                return report;
            }

            try
            {
                _ = Build<TCtx>(graph, lib, tracer);
                report.Success = true;
            }
            catch (Exception ex)
            {
                report.Success = false;
                report.Errors.Add(ex.Message);
            }

            return report;
        }

        // ====== 公开：图校验（Odin 按钮会调用） ====== //
        public static bool ValidateGraph(BTGraph g, List<string> outIssues)
        {
            bool ok = true;
            if (g == null)
            {
                outIssues.Add("Graph 为 null");
                return false;
            }

            if (!g.Root)
            {
                outIssues.Add("- 缺少 Root 节点");
                ok = false;
            }

            foreach (var node in g.AllNodes())
            {
                switch (node)
                {
                    case RootNode r:
                        if (!IsConnected(r, nameof(RootNode.Child)))
                        {
                            outIssues.Add($"- {r.RuntimeName}: Root 未连接子节点");
                            ok = false;
                        }

                        break;
                    case SequenceNode s:
                        if (!HasAny(s, nameof(SequenceNode.Children)))
                        {
                            outIssues.Add($"- {s.RuntimeName}: Sequence 没有子节点");
                            ok = false;
                        }

                        break;
                    case SelectorNode sel:
                        if (!HasAny(sel, nameof(SelectorNode.Children)))
                        {
                            outIssues.Add($"- {sel.RuntimeName}: Selector 没有子节点");
                            ok = false;
                        }

                        break;
                    case ParallelNode p:
                        if (!HasAny(p, nameof(ParallelNode.Children)))
                        {
                            outIssues.Add($"- {p.RuntimeName}: Parallel 没有子节点");
                            ok = false;
                        }

                        break;
                    case ActionNode a:
                        if (string.IsNullOrEmpty(a.ActionId))
                        {
                            outIssues.Add($"- {a.RuntimeName}: ActionId 为空");
                            ok = false;
                        }

                        break;
                    case ConditionNode c:
                        if (string.IsNullOrEmpty(c.ConditionId))
                        {
                            outIssues.Add($"- {c.RuntimeName}: ConditionId 为空");
                            ok = false;
                        }

                        break;
                    case SubTreeNode st:
                        if (st.Graph == null)
                        {
                            outIssues.Add($"- {st.RuntimeName}: SubTree 未指定 Graph");
                            ok = false;
                        }

                        break;
                }
            }

            return ok;

            static bool IsConnected(BTNodeBase n, string port)
            {
                var p = n.GetOutputPort(port);
                return p != null && p.IsConnected;
            }

            static bool HasAny(BTNodeBase n, string port)
            {
                var p = n.GetOutputPort(port);
                return p != null && p.ConnectionCount > 0;
            }
        }

        // ====== 编译实现 ====== //
        sealed class BuildCtx<TCtx>
        {
            public readonly BTGraph Graph;
            public readonly INodeLibrary<TCtx> Lib;
            public readonly IBTTracer Tracer;
            public readonly Dictionary<BTNodeBase, IRTNode<TCtx>> Cache = new();

            public BuildCtx(BTGraph g, INodeLibrary<TCtx> lib, IBTTracer tracer)
            {
                Graph = g;
                Lib = lib;
                Tracer = tracer ?? new ConsoleTracer();
            }
        }

        static IRTNode<TCtx> CompileNode<TCtx>(BuildCtx<TCtx> ctx, BTNodeBase node)
        {
            if (node == null) return null;
            if (ctx.Cache.TryGetValue(node, out var hit)) return hit;

            IRTNode<TCtx> rt = node switch
            {
                RootNode n => CompileRoot(ctx, n),
                SequenceNode n => CompileSequence(ctx, n),
                SelectorNode n => CompileSelector(ctx, n),
                ParallelNode n => CompileParallel(ctx, n),

                InverterNode n => new InverterRT<TCtx>(n.RuntimeName,
                    GetSingleChild(ctx, n, nameof(InverterNode.Child)), ctx.Tracer),
                SucceederNode n => new SucceederRT<TCtx>(n.RuntimeName,
                    GetSingleChild(ctx, n, nameof(SucceederNode.Child)), ctx.Tracer),
                FailureNode n => new FailureRT<TCtx>(n.RuntimeName, GetSingleChild(ctx, n, nameof(FailureNode.Child)),
                    ctx.Tracer),
                TimeoutNode n => new TimeoutDecoratorRT<TCtx>(n.RuntimeName,
                    GetSingleChild(ctx, n, nameof(TimeoutNode.Child)), n.Seconds, ctx.Tracer),
                RepeatNode n => new RepeatRT<TCtx>(n.RuntimeName, GetSingleChild(ctx, n, nameof(RepeatNode.Child)),
                    n.Count, n.BreakOnFailure, ctx.Tracer),
                WaitNode n => new WaitRT<TCtx>(n.RuntimeName, n.Seconds, ctx.Tracer),

                ActionNode n => CompileAction(ctx, n),
                ConditionNode n => CompileCondition(ctx, n),

                RandomSelectorNode n => CompileRandomSelector(ctx, n),
                SubTreeNode n => CompileSubTree(ctx, n),
                ForEachTargetsNode n => CompileForEach(ctx, n),

                _ => TryResolveCustom(ctx, node)
            };

            if (rt == null) throw new Exception($"不支持的节点或解析失败：{node.GetType().Name} ({node.RuntimeName})");
            ctx.Cache[node] = rt;
            return rt;
        }

        static IRTNode<TCtx> CompileRoot<TCtx>(BuildCtx<TCtx> ctx, RootNode n)
        {
            var child = GetSingleChild(ctx, n, nameof(RootNode.Child));
            if (child == null) throw new Exception("Root 必须连接一个子节点。");
            return child;
        }

        static IRTNode<TCtx> CompileSequence<TCtx>(BuildCtx<TCtx> ctx, SequenceNode n)
        {
            var arr = GetListChildren(ctx, n, nameof(SequenceNode.Children));
            return new SequenceRT<TCtx>(n.RuntimeName, arr, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileSelector<TCtx>(BuildCtx<TCtx> ctx, SelectorNode n)
        {
            var arr = GetListChildren(ctx, n, nameof(SelectorNode.Children));
            return new SelectorRT<TCtx>(n.RuntimeName, arr, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileParallel<TCtx>(BuildCtx<TCtx> ctx, ParallelNode n)
        {
            var arr = GetListChildren(ctx, n, nameof(ParallelNode.Children));

            // 映射到运行时策略，并处理阈值
            ParallelPolicy policy;
            // int threshold = 0;

            switch (n.ModePolicy)
            {
                case ParallelNode.Mode.AllSuccess:
                    policy = ParallelPolicy.AllSuccess;
                    break;
                case ParallelNode.Mode.Threshold:
                    policy = ParallelPolicy.Threshold;
                    // threshold = Mathf.Clamp(n.Threshold, 1, Mathf.Max(1, arr.Length));
                    break;
                default:
                    policy = ParallelPolicy.AnySuccess;
                    break;
            }

            return new ParallelRT<TCtx>(n.RuntimeName, arr, policy, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileRandomSelector<TCtx>(BuildCtx<TCtx> ctx, RandomSelectorNode n)
        {
            var arr = GetListChildren(ctx, n, nameof(RandomSelectorNode.Children));
            var w = n.Weights;
            return new RandomSelectorRT<TCtx>(n.RuntimeName, arr, w, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileSubTree<TCtx>(BuildCtx<TCtx> ctx, SubTreeNode n)
        {
            if (n.Graph == null) throw new Exception($"SubTree `{n.RuntimeName}` 未指定 Graph。");
            var sub = Build(n.Graph, ctx.Lib, n.InheritTracer ? ctx.Tracer : new ConsoleTracer());
            return new SubTreeRT<TCtx>(n.RuntimeName, sub, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileForEach<TCtx>(BuildCtx<TCtx> ctx, ForEachTargetsNode n)
        {
            var child = GetSingleChild(ctx, n, nameof(ForEachTargetsNode.Child));
            return new ForEachTargetsRT<TCtx>(n.RuntimeName, child, n.BreakOnFirstSuccess, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileAction<TCtx>(BuildCtx<TCtx> ctx, ActionNode n)
        {
            var fn = ctx.Lib.ResolveAction(n.ActionId, new NodeUserData(n.UserData, n.Guid, n.RuntimeName));
            if (fn == null) throw new Exception($"未能解析 ActionId: `{n.ActionId}` in {n.RuntimeName}");
            return new RTAction<TCtx>(n.RuntimeName, fn, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileCondition<TCtx>(BuildCtx<TCtx> ctx, ConditionNode n)
        {
            var pred = ctx.Lib.ResolveCondition(n.ConditionId, new NodeUserData(n.UserData, n.Guid, n.RuntimeName));
            if (pred == null) throw new Exception($"未能解析 ConditionId: `{n.ConditionId}` in {n.RuntimeName}");
            return new RTCondition<TCtx>(n.RuntimeName, pred, ctx.Tracer);
        }

        static IRTNode<TCtx> TryResolveCustom<TCtx>(BuildCtx<TCtx> ctx, BTNodeBase node)
        {
            // 允许库返回“整块运行时节点”，例如 WaitSignalRT / 特殊装饰器
            var rt = ctx.Lib.ResolveCustom(node.GetType().Name, new NodeUserData(null, node.Guid, node.RuntimeName),
                ctx.Tracer);
            return rt; // 可能为 null
        }

        static IRTNode<TCtx> GetSingleChild<TCtx>(BuildCtx<TCtx> ctx, BTNodeBase node, string portName)
        {
            var port = node.GetOutputPort(portName);
            var conn = FirstValidConnection(port);
            if (conn == null) return null; // 没有效连接就返回 null（由上层做错误提示/短路）
            var next = conn.node as BTNodeBase;
            if (next == null)
            {
                Debug.LogWarning($"{node.RuntimeName}.{portName} 连接到非 BT 节点或空连接。");
                return null;
            }

            return CompileNode(ctx, next);
        }

        // 供 Sequence/Parallel 等用，安全遍历动态端口 Children
        static List<IRTNode<TCtx>> GetMultiChildren<TCtx>(BuildCtx<TCtx> ctx, BTNodeBase node, string fieldName,
            int count)
        {
            var list = new List<IRTNode<TCtx>>(count);
            for (int i = 0; i < count; i++)
            {
                // xNode 动态端口命名规则："字段名 空格 索引"
                var port = node.GetOutputPort($"{fieldName} {i}");
                var conn = FirstValidConnection(port);
                if (conn == null)
                {
                    list.Add(null);
                    continue;
                }

                var next = conn.node as BTNodeBase;
                list.Add(next ? CompileNode(ctx, next) : null);
            }

            return list;
        }

        static IRTNode<TCtx>[] GetListChildren<TCtx>(BuildCtx<TCtx> ctx, BTNodeBase node, string portName)
        {
            var port = node.GetOutputPort(portName);
            if (port == null) return Array.Empty<IRTNode<TCtx>>();
            var list = new List<IRTNode<TCtx>>(port.ConnectionCount);
            for (int i = 0; i < port.ConnectionCount; i++)
            {
                var next = port.GetConnection(i).node as BTNodeBase;
                var rt = CompileNode(ctx, next);
                if (rt != null) list.Add(rt);
            }

            return list.ToArray();
        }
    }
}