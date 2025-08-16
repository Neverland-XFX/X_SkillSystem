using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Composite/ForEachTargets")]
    public sealed class ForEachTargetsNode : BTNodeBase
    {
        [Output(connectionType: ConnectionType.Override), LabelText("Child")]
        public BTNodeBase Child;

        [LabelText("Break On First Success")] public bool BreakOnFirstSuccess = false;
    }
}