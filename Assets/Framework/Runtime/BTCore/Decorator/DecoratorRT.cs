using System;
using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    // =================== 组合节点 ===================
    public sealed class SequenceRT<TCtx> : RTNodeBase<TCtx>
    {
        private readonly IRTNode<TCtx>[] _children;
        private int _i;

        public SequenceRT(string name, IRTNode<TCtx>[] children, IBTTracer tracer) : base(name, tracer)
        {
            _children = children ?? Array.Empty<IRTNode<TCtx>>();
        }

        public override void Reset(ref TCtx ctx)
        {
            _i = 0;
            foreach (var node in _children)
                node?.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            while (_i < _children.Length)
            {
                var c = _children[_i];
                var s = c.Tick(ref ctx, rng);
                switch (s)
                {
                    case BTStatus.Running:
                        Exit(BTStatus.Running);
                        return BTStatus.Running;
                    case BTStatus.Failure:
                        Exit(BTStatus.Failure);
                        return BTStatus.Failure;
                    default:
                        _i++; // success -> next
                        break;
                }
            }

            Exit(BTStatus.Success);
            return BTStatus.Success;
        }
    }
}