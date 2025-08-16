using System;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 把 BTCore 的 Tracer 事件转发到 EventBus。
    /// </summary>
    public sealed class BusTracer : IBTTracer
    {
        readonly EventBus _bus;
        readonly BTTreeInfo _tree;
        readonly Func<int> _getFrame;
        readonly Func<double> _getTime;

        public BusTracer(EventBus bus, BTTreeInfo tree, Func<int> getFrame, Func<double> getTime)
        {
            _bus = bus;
            _tree = tree;
            _getFrame = getFrame ?? (() => Time.frameCount);
            _getTime = getTime ?? (() => Time.timeAsDouble);
        }

        public void Enter(string nodeName)
            => _bus.Publish(new EV_BTNodeEnter(_tree, nodeName, _getFrame(), _getTime()));

        public void Exit(string nodeName, BTStatus status)
            => _bus.Publish(new EV_BTNodeExit(_tree, nodeName, (byte)status, _getFrame(), _getTime()));

        public void Abort(string nodeName)
            => _bus.Publish(new EV_BTNodeAbort(_tree, nodeName, _getFrame(), _getTime()));
    }
}