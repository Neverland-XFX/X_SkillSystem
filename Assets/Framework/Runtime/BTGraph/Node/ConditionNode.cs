using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Leaf/Condition")]
    public sealed class ConditionNode : BTNodeBase
    {
        [LabelText("ConditionId")] public string ConditionId = "HasTarget";

        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden), HideLabel, ShowIf("@UserData!=null")]
        public ScriptableObject UserData; // 可选：节点自带配置
    }
}