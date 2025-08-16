using System;
using UnityEngine;

namespace XSkillSystem
{
    public class EnemyController : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<TeamProvider>().TeamId = 2;
        }
    }
}