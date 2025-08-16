using UnityEngine;

namespace XSkillSystem.Samples
{
    public class XSkillBootstrap_Bus : MonoBehaviour
    {
        public BTGraph GraphAsset;
        public BlackboardAsset BBDefaults;

        EventBus _bus;
        RingRecorder _rec;
        BTProfiler _profiler;
        BTSession<XContext> _session;
        XContext _ctx;

        void Start()
        {
            _bus = new EventBus();
            _rec = new RingRecorder(512);
            _bus.AttachRecorder(_rec);

            _profiler = new BTProfiler(_bus);

            _ctx = new XContext
            {
                Caster = this.gameObject,
                Targets = new System.Collections.Generic.List<GameObject>(8),
                Clock = new UnityClock(),
                EventBus = _bus,
                BB = new Blackboard(BBDefaults),
                SkillLevel = 1,
                RandomSeed = 1337
            };

            // 编译树
            var lib = new DefaultNodeLibrary();
            var tracer = new ConsoleTracer(); // 也可以不加
            // 注意：这里传的 tracer 只用于节点构造阶段（节点内部保留）
            var tree = BTCompiler.Build(GraphAsset, lib, tracer);
            // 用与 Executor 同种 RNG
            var rng = new DeterministicRandom(_ctx.RandomSeed);

            _session = new BTSession<XContext>(GraphAsset, tree, rng, _bus, tracer);
            _session.Start(ref _ctx);

            // 示例：订阅日志
            _bus.Subscribe<EV_Log>(e => Debug.Log(e.Message));
        }

        void Update()
        {
            _session.Tick(ref _ctx);
        }

        void OnGUI()
        {
            // 简易调试面板：显示最近事件与Top节点
            GUILayout.BeginArea(new Rect(10, 10, 500, 400), GUI.skin.box);
            GUILayout.Label("<b>XSkillSystem · EventBus</b>");

            GUILayout.Label("<b>Recent:</b>");
            int shown = 0;
            foreach (var (payload, type) in _rec.Enumerate())
            {
                if (shown++ > 10) break;
                GUILayout.Label($"{type.Name} :: {payload}");
            }

            GUILayout.Space(6);
            GUILayout.Label("<b>Top Nodes (by time):</b>");
            int top = 0;
            foreach (var kv in _profiler.Stats)
            {
                if (top++ > 10) break;
                var s = kv.Value;
                GUILayout.Label(
                    $"{s.NodeName}  enter:{s.Enters}  succ:{s.Success}  fail:{s.Failure}  time:{s.TotalTime:F3}s");
            }

            GUILayout.EndArea();
        }
    }
}