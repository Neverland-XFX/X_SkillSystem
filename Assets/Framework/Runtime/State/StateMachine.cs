using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace XSkillSystem
{
    /// <summary>
    /// 角色状态机：管理短期/长期状态与通道（Cast/Action/Move 等），并通过 EventBus 发出事件。
    /// 修复点：
    /// - 使用 List<StateId> + 运行时 StateMask64 维护免疫/易伤，避免枚举掩码序列化报错。
    /// - EventBus 通过 EventBusHost / XSkillInstaller.Bus 获取，避免 GetComponent<EventBus>() 异常。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StateMachine : MonoBehaviour
    {
        #region --- Nested types ---

        [System.Serializable]
        sealed class StateEntry
        {
            public StateId Id;
            public int Stacks;
            // -1 = 永久
            public float TimeLeft;
        }

        [System.Serializable]
        sealed class ChannelLock
        {
            public Channel Channel;
            // 唯一句柄
            public int Handle;
            public int Priority;
            // 可选超时（<=0 表示不超时）
            public float TimeLeft;
            // 调试
            public string OwnerName;
        }

        /// <summary>
        /// 最多 64 位状态掩码（若已有 StateMask64 单独文件，可删除此内联定义）。
        /// </summary>
        [System.Serializable]
        public struct StateMask64
        {
            [SerializeField] private ulong _bits;

            public ulong Bits
            {
                get => _bits;
                set => _bits = value;
            }

            public void Clear() => _bits = 0ul;
            public bool Has(StateId id) => (((_bits >> (int)id) & 1ul) != 0ul);

            public void Set(StateId id, bool on = true)
            {
                int i = (int)id;
                if (i < 0 || i > 63) return;
                if (on) _bits |= (1ul << i);
                else _bits &= ~(1ul << i);
            }

            public void MergeFrom(IEnumerable<StateId> list, bool on = true)
            {
                if (list == null) return;
                foreach (var id in list) Set(id, on);
            }

            public override string ToString() => $"0x{_bits:X16}";
        }

        #endregion

        #region --- Inspector (Odin) ---

        [Title("Event Bus & Debug")] [Tooltip("从 EventBusHost / XSkillInstaller 注入；可留空。")]
        public EventBus Bus;

        [Title("Immunity / Vulnerability (Editor)")] [LabelText("Immune To"), InfoBox("用列表选择免疫的状态；运行时会合成为位掩码。")]
        public List<StateId> ImmuneTo = new();

        [LabelText("Vulnerable To")] public List<StateId> VulnerableTo = new();

        [ShowInInspector, ReadOnly, LabelText("ImmunityMask (runtime)")]
        public ulong ImmunityBits => _immunityMask.Bits;

        [ShowInInspector, ReadOnly, LabelText("VulnerableMask (runtime)")]
        public ulong VulnerableBits => _vulnerableMask.Bits;

        #endregion

        #region --- Obsolete ---

        [System.Obsolete("请迁移到 ImmuneTo/VulnerableTo 或 IsImmune()/IsVulnerable()")]
        public StateId ImmunityMask
        {
            get => (StateId)unchecked((int)_immunityMask.Bits); // 只导出低 32 位
            set
            {
                _immunityMask.Bits = unchecked((ulong)(int)value);
                EnsureList(ref ImmuneTo);
                SyncListFromMask(ImmuneTo, value);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        [System.Obsolete("请迁移到 ImmuneTo/VulnerableTo 或 IsImmune()/IsVulnerable()")]
        public StateId VulnerableMask
        {
            get => (StateId)unchecked((int)_vulnerableMask.Bits); // 只导出低 32 位
            set
            {
                _vulnerableMask.Bits = unchecked((ulong)(int)value);
                EnsureList(ref VulnerableTo);
                SyncListFromMask(VulnerableTo, value);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        static void EnsureList(ref List<StateId> list)
        {
            if (list == null) list = new List<StateId>(8);
        }

        static void SyncListFromMask(List<StateId> list, StateId mask)
        {
            list.Clear();
            // 逐枚举值转换（要求你的 StateId 是 [Flags] 且每个成员为 1<<n）
            foreach (StateId id in System.Enum.GetValues(typeof(StateId)))
            {
                if (id == StateId.None) continue;
                if ((mask & id) != 0) list.Add(id);
            }
        }

        #endregion

        #region --- Runtime storage ---

        // 运行时使用的掩码（不直接在 Inspector 修改）
        [SerializeField, HideInInspector] private StateMask64 _immunityMask;
        [SerializeField, HideInInspector] private StateMask64 _vulnerableMask;

        // 活动状态 & 通道
        private readonly Dictionary<StateId, StateEntry> _active = new(16);
        private readonly Dictionary<Channel, ChannelLock> _ch = new(4);
        private int _nextHandle = 1;

        // 复用临时列表，减少 GC
        private readonly List<StateId> _tmpStates = new(8);
        private readonly List<Channel> _tmpCh = new(4);

        #endregion

        #region --- Unity lifecycle ---

        void Awake()
        {
            // 获取 Bus（避免 GetComponent<EventBus>()）
            if (Bus == null)
            {
                var host = GetComponent<EventBusHost>();
                Bus = (host ? host.Bus : null) ?? XSkillInstaller.Bus ?? EventBusHost.GetOrCreateGlobal();
            }

            RebuildMasks();
        }

        void Update()
        {
            float dt = Time.deltaTime;

            // 状态计时
            _tmpStates.Clear();
            foreach (var kv in _active)
            {
                var e = kv.Value;
                if (e.TimeLeft > 0f)
                {
                    e.TimeLeft -= dt;
                    if (e.TimeLeft <= 0f) _tmpStates.Add(kv.Key);
                }
            }

            foreach (var stateId in _tmpStates)
                Remove(stateId, allStacks: true);

            // 通道超时（很少用，通常交由装饰器释放）
            _tmpCh.Clear();
            foreach (var kv in _ch)
            {
                var c = kv.Value;
                if (c.TimeLeft > 0f)
                {
                    c.TimeLeft -= dt;
                    if (c.TimeLeft <= 0f) _tmpCh.Add(kv.Key);
                }
            }

            foreach (var channel in _tmpCh)
                ReleaseChannel(channel, reason: InterruptReason.ExternalCancel);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            RebuildMasks();
        }
#endif

        private void RebuildMasks()
        {
            _immunityMask.Clear();
            _immunityMask.MergeFrom(ImmuneTo);
            _vulnerableMask.Clear();
            _vulnerableMask.MergeFrom(VulnerableTo);
        }

        #endregion

        #region --- State APIs ---

        public bool Has(StateId id) => _active.ContainsKey(id);
        public bool IsImmune(StateId id) => _immunityMask.Has(id);
        public bool IsVulnerable(StateId id) => _vulnerableMask.Has(id);

        /// <summary>
        /// 应用状态：支持叠层与刷新。
        /// respectImmunity = true 时，会检查免疫掩码（通过 ImmuneTo 列表合成）。
        /// </summary>
        public void Apply(StateId id, float duration, int stacks = 1, bool respectImmunity = true, bool refresh = true)
        {
            if (id == StateId.None) return;
            if (respectImmunity && IsImmune(id)) return;

            stacks = Mathf.Max(1, stacks);
            if (_active.TryGetValue(id, out var e))
            {
                e.Stacks += stacks;
                if (refresh) e.TimeLeft = duration > 0 ? duration : e.TimeLeft;
            }
            else
            {
                _active[id] = new StateEntry { Id = id, Stacks = stacks, TimeLeft = duration };
            }

            Bus?.Publish(new EV_StateApplied(gameObject, id));

            // 常见：某些状态会触发打断（按位检查）
            if ((id & (StateId.Stunned | StateId.KnockedUp)) != 0)
            {
                InterruptChannels(Channel.Cast, InterruptReason.StateApplied, id);
                InterruptChannels(Channel.Action, InterruptReason.StateApplied, id);
            }
            else if ((id & StateId.Silenced) != 0)
            {
                InterruptChannels(Channel.Cast, InterruptReason.StateApplied, id);
            }
            else if ((id & StateId.Rooted) != 0)
            {
                InterruptChannels(Channel.Move, InterruptReason.StateApplied, id);
            }
        }

        public void Remove(StateId id, bool allStacks = false)
        {
            if (!_active.TryGetValue(id, out var e)) return;

            if (!allStacks && e.Stacks > 1) e.Stacks--;
            else _active.Remove(id);

            Bus?.Publish(new EV_StateRemoved(gameObject, id));
        }

        /// <summary>
        /// 驱散：按掩码移除所有命中的状态位。
        /// </summary>
        public void Dispel(StateId mask)
        {
            _tmpStates.Clear();
            foreach (var kv in _active.Where(kv => (kv.Key & mask) != 0))
                _tmpStates.Add(kv.Key);
            foreach (var stateId in _tmpStates)
                Remove(stateId, allStacks: true);
        }

        #endregion

        #region --- Channel APIs ---

        public bool IsChannelFree(Channel ch, int minPriority = int.MinValue)
        {
            if (!_ch.TryGetValue(ch, out var ck)) return true;
            return ck.Priority <= minPriority;
        }

        /// <summary>
        /// 占用通道；成功返回 handle（>0），失败返回 0；若优先级高则抢占并触发打断。
        /// </summary>
        public int AcquireChannel(Channel ch, int priority, float timeout = 0f, string ownerName = null)
        {
            if (_ch.TryGetValue(ch, out var cur))
            {
                if (priority > cur.Priority)
                {
                    var old = cur.Handle;
                    var handle = ++_nextHandle;
                    _ch[ch] = new ChannelLock
                    {
                        Channel = ch, Handle = handle, Priority = priority, TimeLeft = timeout, OwnerName = ownerName
                    };

                    Bus?.Publish(new EV_ChannelPreempt(gameObject, ch, old, handle));
                    Bus?.Publish(new EV_Interrupt(gameObject, InterruptReason.ChannelPreempt, ch, StateId.None));
                    return handle;
                }

                return 0;
            }
            else
            {
                var handle = ++_nextHandle;
                _ch[ch] = new ChannelLock
                    { Channel = ch, Handle = handle, Priority = priority, TimeLeft = timeout, OwnerName = ownerName };
                Bus?.Publish(new EV_ChannelAcquired(gameObject, ch, handle, priority));
                return handle;
            }
        }

        public bool ReleaseChannel(Channel ch, int handle = 0, InterruptReason reason = InterruptReason.ExternalCancel)
        {
            if (!_ch.TryGetValue(ch, out var cur)) return false;
            if (handle == 0 || handle == cur.Handle)
            {
                _ch.Remove(ch);
                Bus?.Publish(new EV_ChannelReleased(gameObject, ch, cur.Handle));
                if (reason != InterruptReason.ExternalCancel)
                    Bus?.Publish(new EV_Interrupt(gameObject, reason, ch, StateId.None));
                return true;
            }

            return false;
        }

        void InterruptChannels(Channel ch, InterruptReason reason, StateId byState)
        {
            if (_ch.TryGetValue(ch, out var cur))
            {
                _ch.Remove(ch);
                Bus?.Publish(new EV_ChannelPreempt(gameObject, ch, cur.Handle, 0));
                Bus?.Publish(new EV_Interrupt(gameObject, reason, ch, byState));
            }
        }

        #endregion
    }
}