using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Timeline/Stop")]
    public sealed class TL_StopNode : BTNodeBase
    {
#if ODIN_INSPECTOR
        [InlineEditor(InlineEditorObjectFieldModes.Boxed), LabelText("Config")]
#endif
        public StopTimelineConfig Config; //可为空
    }
}