using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Collision/SpawnProjectile")]
    public sealed class SpawnProjectileNode : BTNodeBase
    {
        [Required, InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        public SpawnProjectileConfig Config;
    }
}