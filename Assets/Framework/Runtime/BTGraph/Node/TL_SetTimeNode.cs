using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Timeline/SetTime")]
    public sealed class TL_SetTimeNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public SetTLTimeConfig Config;
    }
}