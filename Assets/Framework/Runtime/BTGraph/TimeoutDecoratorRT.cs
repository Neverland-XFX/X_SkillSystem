using System;

namespace XSkillSystem
{
    // 超时：超时返回 Failure，先简化版
    internal sealed class TimeoutDecoratorRT<TCtx> : RTDecorator<TCtx>
    {
        readonly float _seconds;
        float _elapsed;

        public TimeoutDecoratorRT(string name, IRTNode<TCtx> child, float seconds, IBTTracer tracer) : base(name, child,
            tracer)
        {
            _seconds = Math.Max(0f, seconds);
        }

        public override void Reset(ref TCtx ctx)
        {
            _elapsed = 0f;
            base.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            // TODO:暂用 1 Tick = 1 帧假设（后续用 Clock 注入）
            _elapsed += 1f / 60f;
            if (_elapsed >= _seconds)
            {
                Exit(BTStatus.Failure);
                _elapsed = 0f;
                return BTStatus.Failure;
            }

            var s = Child.Tick(ref ctx, rng);
            Exit(s);
            return s;
        }
    }
}