using UnityEngine;

namespace XSkillSystem
{
    public sealed class ProjectilePool : MonoBehaviour
    {
        public ProjectileRunner Prefab;
        readonly System.Collections.Generic.Stack<ProjectileRunner> _stack = new();

        public ProjectileRunner Get()
        {
            if (_stack.Count > 0)
            {
                var p = _stack.Pop();
                p.gameObject.SetActive(true);
                return p;
            }

            return Instantiate(Prefab);
        }

        public void Return(ProjectileRunner pr)
        {
            pr.gameObject.SetActive(false);
            _stack.Push(pr);
        }
    }
}