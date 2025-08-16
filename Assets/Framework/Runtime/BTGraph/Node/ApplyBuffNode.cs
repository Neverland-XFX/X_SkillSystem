using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Buff/ApplyBuff")]
    public sealed class ApplyBuffNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public ApplyBuffConfig Config;
    }
}