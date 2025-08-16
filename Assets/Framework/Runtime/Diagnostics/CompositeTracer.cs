using System;

namespace XSkillSystem
{
    /// <summary>
    /// 把多个 Tracer 合并（例如：BusTracer + ConsoleTracer）。
    /// </summary>
    public sealed class CompositeTracer : IBTTracer
    {
        readonly IBTTracer[] _arr;

        public CompositeTracer(params IBTTracer[] arr)
        {
            _arr = arr ?? Array.Empty<IBTTracer>();
        }

        public void Enter(string nodeName)
        {
            for (int i = 0; i < _arr.Length; i++) _arr[i].Enter(nodeName);
        }

        public void Exit(string nodeName, BTStatus status)
        {
            for (int i = 0; i < _arr.Length; i++) _arr[i].Exit(nodeName, status);
        }

        public void Abort(string nodeName)
        {
            for (int i = 0; i < _arr.Length; i++) _arr[i].Abort(nodeName);
        }
    }
}