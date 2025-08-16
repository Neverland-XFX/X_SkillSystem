using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Timeline/PauseOrResume")]
    public sealed class TL_PauseResumeNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public PauseTimelineConfig Config;
    }
}