using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XSkillSystem
{
    [DisallowMultipleComponent]
    public sealed class AuraZone : MonoBehaviour
    {
        [Header("Config")] public AuraDef Def;
        public GameObject Caster;

        [Header("Runtime Dependencies")] public EventBus Bus;
        public DefaultNodeLibrary BaseLib;
        public BuffNodeLibrary BuffLib;

        // 跟踪当前在域内的目标（直接存 GameObject，避免使用 internal API）
        private readonly HashSet<GameObject> _inside = new();
        private float _timer;

        void Awake()
        {
            // 允许从宿主上取 EventBus（如果有统一的 EventBus 组件）
            if (Bus == null)
                Bus = GetComponent<EventBusHost>()?.Bus ?? XSkillInstaller.Bus ?? EventBusHost.GetOrCreateGlobal();
            ;
        }

        void Update()
        {
            if (Def == null) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = Mathf.Max(0.05f, Def.Interval);

            var list = _scratchList ??= new List<GameObject>(32);
            list.Clear();

            Vector3 pos = transform.position;
            Vector3 fwd = transform.forward;

            switch (Def.Shape)
            {
                case ShapeKind.Sphere:
                    AreaQuery.Sphere(pos, Def.Radius, Def.Rule, Caster, fwd, list); break;
                case ShapeKind.Box:
                    AreaQuery.Box(pos, Def.BoxHalfExt, transform.rotation, Def.Rule, Caster, fwd, list); break;
                case ShapeKind.Capsule:
                {
                    var p0 = pos + Vector3.up * Def.CapsuleHalfHeight;
                    var p1 = pos - Vector3.up * Def.CapsuleHalfHeight;
                    AreaQuery.Capsule(p0, p1, Def.Radius, Def.Rule, Caster, fwd, list);
                    break;
                }
                case ShapeKind.Cone:
                    AreaQuery.Cone(pos, fwd, Def.Radius, 60f, Def.Rule, Caster, list); break;
            }

            // 进入/离开判定
            var now = _scratchSet ??= new HashSet<GameObject>();
            now.Clear();
            foreach (var go in list)
                now.Add(go);

            // OnEnter
            foreach (var go in now.Where(go => !_inside.Contains(go)))
            {
                _inside.Add(go);
                if (Def.OnEnter) RunOne(Def.OnEnter, go);
            }

            // OnExit
            _toRemove ??= new List<GameObject>(8);
            _toRemove.Clear();
            foreach (var go in _inside.Where(go => !now.Contains(go)))
            {
                _toRemove.Add(go);
                if (Def.OnExit) RunOne(Def.OnExit, go);
            }

            foreach (var go in _toRemove)
                _inside.Remove(go);

            // OnTickEach
            if (Def.OnTickEach)
            {
                foreach (var go in list)
                    RunOne(Def.OnTickEach, go);
            }
        }

        // ----------------- 内部：运行一个子树 -----------------
        private void RunOne(BTGraph graph, GameObject target)
        {
            if (graph == null || target == null) return;

            var tracer = new ConsoleTracer();
            var lib = CombineLibs(BaseLib, BuffLib);

            var ctx = new XContext
            {
                Caster = Caster,
                PrimaryTarget = target,
                Targets = new List<GameObject>(1) { target },
                Clock = new UnityClock(),
                EventBus = Bus,
                BB = new Blackboard(null),
                SkillLevel = 1,
                RandomSeed = 2468,
                Stats = (Caster && Caster.TryGetComponent<IStatProvider>(out var sp)) ? sp : null
            };

            // 常用黑板
            ctx.BB.Set(BBKeys.k_TargetGO, target);
            // 可以扩展：命中点/法阵中心等

            var tree = BTCompiler.Build(graph, lib, tracer);
            var session = new BTSession<XContext>(graph, tree, new DeterministicRandom(99), Bus, tracer);
            session.Start(ref ctx);
            while (session.Tick(ref ctx) == BTStatus.Running)
            {
            }
        }

        // ----------------- 本地库合并器（避免外部依赖冲突） -----------------
        private static INodeLibrary<XContext> CombineLibs(DefaultNodeLibrary baseLib, BuffNodeLibrary buffLib)
            => new Mux(baseLib, buffLib);

        private sealed class Mux : INodeLibrary<XContext>
        {
            private readonly INodeLibrary<XContext>[] _libs;

            public Mux(params INodeLibrary<XContext>[] libs)
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

        // ----------------- 复用的小临时容器 -----------------
        private static List<GameObject> _scratchList;
        private static HashSet<GameObject> _scratchSet;
        private static List<GameObject> _toRemove;

        // 在 Scene 里可视化 Aura 边界
        void OnDrawGizmosSelected()
        {
            if (Def == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.25f);
            switch (Def.Shape)
            {
                case ShapeKind.Sphere:
                    Gizmos.DrawWireSphere(Vector3.zero, Def.Radius);
                    break;
                case ShapeKind.Box:
                    Gizmos.DrawWireCube(Vector3.zero, Def.BoxHalfExt * 2f);
                    break;
                case ShapeKind.Capsule:
                    // 简化显示：上下两个球 + 中间盒
                    Gizmos.DrawWireSphere(Vector3.up * Def.CapsuleHalfHeight, Def.Radius);
                    Gizmos.DrawWireSphere(Vector3.down * Def.CapsuleHalfHeight, Def.Radius);
                    Gizmos.DrawWireCube(Vector3.zero,
                        new Vector3(Def.Radius * 2f, Def.CapsuleHalfHeight * 2f, Def.Radius * 2f));
                    break;
            }
        }
    }
}