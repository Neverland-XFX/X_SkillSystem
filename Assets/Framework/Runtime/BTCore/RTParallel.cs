using System;
using System.Collections.Generic;

namespace XSkillSystem
{
    public sealed class RTParallel<TCtx> : RTNodeBase<TCtx>
    {
        readonly List<IRTNode<TCtx>> _children;
        readonly BTParallelPolicy _policy;
        readonly int _threshold; // 当 policy = Threshold 时有效
        readonly bool _abortOnHardStop;

        // 运行态
        int _runningCount;
        int _successCount;
        int _failureCount;
        readonly bool[] _finished;

        public RTParallel(
            string name,
            List<IRTNode<TCtx>> children,
            BTParallelPolicy policy,
            int threshold = 1,
            bool abortOnHardStop = true,
            IBTTracer tracer = null
        ) : base(name, tracer)
        {
            _children = children ?? throw new ArgumentNullException(nameof(children));
            if (policy == BTParallelPolicy.Threshold && (threshold < 1 || threshold > _children.Count))
                throw new ArgumentOutOfRangeException(nameof(threshold));
            _policy = policy;
            _threshold = threshold;
            _abortOnHardStop = abortOnHardStop;
            _finished = new bool[_children.Count];
        }

        public override void Reset(ref TCtx ctx)
        {
            Array.Fill(_finished, false);
            _runningCount = _children.Count;
            _successCount = _failureCount = 0;
            foreach (var c in _children) c.Reset(ref ctx);
        }

        public override void Abort(ref TCtx ctx)
        {
            base.Abort(ref ctx);
            if (_abortOnHardStop)
                foreach (var c in _children)
                    c.Abort(ref ctx);
            Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (_runningCount == 0) Reset(ref ctx);

            for (int i = 0; i < _children.Count; i++)
            {
                if (_finished[i]) continue;
                var s = _children[i].Tick(ref ctx, rng);
                if (s == BTStatus.Running) continue;

                _finished[i] = true;
                _runningCount--;
                if (s == BTStatus.Success) _successCount++;
                else _failureCount++;

                switch (_policy)
                {
                    // 提前结束判断
                    case BTParallelPolicy.AnySuccess when s == BTStatus.Success:
                    {
                        // 提前终止剩余 Running 子节点
                        for (int j = 0; j < _children.Count; j++)
                            if (!_finished[j])
                                _children[j].Abort(ref ctx);
                        Exit(BTStatus.Success);
                        Reset(ref ctx);
                        return BTStatus.Success;
                    }
                    case BTParallelPolicy.AllSuccess when s == BTStatus.Failure:
                    {
                        for (int j = 0; j < _children.Count; j++)
                            if (!_finished[j])
                                _children[j].Abort(ref ctx);
                        Exit(BTStatus.Failure);
                        Reset(ref ctx);
                        return BTStatus.Failure;
                    }
                    case BTParallelPolicy.Threshold when _successCount >= _threshold:
                    {
                        for (int j = 0; j < _children.Count; j++)
                            if (!_finished[j])
                                _children[j].Abort(ref ctx);
                        Exit(BTStatus.Success);
                        Reset(ref ctx);
                        return BTStatus.Success;
                    }
                }
            }

            if (_runningCount > 0)
            {
                Exit(BTStatus.Running);
                return BTStatus.Running;
            }

            // 所有完成后根据策略判定
            BTStatus result = _policy switch
            {
                BTParallelPolicy.AnySuccess => _successCount > 0 ? BTStatus.Success : BTStatus.Failure,
                BTParallelPolicy.AllSuccess => _failureCount == 0 ? BTStatus.Success : BTStatus.Failure,
                BTParallelPolicy.Threshold => _successCount >= _threshold ? BTStatus.Success : BTStatus.Failure,
                _ => BTStatus.Failure
            };
            Exit(result);
            Reset(ref ctx);
            return result;
        }
    }
}