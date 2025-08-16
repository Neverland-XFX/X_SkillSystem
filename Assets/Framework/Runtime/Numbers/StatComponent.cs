using UnityEngine;

namespace XSkillSystem
{
    [DisallowMultipleComponent]
    public sealed class StatComponent : MonoBehaviour, IStatWritable, IStatProvider
    {
        public float BaseAttack = 10,
            BaseSpell = 30,
            CritChance = 0.2f,
            CritDamage = 1.5f,
            Armor = 10,
            ResistFire = 0f,
            Level = 1;

        readonly StatBlock _stats = new();

        void Awake()
        {
            _stats.SetBase((int)StatId.Attack, BaseAttack);
            _stats.SetBase((int)StatId.SpellPower, BaseSpell);
            _stats.SetBase((int)StatId.CritChance, CritChance);
            _stats.SetBase((int)StatId.CritDamage, CritDamage);
            _stats.SetBase((int)StatId.Armor, Armor);
            _stats.SetBase((int)StatId.Resist_Fire, ResistFire);
            _stats.SetBase((int)StatId.Level, Level);
        }

        public float GetStat(int id) => _stats.GetStat(id);
        public void SetBase(int id, float v) => _stats.SetBase(id, v);
        public void AddModifier(StatModifier m) => _stats.AddModifier(m);
        public void RemoveModifier(StatModifier m) => _stats.RemoveModifier(m);
        public void ClearModifiers() => _stats.ClearModifiers();
    }
}