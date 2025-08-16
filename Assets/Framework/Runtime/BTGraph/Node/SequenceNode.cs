using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Composite/Sequence")]
    public sealed class SequenceNode : BTNodeBase
    {
        [LabelText("Children"), Output(dynamicPortList = true)]
        [ListDrawerSettings(DraggableItems = true, DefaultExpandedState = true)]
        public List<BTNodeBase> Children = new();
    }
}