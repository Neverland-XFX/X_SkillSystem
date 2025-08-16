using System;
using System.Collections.Generic;
using System.Linq;
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
        // ------- 辅助函数：查找动态端口的第一个有效连接 -------
        // 例如 baseName = "Children"，xNode 的动态端口名为 "Children 0", "Children 1", ...
        static NodePort FindFirstDynamicConnection(BTNodeBase node, string baseName)
        {
            if (node == null) return null;
            foreach (var p in node.Ports) // node.Ports 返回 IEnumerable<NodePort>
            {
                if (!p.IsOutput) continue;
                if (string.IsNullOrEmpty(p.fieldName)) continue;
                // 精准匹配 "Children" 或 "Children <index>"
                if (p.fieldName.Equals(baseName, StringComparison.Ordinal) ||
                    p.fieldName.StartsWith(baseName + " ", StringComparison.Ordinal))
                {
                    var c = FirstValidConnection(p);
                    if (c != null) return c;
                }
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
                
                InverterNode n => new RTInverter<TCtx>(n.RuntimeName,
                    GetSingleChild(ctx, n, nameof(InverterNode.Child)), ctx.Tracer),
                SucceederNode n => new RTSucceeder<TCtx>(n.RuntimeName,
                    GetSingleChild(ctx, n, nameof(SucceederNode.Child)), ctx.Tracer),
                FailureNode n => new RTFailure<TCtx>(n.RuntimeName, GetSingleChild(ctx, n, nameof(FailureNode.Child)),
                    ctx.Tracer),
                TimeoutNode n => new TimeoutDecoratorRT<TCtx>(n.RuntimeName,
                    GetSingleChild(ctx, n, nameof(TimeoutNode.Child)), n.Seconds, ctx.Tracer),
                RepeatNode n => new RTRepeat<TCtx>(n.RuntimeName, GetSingleChild(ctx, n, nameof(RepeatNode.Child)),
                    n.Count, n.BreakOnFailure, ctx.Tracer),
                WaitNode n => new WaitRT<TCtx>(n.RuntimeName, n.Seconds, ctx.Tracer),

                ActionNode n => CompileAction(ctx, n),
                ConditionNode n => CompileCondition(ctx, n),

                ChannelGuardNode n => Compile_ChannelGuard(ctx, n),
                CastTimeNode n => Compile_CastTime(ctx, n),
                TL_PlayNode n => Compile_TLPlay(ctx, n),
                TL_StopNode n => Compile_TLStop(ctx, n),
                TL_WaitSignalNode n => Compile_TLWait(ctx, n),
                
                
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
            return new RTSequence<TCtx>(n.RuntimeName, arr.ToList(), ctx.Tracer);
        }

        static IRTNode<TCtx> CompileSelector<TCtx>(BuildCtx<TCtx> ctx, SelectorNode n)
        {
            var arr = GetListChildren(ctx, n, nameof(SelectorNode.Children));
            return new RTSelector<TCtx>(n.RuntimeName, arr.ToList(), ctx.Tracer);
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

            return new RTParallel<TCtx>(n.RuntimeName, arr.ToList(), (BTParallelPolicy)policy, tracer:ctx.Tracer);
        }

        static IRTNode<TCtx> CompileRandomSelector<TCtx>(BuildCtx<TCtx> ctx, RandomSelectorNode n)
        {
            var arr = GetListChildren(ctx, n, nameof(RandomSelectorNode.Children));
            var w = n.Weights;
            return new RTRandomSelector<TCtx>(n.RuntimeName, arr, w, ctx.Tracer);
        }

        static IRTNode<TCtx> CompileSubTree<TCtx>(BuildCtx<TCtx> ctx, SubTreeNode n)
        {
            if (n.Graph == null) throw new Exception($"SubTree `{n.RuntimeName}` 未指定 Graph。");
            var sub = Build(n.Graph, ctx.Lib, n.InheritTracer ? ctx.Tracer : new ConsoleTracer());
            return new RTSubTree<TCtx>(n.RuntimeName, sub, ctx.Tracer);
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
        // Compile ChannelGuardNode -> ChannelGuardRT
        static IRTNode<TCtx> Compile_ChannelGuard<TCtx>(BuildCtx<TCtx> ctx, ChannelGuardNode cg)
        {
            var childRt = GetSingleChild(ctx, cg, nameof(ChannelGuardNode.Child));
            // childRt may be null (checked by ChannelGuardRT constructor / runtime)
            return new RTChannelGuard<TCtx>(
                cg.RuntimeName,
                childRt,
                Channel.Cast,
                cg.Priority,
                cg.Timeout,
                getSM: (TCtx c) =>
                {
                    // 试图从上下文取 StateMachine；这里假定运行时使用 XContext
                    if (c is XContext xc && xc.Caster != null)
                        return xc.Caster.GetComponent<StateMachine>();
                    return null;
                },
                bus: null, // 通常由运行时 Node 从 XContext 取得 Bus；传 null 可行
                ctx.Tracer);
        }
        
        static IRTNode<TCtx> Compile_TLPlay<TCtx>(BuildCtx<TCtx> ctx, TL_PlayNode n)
        {
            return new RTTL_Play<TCtx>(n.RuntimeName, n.Config, ctx.Tracer);
        }
        static IRTNode<TCtx> Compile_TLStop<TCtx>(BuildCtx<TCtx> ctx, TL_StopNode n)
        {
            return new RTTL_Stop<TCtx>(n.RuntimeName, n.Config, ctx.Tracer);
        }
        static IRTNode<TCtx> Compile_TLWait<TCtx>(BuildCtx<TCtx> ctx, TL_WaitSignalNode n)
        {
            return new RTTL_WaitSignal<TCtx>(n.RuntimeName, n.Config, ctx.Tracer);
        }

        // Compile CastTimeNode -> CastTimeRT
        static IRTNode<TCtx> Compile_CastTime<TCtx>(BuildCtx<TCtx> ctx, CastTimeNode cd)
        {
            var childRt = GetSingleChild(ctx, cd, nameof(CastTimeNode.Child));
            return new RTCastTime<TCtx>(
                cd.RuntimeName,
                childRt,
                cd.Duration,
                Channel.Cast,
                cd.Priority,
                (StateId)cd.InterruptStates,
                getSM: (TCtx c) =>
                {
                    if (c is XContext xc && xc.Caster != null)
                        return xc.Caster.GetComponent<StateMachine>();
                    return null;
                },
                getDelta: (TCtx c) =>
                {
                    if (c is XContext xc && xc.Clock != null)
                        return (float)xc.Clock.DeltaTime;
                    return Time.deltaTime;
                },
                ctx.Tracer);
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
            if (node == null) return null;

            // 尝试直接的命名端口（非动态端口）
            var port = node.GetOutputPort(portName);
            var conn = FirstValidConnection(port);

            // 若没有直接端口连接，尝试查找动态命名端口（Children 0 / Children 1 ...）
            if (conn == null)
                conn = FindFirstDynamicConnection(node, portName);

            if (conn == null) return null;

            var next = conn.node as BTNodeBase;
            if (next == null)
            {
                Debug.LogWarning($"{node.RuntimeName}.{portName} 连接到非 BT 节点或空连接（node:{node.name}, port:{portName}）。");
                return null;
            }

            return CompileNode(ctx, next);
            
            // var port = node.GetOutputPort(portName);
            // var conn = FirstValidConnection(port);
            // if (conn == null) return null; // 没有效连接就返回 null（由上层做错误提示/短路）
            // var next = conn.node as BTNodeBase;
            // if (next == null)
            // {
            //     Debug.LogWarning($"{node.RuntimeName}.{portName} 连接到非 BT 节点或空连接。");
            //     return null;
            // }
            //
            // return CompileNode(ctx, next);
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
            if (node == null) return Array.Empty<IRTNode<TCtx>>();

            // 收集所有符合 "portName" 前缀的输出端口（例如 "Children 0", "Children 1"）
            var found = new List<(int idx, NodePort port)>();
            foreach (var p in node.Ports)
            {
                if (!p.IsOutput) continue;
                if (string.IsNullOrEmpty(p.fieldName)) continue;
                if (p.fieldName.Equals(portName, StringComparison.Ordinal))
                {
                    // treat as index 0 (no suffix)
                    found.Add((0, p));
                    continue;
                }
                if (p.fieldName.StartsWith(portName + " ", StringComparison.Ordinal))
                {
                    // try parse suffix number
                    var suffix = p.fieldName.Substring(portName.Length).Trim();
                    int idx = int.MaxValue;
                    if (int.TryParse(suffix, out var v)) idx = v;
                    found.Add((idx, p));
                }
            }

            if (found.Count == 0)
            {
                // 兼容旧实现：尝试直接取单个 port（可能对非动态端口有用）
                var single = node.GetOutputPort(portName);
                if (single != null)
                {
                    var list = new List<IRTNode<TCtx>>();
                    var c = FirstValidConnection(single);
                    if (c != null && c.node is BTNodeBase nb)
                    {
                        var rt = CompileNode(ctx, nb);
                        if (rt != null) list.Add(rt);
                    }
                    return list.ToArray();
                }
                return Array.Empty<IRTNode<TCtx>>();
            }

            // 按 index 排序，低到高
            found.Sort((a, b) => a.idx.CompareTo(b.idx));

            var result = new List<IRTNode<TCtx>>(found.Count);
            foreach (var (idx, port) in found)
            {
                var conn = FirstValidConnection(port);
                if (conn == null) continue; // 该端口没有有效连接
                var next = conn.node as BTNodeBase;
                if (next == null)
                {
                    Debug.LogWarning($"{node.RuntimeName}.{port.fieldName} 连接到非 BT 节点或 null。");
                    continue;
                }
                var rt = CompileNode(ctx, next);
                if (rt != null) result.Add(rt);
            }

            return result.ToArray();
        
            
            // var port = node.GetOutputPort(portName);
            // if (port == null) return Array.Empty<IRTNode<TCtx>>();
            // var list = new List<IRTNode<TCtx>>(port.ConnectionCount);
            // for (int i = 0; i < port.ConnectionCount; i++)
            // {
            //     var next = port.GetConnection(i).node as BTNodeBase;
            //     var rt = CompileNode(ctx, next);
            //     if (rt != null) list.Add(rt);
            // }
            //
            // return list.ToArray();
        }
    }
}