using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Root")]
    public sealed class RootNode : BTNodeBase
    {
        [Output(connectionType: ConnectionType.Override), LabelText("Child")]
        public BTNodeBase Child;

        public override string RuntimeName => $"Root#{ShortGuid}";
    }
}