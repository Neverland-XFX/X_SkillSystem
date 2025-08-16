using System.Linq;

namespace XSkillSystem
{
    // 合并库
    internal static class NodeLibraryMux
    {
        public static INodeLibrary<XContext> For(params INodeLibrary<XContext>[] libs) => new Mux(libs);

        sealed class Mux : INodeLibrary<XContext>
        {
            readonly INodeLibrary<XContext>[] _libs;

            public Mux(INodeLibrary<XContext>[] libs)
            {
                _libs = libs;
            }

            public RTAction<XContext>.Func ResolveAction(string id, object ud)
            {
                return _libs.Select(t => t.ResolveAction(id, ud)).FirstOrDefault(f => f != null);
            }

            public RTCondition<XContext>.Pred ResolveCondition(string id, object ud)
            {
                return _libs.Select(t => t.ResolveCondition(id, ud)).FirstOrDefault(f => f != null);
            }

            public IRTNode<XContext> ResolveCustom(string id, object ud, IBTTracer t)
            {
                return _libs.Select(t1 => t1.ResolveCustom(id, ud, t)).FirstOrDefault(f => f != null);
            }
        }
    }
}