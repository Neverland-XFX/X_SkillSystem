using System.Collections.Generic;

namespace XSkillSystem
{
    /// <summary>
    /// 合并多个 INodeLibrary 为一个，按顺序查找。
    /// （全局通用，避免在各处重复写 Mux）
    /// </summary>
    public static class XSLibMux
    {
        public static INodeLibrary<XContext> For(params INodeLibrary<XContext>[] libs)
            => new Mux(libs);

        private sealed class Mux : INodeLibrary<XContext>
        {
            private readonly INodeLibrary<XContext>[] _libs;

            public Mux(INodeLibrary<XContext>[] libs)
            {
                _libs = libs;
            }

            public RTAction<XContext>.Func ResolveAction(string id, object ud)
            {
                for (int i = 0; i < _libs.Length; i++)
                {
                    var f = _libs[i].ResolveAction(id, ud);
                    if (f != null) return f;
                }

                return null;
            }

            public RTCondition<XContext>.Pred ResolveCondition(string id, object ud)
            {
                for (int i = 0; i < _libs.Length; i++)
                {
                    var f = _libs[i].ResolveCondition(id, ud);
                    if (f != null) return f;
                }

                return null;
            }

            public IRTNode<XContext> ResolveCustom(string id, object ud, IBTTracer tracer)
            {
                for (int i = 0; i < _libs.Length; i++)
                {
                    var f = _libs[i].ResolveCustom(id, ud, tracer);
                    if (f != null) return f;
                }

                return null;
            }
        }
    }
}