using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    [DisallowMultipleComponent]
    public sealed class BuffSystem : MonoBehaviour
    {
        // 提供给 Tick 伤害等
        public NumbersConfig Numbers;

        // 可从角色或全局注入
        public EventBus Bus;

        // 角色属性组件（用于注入修正）
        public IStatWritable StatWritable;

        // 自身作为攻击方的属性（可与上同一个）
        public IStatProvider AttackerStatsAdapter;

        readonly List<BuffInstance> _list = new(16);

        void Awake()
        {
            if (Bus == null) Bus = new EventBus();
            if (StatWritable == null) StatWritable = GetComponent<IStatWritable>();
            if (AttackerStatsAdapter == null) AttackerStatsAdapter = GetComponent<IStatProvider>();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            // 周期推进
            for (int i = _list.Count - 1; i >= 0; i--)
            {
                var b = _list[i];
                b.TimeLeft -= dt;
                if (b.Def.TickInterval > 0f)
                {
                    b.NextTickLeft -= dt;
                    if (b.NextTickLeft <= 0f)
                    {
                        // 触发 OnTick（将攻击方设为快照或来源）
                        var atk = b.Snapshot ?? (AttackerStatsAdapter ?? b.Snapshot);
                        b.Runner.SetupContext(b.Owner, b.Source, atk, null);
                        b.Runner.RunOnce(b.Def.OnTick);
                        // 避免累积误差
                        b.NextTickLeft += Mathf.Max(0.001f, b.Def.TickInterval); 
                    }
                }

                if (b.Expired)
                {
                    // OnRemove
                    b.Runner.SetupContext(b.Owner, b.Source, AttackerStatsAdapter, null);
                    b.Runner.RunOnce(b.Def.OnRemove);

                    // 移除修正
                    RemoveModifiers(b);

                    _list.RemoveAt(i);
                }
            }
        }

        public void Apply(BuffDef def, GameObject source, int stacks = 1)
        {
            if (def == null || stacks <= 0) return;
            
            if (def.UniquePerSource)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    var bi = _list[i];
                    if (bi.Def == def && bi.Source == source)
                    {
                        AddStacks(bi, stacks, refresh: def.Stacks != StackPolicy.Independent);
                        return;
                    }
                }
            }

            if (def.Stacks == StackPolicy.Independent)
            {
                for (int k = 0; k < stacks; k++)
                    CreateInstance(def, source, 1);
            }
            else
            {
                // 查找是否已有同类
                BuffInstance exist = null;
                for (int i = 0; i < _list.Count; i++)
                    if (_list[i].Def == def)
                    {
                        exist = _list[i];
                        break;
                    }

                if (exist == null) CreateInstance(def, source, Mathf.Min(stacks, def.MaxStacks));
                else AddStacks(exist, stacks, refresh: def.Stacks == StackPolicy.RefreshTime);
            }
        }

        public void Dispel(DispelType mask, int count = int.MaxValue)
        {
            for (int i = _list.Count - 1; i >= 0 && count > 0; i--)
            {
                var b = _list[i];
                if ((b.Def.Dispel & mask) != 0)
                {
                    RemoveModifiers(b);
                    b.Runner.SetupContext(b.Owner, b.Source, AttackerStatsAdapter, null);
                    b.Runner.RunOnce(b.Def.OnRemove);
                    _list.RemoveAt(i);
                    count--;
                }
            }
        }

        public bool Has(BuffDef def)
        {
            for (int i = 0; i < _list.Count; i++)
                if (_list[i].Def == def)
                    return true;
            return false;
        }

        public int GetStacks(BuffDef def)
        {
            for (int i = 0; i < _list.Count; i++)
                if (_list[i].Def == def)
                    return _list[i].Stacks;
            return 0;
        }

        
        void CreateInstance(BuffDef def, GameObject source, int initialStacks)
        {
            var inst = new BuffInstance(def, owner: this.gameObject, source: source);
            inst.Stacks = Mathf.Clamp(initialStacks, 1, def.MaxStacks);
            inst.Runner = new BuffRunner(Bus, Numbers, Random.Range(1, int.MaxValue));

            // 快照
            if (def.SnapshotAtApply)
            {
                var atk = source != null ? source.GetComponent<IStatProvider>() : AttackerStatsAdapter;
                inst.Snapshot = new SnapshotStats(atk,
                    (int)StatId.Attack, (int)StatId.SpellPower, (int)StatId.CritChance, (int)StatId.CritDamage,
                    (int)StatId.ElementalBonus_Fire, (int)StatId.ElementalBonus_Ice, (int)StatId.SkillBonus,
                    (int)StatId.Level);
            }

            // 应用修正
            ApplyModifiers(inst);

            // OnApply
            var atkStats = def.SnapshotAtApply
                ? (IStatProvider)inst.Snapshot
                : (source != null ? source.GetComponent<IStatProvider>() : AttackerStatsAdapter);
            inst.Runner.SetupContext(inst.Owner, inst.Source, atkStats, null);
            inst.Runner.RunOnce(def.OnApply);

            _list.Add(inst);
        }

        void AddStacks(BuffInstance b, int delta, bool refresh)
        {
            int newStacks = Mathf.Clamp(b.Stacks + delta, 1, b.Def.MaxStacks);
            if (newStacks == b.Stacks && !refresh) return;

            // 先移除旧修正，再以新叠数加回
            RemoveModifiers(b);
            b.Stacks = newStacks;
            ApplyModifiers(b);

            if (refresh)
            {
                b.TimeLeft = b.Def.Duration;
                b.NextTickLeft = b.Def.TickInterval;
            }
        }

        void ApplyModifiers(BuffInstance b)
        {
            if (StatWritable == null || b.Def.ModifiersPerStack == null) return;
            var list = new System.Collections.Generic.List<StatModifier>(b.Def.ModifiersPerStack.Length);
            for (int i = 0; i < b.Def.ModifiersPerStack.Length; i++)
            {
                var md = b.Def.ModifiersPerStack[i];
                var mod = new StatModifier(md.StatId, md.Op, md.ValuePerStack, b.Stacks, md.UniqueKey);
                StatWritable.AddModifier(mod);
                list.Add(mod);
            }

            b.ActiveMods = list.ToArray();
        }

        void RemoveModifiers(BuffInstance b)
        {
            if (StatWritable == null || b.ActiveMods == null) return;
            for (int i = 0; i < b.ActiveMods.Length; i++) StatWritable.RemoveModifier(b.ActiveMods[i]);
            b.ActiveMods = null;
        }

        
        int _tokDealt, _tokHit;

        void OnEnable()
        {
            if (Bus == null) return;
            _tokDealt = Bus.Subscribe<EV_DamageApplied>(OnDamageApplied, e => e.Attacker == this.gameObject);
            _tokHit = Bus.Subscribe<EV_DamageApplied>(OnDamaged, e => e.Target == this.gameObject);
        }

        void OnDisable()
        {
            if (Bus == null) return;
            Bus.Unsubscribe<EV_DamageApplied>(_tokDealt);
            Bus.Unsubscribe<EV_DamageApplied>(_tokHit);
        }

        void OnDamageApplied(EV_DamageApplied e)
        {
            // 对他人造成伤害
            for (int i = 0; i < _list.Count; i++)
            {
                var b = _list[i];
                if (b.Def.OnOwnerDealDamage == null) continue;
                b.Runner.SetupContext(b.Owner, b.Source, AttackerStatsAdapter, null);
                b.Runner.RunOnce(b.Def.OnOwnerDealDamage);
            }
        }

        void OnDamaged(EV_DamageApplied e)
        {
            // 被他人击中
            for (int i = 0; i < _list.Count; i++)
            {
                var b = _list[i];
                if (b.Def.OnOwnerHit == null) continue;
                b.Runner.SetupContext(b.Owner, e.Attacker, AttackerStatsAdapter, null);
                b.Runner.RunOnce(b.Def.OnOwnerHit);
            }

            // 可发布二次事件
            Bus.Publish(new EV_OwnerHit(this.gameObject, e.Attacker, e.Amount, e.Type));
        }
    }
}