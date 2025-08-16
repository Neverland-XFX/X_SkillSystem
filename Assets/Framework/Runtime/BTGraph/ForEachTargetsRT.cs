using UnityEngine;

namespace XSkillSystem
{
    public sealed class ForEachTargetsRT<TCtx> : RTNodeBase<TCtx>
    {
        readonly IRTNode<TCtx> _child;
        readonly bool _break;

        public ForEachTargetsRT(string name, IRTNode<TCtx> child, bool breakOnFirstSuccess, IBTTracer tracer)
            : base(name, tracer)
        {
            _child = child;
            _break = breakOnFirstSuccess;
        }

        public override BTStatus Tick(ref TCtx ctx, IBTRandom rng)
        {
            Enter();
            object bb = null;
            System.Collections.IList targets = null;
            var t = typeof(TCtx);
            var bbProp = t.GetProperty("BB");
            var tgProp = t.GetProperty("Targets");
            if (bbProp != null) bb = bbProp.GetValue(ctx);
            if (tgProp != null) targets = tgProp.GetValue(ctx) as System.Collections.IList;
            if (targets == null || targets.Count == 0)
            {
                Exit(BTStatus.Failure);
                return BTStatus.Failure;
            }

            int success = 0;
            foreach (var obj in targets)
            {
                var go = obj as GameObject;
                if (go == null) continue;
                if (bb != null)
                {
                    var bbType = bb.GetType();
                    var set = bbType.GetMethod("Set")?.MakeGenericMethod(typeof(GameObject));
                    if (set != null) set.Invoke(bb, new object[] { new BBKey<GameObject>("TargetGO"), go });
                }

                var s = _child.Tick(ref ctx, rng);
                if (s != BTStatus.Success) continue;
                success++;
                if (!_break) continue;
                Exit(BTStatus.Success);
                return BTStatus.Success;
            }

            var r = success > 0 ? BTStatus.Success : BTStatus.Failure;
            Exit(r);
            return r;
        }
    }
}