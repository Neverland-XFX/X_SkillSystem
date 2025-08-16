using UnityEngine;

namespace XSkillSystem
{
    public sealed class BuffRunner
    {
        readonly EventBus _bus;
        readonly DefaultNodeLibrary _baseLib;
        readonly BuffNodeLibrary _buffLib;
        readonly DeterministicRandom _rng;

        // 复用一个上下文供所有钩子图使用（按需填充字段）
        XContext _ctx;

        // 会话复用：不同子图可共用一个 Session（也可按需拆分）
        BTTree<XContext> _treeTmp;
        BTSession<XContext> _sessionTmp;

        public BuffRunner(EventBus bus, NumbersConfig numbers, int seed)
        {
            _bus = bus;
            _baseLib = new DefaultNodeLibrary();
            _buffLib = new BuffNodeLibrary(numbers);
            _rng = new DeterministicRandom(seed);
            _ctx = new XContext
            {
                Targets = new System.Collections.Generic.List<GameObject>(4),
                Clock = new UnityClock(),
                EventBus = bus,
                BB = new Blackboard(null),
                SkillLevel = 1,
                RandomSeed = seed
            };
        }

        public void SetupContext(GameObject owner, GameObject source, IStatProvider atkStats, IStatProvider tgtStats)
        {
            _ctx.Caster = source;
            _ctx.PrimaryTarget = owner;
            _ctx.Stats = atkStats;
        }

        // 运行一个 Graph（一次性运行到结束）
        public void RunOnce(BTGraph graph)
        {
            if (graph == null) return;
            var tracer = new ConsoleTracer();
            var lib = NodeLibraryMux.For(_baseLib, _buffLib);
            _treeTmp = BTCompiler.Build(graph, lib, tracer);
            _sessionTmp = new BTSession<XContext>(graph, _treeTmp, _rng, _bus, tracer);
            _sessionTmp.Start(ref _ctx);
            // 单次推进直到非 Running（简单同步执行）
            while (_sessionTmp.Tick(ref _ctx) == BTStatus.Running)
            {
                /* 可限制步数 */
            }
        }
    }
}