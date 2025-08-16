using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/SubTree")]
    public sealed class SubTreeNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public BTGraph Graph;

        [LabelText("Inherit Tracer")] public bool InheritTracer = true;
    }
}