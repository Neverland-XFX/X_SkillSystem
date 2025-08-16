using System;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 一个简单的“联合类型”用于序列化默认值。
    /// </summary>
    [Serializable]
    public struct BlackboardValue
    {
        public enum Kind : byte { None, Bool, Int, Float, String, Vector3, Object }
        public Kind Type;
        public bool Bool;
        public int Int;
        public float Float;
        public string String;
        public Vector3 Vector3;
        public UnityEngine.Object Object;

        public static BlackboardValue From<T>(T v)
        {
            var t = typeof(T);
            if (t == typeof(bool)) return new BlackboardValue { Type = Kind.Bool, Bool = Convert.ToBoolean(v) };
            if (t == typeof(int)) return new BlackboardValue { Type = Kind.Int, Int = Convert.ToInt32(v) };
            if (t == typeof(float)) return new BlackboardValue { Type = Kind.Float, Float = Convert.ToSingle(v) };
            if (t == typeof(string)) return new BlackboardValue { Type = Kind.String, String = v as string };
            if (t == typeof(Vector3)) return new BlackboardValue { Type = Kind.Vector3, Vector3 = (Vector3)(object)v };
            if (typeof(UnityEngine.Object).IsAssignableFrom(t)) return new BlackboardValue { Type = Kind.Object, Object = v as UnityEngine.Object };
            return default;
        }

        public object Boxed =>
            Type switch
            {
                Kind.Bool => Bool,
                Kind.Int => Int,
                Kind.Float => Float,
                Kind.String => String,
                Kind.Vector3 => Vector3,
                Kind.Object => Object,
                _ => null
            };
    }
}