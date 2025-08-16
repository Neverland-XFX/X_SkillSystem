using System;
using UnityEngine;

namespace XSkillSystem
{
    [Serializable]
    public sealed class SetBBFloatConfig : ScriptableObject
    {
        public string Key = "TempFloat";
        public float Value = 1f;
        // 是否用 NodeLocal 命名空间
        public bool LocalToNode = false;
    }
}