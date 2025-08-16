using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/State/ChannelGuard")]
    public sealed class ChannelGuardNode : BTNodeBase
    {
        [Output(connectionType: ConnectionType.Override), LabelText("Child")]
        public BTNodeBase Child;

        [LabelText("Channel")] public SkillChannel Channel = SkillChannel.Cast;
        [LabelText("Priority")] public int Priority = 10;
        [Min(0), LabelText("Timeout (s) 0=∞")] public float Timeout = 0f;
    }
}