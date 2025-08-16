using System;
using UnityEngine;

namespace XSkillSystem
{
    [Serializable]
    public sealed class LogConfig : ScriptableObject
    {
        [TextArea] public string Message = "Hello from Action";
    }
}