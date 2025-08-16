using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/State/CastTime")]
    public sealed class CastTimeNode : BTNodeBase
    {
        [Output(connectionType: ConnectionType.Override), LabelText("Child")]
        public BTNodeBase Child;

        [LabelText("Duration (s)")] public float Duration = 1.0f;
        [LabelText("Channel")] public SkillChannel Channel = SkillChannel.Cast;
        [LabelText("Priority")] public int Priority = 10;

        [LabelText("Interrupt States")] public StateMask InterruptStates = StateMask.Stunned | StateMask.Silenced;
    }
}