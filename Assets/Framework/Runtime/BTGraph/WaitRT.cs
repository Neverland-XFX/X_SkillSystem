using System;

namespace XSkillSystem
{
    // 简单等待节点
    internal sealed class WaitRT<TCtx> : RTNodeBase<TCtx>
    {
        readonly float _seconds;
        float _elapsed;

        public WaitRT(string name, float seconds, IBTTracer tracer) : base(name, tracer)
        {
            _seconds = Math.Max(0f, seconds);
        }

        public override void Reset(ref TCtx ctx)
        {
            _elapsed = 0f;
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            _elapsed += 1f / 60f; // 先用帧估算，后续接 Clock
            if (_elapsed >= _seconds)
            {
                Exit(BTStatus.Success);
                _elapsed = 0f;
                return BTStatus.Success;
            }

            Exit(BTStatus.Running);
            return BTStatus.Running;
        }
    }
}