using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// XSkillSystem 的运行时上下文（可作为 BT 的 TCtx）。
    /// </summary>
    public struct XContext : IHasClock
    {
        public GameObject Caster;
        public GameObject PrimaryTarget;
        public List<GameObject> Targets;
        // 施法者属性
        public IStatProvider Stats;
        // IHasClock
        public IClock Clock { get; set; }
        public IEventBus EventBus;
        // 运行时黑板（实例层）
        public Blackboard BB;

        // TODO:后续步骤扩展，当前技能信息、随机种子等，
        public int SkillLevel;
        // 可与 BTExecutor 的 rng 区分
        public int RandomSeed;
    }
}