using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Decorator/CastTime")]
    public sealed class CastTimeDecoratorNode : BTNodeBase
    {
        [Output(connectionType = ConnectionType.Override)]
        public BTNodeBase Child;

        [Min(0f)] public float Duration = 1.0f;
        public Channel Channel = Channel.Cast;
        public int Priority = 0;

        [Header("Interrupt on states (bitmask)")]
        public StateId InterruptStates = StateId.Stunned | StateId.Silenced | StateId.KnockedUp;
    }
}