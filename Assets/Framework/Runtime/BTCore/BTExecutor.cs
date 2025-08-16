using System;

namespace XSkillSystem
{
    /// <summary>树
    /// 的调度器。外部每帧/每步调用 Tick。
    /// </summary>
    public sealed class BTExecutor<TCtx>
    {
        readonly BTTree<TCtx> _tree;
        readonly IBTRandom _rng;
        int _stepBudget;
        bool _aborting;

        public bool IsRunning { get; private set; }
        public BTStatus LastStatus { get; private set; }

        public BTExecutor(BTTree<TCtx> tree, IBTRandom rng)
        {
            _tree = tree ?? throw new ArgumentNullException(nameof(tree));
            _rng = rng ?? new DeterministicRandom(1);
            _stepBudget = tree.MaxNodePerTick;
            IsRunning = false;
            LastStatus = BTStatus.Failure;
        }

        public void Start(ref TCtx ctx)
        {
            _tree.Root.Reset(ref ctx);
            IsRunning = true;
            LastStatus = BTStatus.Running;
        }

        public void Abort(ref TCtx ctx, BTStopMode mode = BTStopMode.Hard)
        {
            if (!IsRunning) return;
            _aborting = true;
            if (mode == BTStopMode.Hard) _tree.Root.Abort(ref ctx);
            _aborting = false;
            IsRunning = false;
            LastStatus = BTStatus.Failure;
        }

        /// <summary>
        /// 外部传入 delta 即可；Clock 放到上层。
        /// </summary>
        public BTStatus Tick(ref TCtx ctx)
        {
            if (!IsRunning) return LastStatus;
            _stepBudget = _tree.MaxNodePerTick;

            var status = _tree.Root.Tick(ref ctx, _rng);

            if (_aborting) return LastStatus; // 保护

            if (status != BTStatus.Running)
            {
                IsRunning = false;
                LastStatus = status;
            }
            else
            {
                LastStatus = BTStatus.Running;
            }

            return LastStatus;
        }
    }
}