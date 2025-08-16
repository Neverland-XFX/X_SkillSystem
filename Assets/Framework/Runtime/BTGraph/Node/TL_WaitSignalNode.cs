using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Timeline/WaitSignal")]
    public sealed class TL_WaitSignalNode : BTNodeBase
    {
#if ODIN_INSPECTOR
        [InlineEditor(InlineEditorObjectFieldModes.Boxed), LabelText("Config")]
#endif
        public WaitSignalConfig Config;
    }
}