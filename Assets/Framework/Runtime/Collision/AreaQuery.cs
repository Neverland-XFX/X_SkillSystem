using System.Collections.Generic;
using UnityEngine;

namespace XSkillSystem
{
    public static class AreaQuery
    {
        static readonly Collider[] _buf3D = new Collider[256];

        public static int Sphere(Vector3 center, float radius, TargetingRule rule, GameObject caster, Vector3 forward,
            List<GameObject> outList)
        {
            int n = Physics.OverlapSphereNonAlloc(center, radius, _buf3D, rule?.Layers ?? ~0);
            return Collect(_buf3D, n, rule, caster, forward, outList);
        }

        public static int Capsule(Vector3 p0, Vector3 p1, float radius, TargetingRule rule, GameObject caster,
            Vector3 forward, List<GameObject> outList)
        {
            int n = Physics.OverlapCapsuleNonAlloc(p0, p1, radius, _buf3D, rule?.Layers ?? ~0);
            return Collect(_buf3D, n, rule, caster, forward, outList);
        }

        public static int Box(Vector3 center, Vector3 halfExt, Quaternion rot, TargetingRule rule, GameObject caster,
            Vector3 forward, List<GameObject> outList)
        {
            int n = Physics.OverlapBoxNonAlloc(center, halfExt, _buf3D, rot, rule?.Layers ?? ~0);
            return Collect(_buf3D, n, rule, caster, forward, outList);
        }

        // Cone：先用球体粗取，再按角度与距离二次筛
        public static int Cone(Vector3 origin, Vector3 forward, float radius, float halfAngleDeg, TargetingRule rule,
            GameObject caster, List<GameObject> outList)
        {
            int n = Physics.OverlapSphereNonAlloc(origin, radius, _buf3D, rule?.Layers ?? ~0);
            outList.Clear();
            float cosMin = Mathf.Cos(halfAngleDeg * Mathf.Deg2Rad);
            for (int i = 0; i < n; i++)
            {
                var col = _buf3D[i];
                if (col == null) continue;
                var go = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
                if (!TargetFilter.Pass(caster, go, rule, forward)) continue;

                var dir = (go.transform.position - origin).normalized;
                if (Vector3.Dot(forward, dir) >= cosMin) outList.Add(go);
            }

            return FinalizeSort(caster, outList, rule);
        }

        static int Collect(Collider[] arr, int n, TargetingRule rule, GameObject caster, Vector3 forward,
            List<GameObject> outList)
        {
            outList.Clear();
            for (int i = 0; i < n; i++)
            {
                var col = arr[i];
                if (col == null) continue;
                var go = col.attachedRigidbody ? col.attachedRigidbody.gameObject : col.gameObject;
                if (TargetFilter.Pass(caster, go, rule, forward)) outList.Add(go);
            }

            return FinalizeSort(caster, outList, rule);
        }

        static int FinalizeSort(GameObject caster, List<GameObject> list, TargetingRule rule)
        {
            if (rule == null) return list.Count;
            var pos = caster.transform.position;

            switch (rule.Sort)
            {
                case SortMode.Nearest:
                    list.Sort((a, b) =>
                        Vector3.SqrMagnitude(a.transform.position - pos)
                            .CompareTo(Vector3.SqrMagnitude(b.transform.position - pos)));
                    break;
                case SortMode.Farthest:
                    list.Sort((a, b) =>
                        Vector3.SqrMagnitude(b.transform.position - pos)
                            .CompareTo(Vector3.SqrMagnitude(a.transform.position - pos)));
                    break;
                case SortMode.Random:
                    for (int i = list.Count - 1; i > 0; i--)
                    {
                        int j = Random.Range(0, i + 1);
                        (list[i], list[j]) = (list[j], list[i]);
                    }

                    break;
            }

            if (rule.MaxCount > 0 && list.Count > rule.MaxCount)
                list.RemoveRange(rule.MaxCount, list.Count - rule.MaxCount);
            return list.Count;
        }
    }
}