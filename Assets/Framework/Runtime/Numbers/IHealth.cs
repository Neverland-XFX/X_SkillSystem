namespace XSkillSystem
{
    public interface IHealth
    {
        void TakeDamage(float amount, DamageType type, bool isCrit);
    }
}