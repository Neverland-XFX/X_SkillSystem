using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Decorator/Timeout")]
    public sealed class TimeoutNode : BTNodeBase
    {
        [Output(connectionType: ConnectionType.Override), LabelText("Child")]
        public BTNodeBase Child;

        [Min(0f), LabelText("Seconds")] public float Seconds = 2f;
    }
}