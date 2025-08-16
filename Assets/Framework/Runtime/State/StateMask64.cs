using System;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 最多 64 位状态位掩码。
    /// </summary>
    [Serializable]
    public struct StateMask64
    {
        [SerializeField] private ulong _bits;
        public ulong Bits { get => _bits; set => _bits = value; }
        public void Clear() => _bits = 0ul;
        public bool Has(StateId id) => ((_bits >> (int)id) & 1ul) != 0ul;
        public void Set(StateId id, bool on = true)
        {
            int i = (int)id;
            if (i < 0 || i > 63) return;
            if (on) _bits |=  (1ul << i);
            else    _bits &= ~(1ul << i);
        }
        public void MergeFrom(System.Collections.Generic.IEnumerable<StateId> list, bool on = true)
        {
            if (list == null) return;
            foreach (var id in list) Set(id, on);
        }
        public override string ToString() => $"0x{_bits:X16}";
    }
}