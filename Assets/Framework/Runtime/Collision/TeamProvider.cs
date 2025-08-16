using UnityEngine;

namespace XSkillSystem
{
    public sealed class TeamProvider : MonoBehaviour, ITeamProvider
    {
        public int TeamId { get; set; } = 1;
    }
}