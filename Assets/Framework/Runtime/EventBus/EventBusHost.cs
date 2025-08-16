using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 场景中的事件总线承载组件。因为 EventBus 是普通 C# 类，不能被 FindObjectOfType 直接查到，
    /// 所以通过这个 Host 来承载与持久化。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EventBusHost : MonoBehaviour
    {
        [Tooltip("承载的全局事件总线实例")] public EventBus Bus = new EventBus();

        [Tooltip("是否跨场景常驻")] public bool MakePersistent = true;

        void Awake()
        {
            if (MakePersistent) DontDestroyOnLoad(gameObject);
            // 若 Installer 尚未设置全局 Bus，则以本 Bus 为全局
            if (XSkillInstaller.Bus == null) XSkillInstaller.Bus = Bus;
        }

        /// <summary>
        /// 取得一个可用的全局 EventBus；若场景内没有 Host 会自动创建。
        /// </summary>
        public static EventBus GetOrCreateGlobal()
        {
            if (XSkillInstaller.Bus != null) return XSkillInstaller.Bus;

            var host = FindObjectOfType<EventBusHost>();
            if (host != null)
            {
                XSkillInstaller.Bus = host.Bus;
                return host.Bus;
            }

            var go = new GameObject("[EventBusHost]");
            host = go.AddComponent<EventBusHost>();
            XSkillInstaller.Bus = host.Bus;
            return host.Bus;
        }
    }
}