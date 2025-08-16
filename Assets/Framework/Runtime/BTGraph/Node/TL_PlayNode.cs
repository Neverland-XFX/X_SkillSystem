using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Timeline/Play")]
    public sealed class TL_PlayNode : BTNodeBase
    {
#if ODIN_INSPECTOR
        [InlineEditor(InlineEditorObjectFieldModes.Boxed), LabelText("Config")]
#endif
        public PlayTimelineConfig Config;
    }
}