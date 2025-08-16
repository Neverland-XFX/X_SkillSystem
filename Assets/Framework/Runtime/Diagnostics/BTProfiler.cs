using System.Collections.Generic;

namespace XSkillSystem
{
    public sealed class BTProfiler
    {
        public sealed class NodeStat
        {
            public string NodeName;
            public int Enters, Exits, Success, Failure, Aborts;
            // 仅按 Enter→Exit 累加
            public double TotalTime;
            // -1 表示不在执行
            public double LastEnterTime;

            public void OnEnter(double t)
            {
                Enters++;
                LastEnterTime = t;
            }

            public void OnExit(double t, byte status)
            {
                Exits++;
                if (status == 1) Success++;
                else if (status == 0) Failure++;
                if (LastEnterTime >= 0)
                {
                    TotalTime += (t - LastEnterTime);
                    LastEnterTime = -1;
                }
            }

            public void OnAbort()
            {
                Aborts++;
                LastEnterTime = -1;
            }
        }

        readonly Dictionary<string, NodeStat> _map = new(128);
        readonly EventBus _bus;
        int _tokEnter, _tokExit, _tokAbort;

        public IReadOnlyDictionary<string, NodeStat> Stats => _map;

        public BTProfiler(EventBus bus)
        {
            _bus = bus;
            _tokEnter = _bus.Subscribe<EV_BTNodeEnter>(OnEnter);
            _tokExit = _bus.Subscribe<EV_BTNodeExit>(OnExit);
            _tokAbort = _bus.Subscribe<EV_BTNodeAbort>(OnAbort);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<EV_BTNodeEnter>(_tokEnter);
            _bus.Unsubscribe<EV_BTNodeExit>(_tokExit);
            _bus.Unsubscribe<EV_BTNodeAbort>(_tokAbort);
        }

        void OnEnter(EV_BTNodeEnter e)
        {
            if (!_map.TryGetValue(e.NodeName, out var s))
                _map[e.NodeName] = s = new NodeStat { NodeName = e.NodeName, LastEnterTime = -1 };
            s.OnEnter(e.Time);
        }

        void OnExit(EV_BTNodeExit e)
        {
            if (!_map.TryGetValue(e.NodeName, out var s))
                _map[e.NodeName] = s = new NodeStat { NodeName = e.NodeName, LastEnterTime = -1 };
            s.OnExit(e.Time, e.Status);
        }

        void OnAbort(EV_BTNodeAbort e)
        {
            if (_map.TryGetValue(e.NodeName, out var s)) s.OnAbort();
        }
    }
}