using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    [DisallowMultipleComponent]
    public sealed class SkillController : MonoBehaviour
    {
        public BTGraph SkillGraph;
        public NumbersConfig Numbers;

        BTTree<XContext> _tree;
        BTSession<XContext> _session;
        XContext _ctx;
        DeterministicRandom _rng;

        void Start()
        {
            // 通过 Installer 或 Host 获取全局 Bus
            var bus = XSkillInstaller.Bus ?? EventBusHost.GetOrCreateGlobal();
            _rng = new DeterministicRandom(12345);

            _ctx = new XContext
            {
                Caster = gameObject,
                PrimaryTarget = null,
                Targets = new List<GameObject>(8),
                Clock = new UnityClock(),
                EventBus = bus,
                BB = new Blackboard(null),
                SkillLevel = 1,
                RandomSeed = 12345,
                Stats = GetComponent<IStatProvider>()
            };

            var tracer = new ConsoleTracer();
            _tree = BTCompiler.Build<XContext>(SkillGraph, XSkillInstaller.Library, tracer);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) StartSkill();

            if (_session != null)
            {
                var st = _session.Tick(ref _ctx);
                if (st != BTStatus.Running) _session = null;
            }
        }

        public void StartSkill()
        {
            _ctx.PrimaryTarget = FindNearestEnemy();
            _ctx.BB.Set(BBKeys.k_TargetGO, _ctx.PrimaryTarget);

            _session = new BTSession<XContext>(SkillGraph, _tree, _rng, _ctx.EventBus as EventBus, new ConsoleTracer());
            _session.Start(ref _ctx);
        }

        GameObject FindNearestEnemy()
        {
            var list = _ctx.Targets;
            list.Clear();
            AreaQuery.Sphere(transform.position, 30f, null, gameObject, transform.forward, list);
            list.RemoveAll(go => go == gameObject);
            list.Sort((a, b) =>
                (a.transform.position - transform.position).sqrMagnitude.CompareTo(
                    (b.transform.position - transform.position).sqrMagnitude));
            return list.Count > 0 ? list[0] : null;
        }
    }
}