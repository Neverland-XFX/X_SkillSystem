using System;
using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 强类型 Key，避免装箱与字符串错拼。
    /// </summary>
    [Serializable]
    public readonly struct BBKey<T>
    {
        public readonly int Id;
        public readonly string Name;

        public BBKey(string name)
        {
            Name = name ?? typeof(T).Name;
            Id = Fnv1a32(name);
        }

        public override string ToString() => $"{Name}<{typeof(T).Name}>#{Id:X8}";

        static int Fnv1a32(string s)
        {
            unchecked
            {
                const uint offset = 2166136261u;
                const uint prime = 16777619u;
                uint hash = offset;
                foreach (var c in s)
                {
                    hash ^= c;
                    hash *= prime;
                }

                return (int)hash;
            }
        }
    }
}