using System;
using System.Collections.Generic;

namespace XSkillSystem
{
    public static class BTGraphValidator
    {
        public static void Validate(BTGraph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            // 1) Root 数量
            var roots = new List<RootNode>();
            foreach (var n in graph.nodes)
                if (n is RootNode r)
                    roots.Add(r);
            switch (roots.Count)
            {
                case 0:
                    throw new Exception("图中没有 Root 节点");
                case > 1:
                    throw new Exception("图中存在多个 Root 节点");
            }

            // 2) 端口合法性 & 子数规则
            foreach (var n in graph.nodes)
            {
                switch (n)
                {
                    case SequenceNode seq:
                        RequireChildren(seq, nameof(SequenceNode.Children));
                        break;
                    case SelectorNode sel:
                        RequireChildren(sel, nameof(SelectorNode.Children));
                        break;
                    case ParallelNode par:
                        RequireChildren(par, nameof(ParallelNode.Children));
                        if (par.ModePolicy == ParallelNode.Mode.Threshold && par.Threshold < 1)
                            throw new Exception($"{par.RuntimeName} 的 Threshold 必须 >= 1");
                        break;
                    case InverterNode inv:
                        RequireSingle(inv, nameof(InverterNode.Child));
                        break;
                    case RepeatNode rep:
                        RequireSingle(rep, nameof(RepeatNode.Child));
                        if (rep.Count < 1) throw new Exception($"{rep.RuntimeName} 的 Count 必须 >= 1");
                        break;
                    case TimeoutNode tim:
                        RequireSingle(tim, nameof(TimeoutNode.Child));
                        if (tim.Seconds < 0f) throw new Exception($"{tim.RuntimeName} 的 Seconds 不能小于 0");
                        break;
                    case ActionNode act:
                        if (string.IsNullOrEmpty(act.ActionId)) throw new Exception($"{act.RuntimeName} 缺少 ActionId");
                        break;
                    case ConditionNode cond:
                        if (string.IsNullOrEmpty(cond.ConditionId))
                            throw new Exception($"{cond.RuntimeName} 缺少 ConditionId");
                        break;
                    case WaitNode wait:
                        if (wait.Seconds < 0f) throw new Exception($"{wait.RuntimeName} 的 Seconds 不能小于 0");
                        break;
                    case SubTreeNode sub:
                        if (sub.graph == null) throw new Exception($"{sub.RuntimeName} 缺少 SubGraph 引用");
                        break;
                }
            }

            // 3) 简单环检测（从 Root 出发 DFS）
            var visited = new HashSet<BTNodeBase>();
            var stack = new HashSet<BTNodeBase>();
            DFSCheck(roots[0], visited, stack);
        }

        static void RequireChildren(BTNodeBase node, string field)
        {
            var port = node.GetOutputPort(field);
            if (port == null || port.ConnectionCount == 0)
                throw new Exception($"{node.name} 的 {field} 没有连接子节点");
        }

        static void RequireSingle(BTNodeBase node, string field)
        {
            var port = node.GetOutputPort(field);
            if (port == null || port.ConnectionCount != 1)
                throw new Exception($"{node.name} 的 {field} 必须且只能连接 1 个子节点");
        }

        static void DFSCheck(BTNodeBase node, HashSet<BTNodeBase> visited, HashSet<BTNodeBase> stack)
        {
            if (stack.Contains(node)) throw new Exception($"检测到环：{node.name}");
            if (!visited.Add(node)) return;
            stack.Add(node);

            foreach (var port in node.DynamicOutputs)
            {
                for (int i = 0; i < port.ConnectionCount; i++)
                {
                    var conn = port.GetConnection(i);
                    if (conn.node is BTNodeBase child) DFSCheck(child, visited, stack);
                }
            }

            foreach (var port in node.Outputs) // 包括非动态端口（Child/Next）
            {
                if (port.IsDynamic) continue;
                for (int i = 0; i < port.ConnectionCount; i++)
                {
                    var conn = port.GetConnection(i);
                    if (conn.node is BTNodeBase child) DFSCheck(child, visited, stack);
                }
            }

            stack.Remove(node);
        }
    }
}