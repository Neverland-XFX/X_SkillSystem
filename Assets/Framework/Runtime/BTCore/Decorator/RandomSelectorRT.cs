using System;
using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    // =================== 随机选择 ===================
    public sealed class RandomSelectorRT<TCtx> : RTNodeBase<TCtx>
    {
        private readonly IRTNode<TCtx>[] _children;
        private readonly IList<float> _weights;
        private int _picked = -1;

        public RandomSelectorRT(string name, IRTNode<TCtx>[] children, IList<float> weights, IBTTracer tracer) : base(
            name, tracer)
        {
            _children = children ?? Array.Empty<IRTNode<TCtx>>();
            _weights = weights ?? Array.Empty<float>();
        }

        public override void Reset(ref TCtx ctx)
        {
            _picked = -1;
            foreach (var node in _children)
                node?.Reset(ref ctx);
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            if (_children.Length == 0)
            {
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            if (_picked < 0)
            {
                // —— 兼容任意 IBTRandom：优先反射 NextFloat01，其次用 NextIntInclusive 派生，最后用 UnityEngine.Random —— //
                float r = UnityEngine.Random.value;
                if (rng != null)
                {
                    try
                    {
                        var t = rng.GetType();
                        var m1 = t.GetMethod("NextFloat01", Array.Empty<Type>());
                        if (m1 != null) r = Convert.ToSingle(m1.Invoke(rng, null));
                        else
                        {
                            var m2 = t.GetMethod("NextIntInclusive", new[] { typeof(int), typeof(int) });
                            if (m2 != null)
                            {
                                int v = (int)m2.Invoke(rng, new object[] { 0, int.MaxValue });
                                r = (v / (float)int.MaxValue);
                            }
                        }
                    }
                    catch
                    {
                        /* 退回 UnityEngine.Random.value */
                    }
                }

                // 权重抽样
                var sum = 0f;
                for (int i = 0; i < _children.Length; i++)
                    sum += (i < _weights.Count ? Mathf.Max(0f, _weights[i]) : 1f);
                if (sum <= 0f) sum = _children.Length;

                var acc = 0f;
                for (int i = 0; i < _children.Length; i++)
                {
                    acc += (i < _weights.Count ? Mathf.Max(0f, _weights[i]) : 1f) / sum;
                    if (r <= acc)
                    {
                        _picked = i;
                        break;
                    }
                }

                if (_picked < 0) _picked = _children.Length - 1;
            }

            var c = _children[_picked];
            if (c == null)
            {
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            var s = c.Tick(ref ctx, rng);
            Exit(s);
            return s;
        }
    }
}