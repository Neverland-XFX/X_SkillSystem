using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Decorator/Wait")]
    public sealed class WaitNode : BTNodeBase
    {
        [Min(0f), LabelText("Seconds")] public float Seconds = 0.5f;
    }
}