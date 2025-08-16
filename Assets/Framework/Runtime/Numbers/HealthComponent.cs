using UnityEngine;

namespace XSkillSystem
{
    [DisallowMultipleComponent]
    public sealed class HealthComponent : MonoBehaviour, IHealth
    {
        public float MaxHP = 200;
        public float HP = 200;
        public EventBus Bus;

        void Awake()
        {
            Bus = Bus ?? GetComponent<EventBusHost>()?.Bus ?? XSkillInstaller.Bus ?? EventBusHost.GetOrCreateGlobal();
            ;
            if (HP <= 0) HP = MaxHP;
        }

        public void TakeDamage(float amount, DamageType type, bool isCrit)
        {
            if (GetComponent<StateMachine>()?.Has(StateId.Invulnerable) == true) return;
            HP = Mathf.Max(0, HP - amount);
            Bus?.Publish(new EV_Log($"{name} HP {HP:F0}/{MaxHP:F0}"));
            if (HP <= 0)
            {
                /* 死亡处理/事件 */
            }
        }
    }
}