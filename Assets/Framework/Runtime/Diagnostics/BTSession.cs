using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 封装 BTExecutor 与 Tracer，统一发 Start/End 事件与注入 BusTracer。
    /// </summary>
    public sealed class BTSession<TCtx>
    {
        public readonly BTTree<TCtx> Tree;
        public readonly BTExecutor<TCtx> Exec;
        public readonly BTTreeInfo Info;

        public readonly EventBus Bus;

        // 组合Tracer（Bus+Console等）
        public readonly IBTTracer Tracer;

        bool _started;

        public BTSession(BTGraph graphAsset, BTTree<TCtx> tree, DeterministicRandom rng, EventBus bus,
            IBTTracer extra = null)
        {
            Tree = tree;
            Bus = bus;
            int treeId = Mathf.Abs((graphAsset.GetInstanceID() ^ System.Environment.TickCount));
            Info = new BTTreeInfo(treeId, graphAsset.name, graphAsset.GetInstanceID());

            // 构建 BusTracer + 其它 tracer 的组合
            var busTracer = new BusTracer(bus, Info, () => Time.frameCount, () => Time.timeAsDouble);
            Tracer = extra == null ? (IBTTracer)busTracer : new CompositeTracer(busTracer, extra);

            // 用新的 tracer 包装 Tree（保持 root 不变）
            // 注：Tree.Tracer 在第1步里只用于传入节点构造；这里直接复用 tree 即可
            Exec = new BTExecutor<TCtx>(Tree, rng);
        }

        public void Start(ref TCtx ctx)
        {
            if (_started) return;
            Bus.Publish(new EV_BTTreeStart(Info));
            Exec.Start(ref ctx);
            _started = true;
        }

        public BTStatus Tick(ref TCtx ctx)
        {
            if (!_started) return Exec.LastStatus;
            var s = Exec.Tick(ref ctx);
            if (s != BTStatus.Running)
            {
                Bus.Publish(new EV_BTTreeEnd(Info, (byte)s));
                _started = false;
            }

            return s;
        }

        public void Abort(ref TCtx ctx, BTStopMode mode = BTStopMode.Hard)
        {
            if (!_started) return;
            Exec.Abort(ref ctx, mode);
            Bus.Publish(new EV_BTTreeEnd(Info, (byte)BTStatus.Failure));
            _started = false;
        }
    }
}