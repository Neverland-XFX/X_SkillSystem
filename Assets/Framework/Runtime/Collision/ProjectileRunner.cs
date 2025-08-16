using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XSkillSystem
{
    [DisallowMultipleComponent]
    public sealed class ProjectileRunner : MonoBehaviour
    {
        [Header("Config")] public ProjectileDef Def;

        [Header("Runtime")] public GameObject Caster;
        public string SkillId;
        public Vector3 Velocity;
        public GameObject HomingTarget; // 可选：追踪目标

        public ProjectilePool Pool;
        public EventBus Bus;

        // 命中回调要跑子树，复用已有节点库
        public DefaultNodeLibrary BaseLib;
        public BuffNodeLibrary BuffLib;
        public NumbersConfig Numbers;

        // 运行态
        private float _timeLeft;
        private int _pierced;
        private Vector3 _lastPos;

        // 对同一目标命中次数去重
        private readonly Dictionary<int, int> _hitCount = new(16);

        // 本地碰撞缓冲，避免访问其它类的私有字段
        private static readonly Collider[] _tmpCols = new Collider[128];

        void OnEnable()
        {
            _timeLeft = Def ? Def.Lifetime : 8f;
            _pierced = 0;
            _hitCount.Clear();
            _lastPos = transform.position;

            Bus?.Publish(new EV_ProjectileSpawn(gameObject));
        }

        /// <summary>
        /// 初始化并激活弹体。
        /// </summary>
        public void Init(
            ProjectileDef def,
            GameObject caster,
            Vector3 pos,
            Vector3 dir,
            ProjectilePool pool,
            EventBus bus,
            DefaultNodeLibrary baseLib,
            BuffNodeLibrary buffLib,
            NumbersConfig numbers,
            string skillId,
            GameObject homingTarget = null)
        {
            Def = def;
            Caster = caster;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(dir);
            Velocity = dir * (def ? def.Speed : 0f);

            Pool = pool;
            Bus = bus;
            BaseLib = baseLib;
            BuffLib = buffLib;
            Numbers = numbers;
            SkillId = skillId;
            HomingTarget = homingTarget;

            gameObject.SetActive(true);
        }

        void Update()
        {
            if (Def == null)
            {
                Despawn();
                return;
            }

            float dt = Time.deltaTime;
            _timeLeft -= dt;
            if (_timeLeft <= 0f)
            {
                Despawn();
                return;
            }

            // 追踪
            if (Def.Homing && HomingTarget != null)
            {
                var want = (HomingTarget.transform.position - transform.position).normalized;
                var newDir = Vector3.RotateTowards(Velocity.normalized, want,
                    Def.HomingTurnRateDeg * Mathf.Deg2Rad * dt, float.PositiveInfinity);
                Velocity = newDir * Velocity.magnitude;
                transform.rotation = Quaternion.LookRotation(newDir);
            }

            // 积分速度：重力 + 加速度
            Velocity += Def.Gravity * dt;
            if (Mathf.Abs(Def.Accel) > 1e-6f)
            {
                var s = Mathf.Max(0f, Velocity.magnitude + Def.Accel * dt);
                Velocity = (s > 1e-6f) ? Velocity.normalized * s : Vector3.zero;
            }

            Vector3 nextPos = transform.position + Velocity * dt;

            // 连续碰撞检测
            if (Def.UseSphereCast)
            {
                var move = nextPos - _lastPos;
                float dist = move.magnitude;
                if (dist > 1e-6f)
                {
                    var dir = move / dist;
                    if (Physics.SphereCast(_lastPos, Def.Radius, dir, out var hit, dist, Def.HitLayers))
                    {
                        var go = hit.rigidbody ? hit.rigidbody.gameObject : hit.collider.gameObject;
                        if (TargetFilter.Pass(Caster, go, Def.TargetRule, dir) && CanHit(go))
                        {
                            OnHit(go, hit.point, hit.normal, dir);
                            if (_pierced > Def.Pierce)
                            {
                                Despawn();
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                // 非连续：到达 nextPos 后的重叠检查
                int n = Physics.OverlapSphereNonAlloc(nextPos, Def.Radius, _tmpCols, Def.HitLayers);
                for (int i = 0; i < n; i++)
                {
                    var col = _tmpCols[i];
                    var go = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
                    if (TargetFilter.Pass(Caster, go, Def.TargetRule,
                            Velocity.sqrMagnitude > 1e-6f ? Velocity.normalized : transform.forward) && CanHit(go))
                    {
                        OnHit(go, col.ClosestPoint(nextPos), -Velocity.normalized, Velocity.normalized);
                        if (_pierced > Def.Pierce)
                        {
                            Despawn();
                            return;
                        }
                    }
                }
            }

            transform.position = nextPos;
            _lastPos = transform.position;
        }

        private bool CanHit(GameObject go)
        {
            int id = go.GetInstanceID();
            _hitCount.TryGetValue(id, out var cnt);
            if (cnt >= Mathf.Max(1, Def.MaxHitsPerTarget)) return false;
            _hitCount[id] = cnt + 1;
            _pierced++;
            return true;
        }

        private void OnHit(GameObject target, Vector3 point, Vector3 normal, Vector3 dir)
        {
            var info = new HitInfo(Caster, target, point, normal, dir, Time.time, SkillId);
            Bus?.Publish(new EV_ProjectileHit(info));

            // 命中回调：跑子树（伤害/上Buff/连锁等）
            if (Def.OnHitSubTree != null && BaseLib != null)
            {
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
                    RandomSeed = 42,
                    Stats = (Caster && Caster.TryGetComponent<IStatProvider>(out var sp)) ? sp : null
                };

                // 常用黑板键
                ctx.BB.Set(BBKeys.k_TargetGO, target);
                ctx.BB.Set(new BBKey<Vector3>("HitPoint"), point);

                var tree = BTCompiler.Build(Def.OnHitSubTree, lib, tracer);
                var session =
                    new BTSession<XContext>(Def.OnHitSubTree, tree, new DeterministicRandom(1337), Bus, tracer);
                session.Start(ref ctx);
                while (session.Tick(ref ctx) == BTStatus.Running)
                {
                }
            }
        }

        private void Despawn()
        {
            if (Pool) Pool.Return(this);
            else Destroy(gameObject);
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

#if UNITY_EDITOR
        // 可视化弹体碰撞半径与方向
        void OnDrawGizmosSelected()
        {
            if (Def == null) return;
            Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, Def.Radius);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
        }
#endif
    }
}