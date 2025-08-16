namespace XSkillSystem
{
    public interface IStatWritable : IStatProvider
    {
        void SetBase(int id, float value);
        void AddModifier(StatModifier mod);
        void RemoveModifier(StatModifier mod);
        void ClearModifiers();
    }
}