using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace XSkillSystem
{
    [DisallowMultipleComponent]
    public sealed class TimelineRunner : MonoBehaviour
    {
        [Header("Refs")] public PlayableDirector Director;
        public EventBus Bus;
        public GameObject Caster;

        [Header("Common Bindings (fallback)")] public Animator Animator;
        public Transform RootTransform;
        public AudioSource Audio;
        public TimelineSignalForwarder Forwarder;

        // 自动按轨道类型绑定
        [Header("Options")] public bool AutoBindByType = true;

        // 自动将所有 SignalTrack 绑定到 Forwarder
        public bool AutoBindSignals = true;

        public string CurrentId { get; private set; }

        void Awake()
        {
            if (!Director) Director = GetComponent<PlayableDirector>();
            Bus ??= GetComponent<EventBusHost>()?.Bus ?? XSkillInstaller.Bus ?? EventBusHost.GetOrCreateGlobal();
            
            if (!Animator) Animator = GetComponentInChildren<Animator>();
            if (!RootTransform) RootTransform = transform;
            if (!Audio) Audio = GetComponentInChildren<AudioSource>();
            if (!Forwarder)
            {
                Forwarder = GetComponent<TimelineSignalForwarder>() ??
                            gameObject.AddComponent<TimelineSignalForwarder>();
                Forwarder.Director = Director;
                Forwarder.Bus = Bus;
            }
        }

        public void Play(TimelineDef def, Dictionary<string, Object> overrideBindings = null,
            double startTime = 0.0, double timeScale = 1.0)
        {
            if (!def || !def.Asset || !Director) return;

            CurrentId = def.Id;
            Director.playableAsset = def.Asset;
            Director.time = startTime;

            // 默认绑定（按 TrackName）
            if (def.DefaultBindings != null)
            {
                foreach (var b in def.DefaultBindings)
                {
                    if (string.IsNullOrEmpty(b.TrackName)) continue;
                    var track = FindTrackByName(def.Asset, b.TrackName);
                    if (track) Director.SetGenericBinding(track, b.Default ? b.Default : GuessBinding(track));
                }
            }

            // 运行时覆盖
            if (overrideBindings != null)
            {
                foreach (var kv in overrideBindings)
                {
                    var track = FindTrackByName(def.Asset, kv.Key);
                    if (track && kv.Value) Director.SetGenericBinding(track, kv.Value);
                }
            }

            // 自动按类型绑定（没有绑定的轨道尝试绑定）
            if (AutoBindByType)
            {
                foreach (var track in def.Asset.GetOutputTracks())
                {
                    if (!track) continue;
                    if (Director.GetGenericBinding(track) != null) continue;
                    var guess = GuessBinding(track);
                    if (guess) Director.SetGenericBinding(track, guess);
                }
            }

            // 强制绑定所有 SignalTrack 到 Forwarder
            if (AutoBindSignals && Forwarder)
            {
                foreach (var track in def.Asset.GetOutputTracks())
                {
                    if (track is SignalTrack)
                    {
                        var cur = Director.GetGenericBinding(track);
                        if (cur == null || cur as Object != Forwarder.gameObject)
                            Director.SetGenericBinding(track, Forwarder.gameObject);
                    }
                }
            }

            // Forwarder 带上 Id
            Forwarder.TimelineId = def.Id;

            Director.Play();
            SetSpeed(timeScale);
            Bus?.Publish(new EV_TL_Play(gameObject, def.Id));
            
            //TODO:测试发信号
            Task.Run(async () =>
            {
                await Task.Delay(500);
                Bus?.Publish(new EV_TL_Signal());
            });
        }

        public void Stop()
        {
            if (Director && Director.playableGraph.IsValid())
            {
                Director.Stop();
                if (!string.IsNullOrEmpty(CurrentId))
                    Bus?.Publish(new EV_TL_Stopped(gameObject, CurrentId));
                CurrentId = null;
            }
        }

        public bool IsPlaying() => Director && Director.state == PlayState.Playing;

        public void SetSpeed(double speed)
        {
            if (!Director) return;
            var root = Director.playableGraph.GetRootPlayable(0);
            if (root.IsValid()) root.SetSpeed(speed);
        }

        static TrackAsset FindTrackByName(TimelineAsset asset, string name)
        {
            if (!asset || string.IsNullOrEmpty(name)) return null;
            foreach (var track in asset.GetOutputTracks())
                if (track && track.name == name)
                    return track;
            return null;
        }

        // 根据轨道类型猜一个合理的绑定对象
        Object GuessBinding(TrackAsset track)
        {
            if (!track) return null;
            return track switch
            {
                AnimationTrack => Animator ? Animator.gameObject : (Object)gameObject,
                AudioTrack => Audio ? (Object)Audio : gameObject,
                ActivationTrack => gameObject,
                ControlTrack => gameObject,
                SignalTrack => Forwarder ? (Object)Forwarder.gameObject : gameObject,
                _ => Caster ? (Object)Caster : (Object)(RootTransform ? RootTransform.gameObject : gameObject)
            };
            // 其他自定义轨道：尝试 Caster、Root
        }
    }
}