using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    public sealed class EventHud : MonoBehaviour
    {
        public EventBusHost BusHost;
        EventBus _bus;
        readonly Queue<string> _lines = new Queue<string>();
        int _t1, _t2, _t3, _t4;

        void Start()
        {
            _bus = (BusHost ? BusHost.Bus : null) ?? XSkillInstaller.Bus ?? EventBusHost.GetOrCreateGlobal();
            _t1 = _bus.Subscribe<EV_Log>(e => Add(e.Message));
            _t2 = _bus.Subscribe<EV_ProjectileHit>(e => Add($"Hit {e.Info.Target.name}"));
            _t3 = _bus.Subscribe<EV_DamageApplied>(e =>
                Add($"Damage {e.Amount:F0}{(e.IsCrit ? " CRIT" : "")} → {e.Target.name}"));
            _t4 = _bus.Subscribe<EV_TL_Signal>(e => Add($"Signal {e.SignalName}"));
        }

        void Add(string s)
        {
            _lines.Enqueue(s);
            while (_lines.Count > 12) _lines.Dequeue();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 480, 320), GUI.skin.box);
            foreach (var l in _lines) GUILayout.Label(l);
            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<EV_Log>(_t1);
            _bus.Unsubscribe<EV_ProjectileHit>(_t2);
            _bus.Unsubscribe<EV_DamageApplied>(_t3);
            _bus.Unsubscribe<EV_TL_Signal>(_t4);
        }
    }
}