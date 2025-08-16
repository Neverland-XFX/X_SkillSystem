using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XSkillSystem
{
    /// <summary>
    /// 将 Timeline 的 SignalEmitter/Markers 通过 PlayableDirector 推送到 EventBus。
    /// 请确保：SignalTrack 绑定到挂有本脚本的 GameObject（或其 PlayableDirector）。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TimelineSignalForwarder : MonoBehaviour, INotificationReceiver
    {
        public PlayableDirector Director;
        public EventBus Bus;

        [Tooltip("用于过滤/标识的 Timeline Id（来自 TimelineDef.Id）")]
        public string TimelineId = "Timeline";

        void Awake()
        {
            if (!Director) Director = GetComponent<PlayableDirector>();
            Bus ??= GetComponent<EventBusHost>()?.Bus ?? XSkillInstaller.Bus ?? EventBusHost.GetOrCreateGlobal();
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            // 兼容任意 Marker（SignalEmitter/自定义 Marker）
            string name = notification.GetType().Name;
            if (notification is SignalEmitter se && se.asset != null)
                name = se.asset.name;
            int hash = Animator.StringToHash(name);
            Bus?.Publish(new EV_TL_Signal(gameObject, TimelineId, name, hash));
        }
    }
}