using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Collision/AreaQueryToBB")]
    public sealed class AreaQueryToBBNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public AreaQueryConfig Config;
    }
}