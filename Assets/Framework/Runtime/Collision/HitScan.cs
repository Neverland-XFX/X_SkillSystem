using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    public static class HitScan
    {
        static readonly RaycastHit[] _rayBuf = new RaycastHit[128];

        public static GameObject Ray(Vector3 origin, Vector3 dir, float distance, TargetingRule rule, GameObject caster,
            out RaycastHit hit)
        {
            int n = Physics.RaycastNonAlloc(origin, dir, _rayBuf, distance, rule?.Layers ?? ~0);
            float best = float.MaxValue;
            GameObject bestGo = null;
            hit = default;
            for (int i = 0; i < n; i++)
            {
                var h = _rayBuf[i];
                var go = h.rigidbody ? h.rigidbody.gameObject : h.collider.gameObject;
                if (!TargetFilter.Pass(caster, go, rule, dir)) continue;
                if (h.distance < best)
                {
                    best = h.distance;
                    bestGo = go;
                    hit = h;
                }
            }

            return bestGo;
        }
    }
}