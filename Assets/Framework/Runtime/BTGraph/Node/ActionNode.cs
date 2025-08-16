using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Leaf/Action")]
    public sealed class ActionNode : BTNodeBase
    {
        [LabelText("ActionId")] public string ActionId = "EmitLog";

        [LabelText("UserData"), InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public ScriptableObject UserData;
    }
}