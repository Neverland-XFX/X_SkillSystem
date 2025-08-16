using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Numbers/ComputeDamage")]
    public sealed class ComputeDamageNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public ComputeDamageConfig Config;
    }
}