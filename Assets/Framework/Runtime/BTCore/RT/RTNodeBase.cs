using System;
using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    public abstract class RTNodeBase<TCtx> : IRTNode<TCtx>
    {
        protected readonly string NameInternal;
        protected readonly IBTTracer Tracer;
        public string Name => NameInternal;

        protected RTNodeBase(string name, IBTTracer tracer)
        {
            NameInternal = string.IsNullOrEmpty(name) ? GetType().Name : name;
            Tracer = tracer ?? new ConsoleTracer();
        }

        public virtual void Reset(ref TCtx ctx)
        {
        }

        public abstract BTStatus Tick(ref TCtx ctx, IBTRandom rng);

        public virtual void Abort(ref TCtx ctx)
        {
        }

        protected void Enter() => Tracer?.Enter(NameInternal);
        protected void Exit(BTStatus s) => Tracer?.Exit(NameInternal, s);

        protected static float GetDeltaTime(ref TCtx ctx)
        {
            try
            {
                object boxed = ctx!;
                var t = boxed.GetType();
                var pClock = t.GetProperty("Clock");
                var clock = pClock?.GetValue(boxed, null);
                if (clock != null)
                {
                    var pDt = clock.GetType().GetProperty("DeltaTime");
                    if (pDt != null)
                    {
                        var v = pDt.GetValue(clock, null);
                        if (v is float f) return f;
                        if (v is double d) return (float)d;
                    }
                }
#if UNITY_5_3_OR_NEWER
                return Time.deltaTime;
#else
                return 1f / 60f;
#endif
            }
            catch
            {
                return 1f / 60f;
            }
        }
    }
}