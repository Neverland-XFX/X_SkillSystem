using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Composite/Selector")]
    public sealed class SelectorNode : BTNodeBase
    {
        [LabelText("Children"), Output(dynamicPortList = true)]
        [ListDrawerSettings(DraggableItems = true, DefaultExpandedState = true)]
        public List<BTNodeBase> Children = new();
    }
}