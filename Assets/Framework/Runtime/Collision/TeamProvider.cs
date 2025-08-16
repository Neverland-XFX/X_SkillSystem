using UnityEngine;

namespace XSkillSystem
{
    public sealed class TeamProvider : MonoBehaviour, ITeamProvider
    {
        [SerializeField] 
        public int TeamId { get; set; } = 1;
    }
}