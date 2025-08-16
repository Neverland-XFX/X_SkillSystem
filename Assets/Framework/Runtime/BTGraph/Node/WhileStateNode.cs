using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/State/WhileState")]
    public sealed class WhileStateNode : BTNodeBase
    {
        [Output(connectionType: ConnectionType.Override), LabelText("Child")]
        public BTNodeBase Child;

        [LabelText("Required States")] public StateMask Required;
        [LabelText("Forbidden States")] public StateMask Forbidden;
    }
}