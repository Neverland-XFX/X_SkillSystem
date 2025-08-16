using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Collision/AttachAura")]
    public sealed class AttachAuraNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public AttachAuraConfig Config;
    }
}