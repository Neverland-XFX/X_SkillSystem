using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Decorator/Repeat")]
    public sealed class RepeatNode : BTNodeBase
    {
        [Output(connectionType: ConnectionType.Override), LabelText("Child")]
        public BTNodeBase Child;

        [Min(1), LabelText("Count")] public int Count = 2;
        [LabelText("Break On Failure")] public bool BreakOnFailure = false;
    }
}