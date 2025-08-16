namespace XSkillSystem
{
    public readonly struct EV_ProjectileHit
    {
        public readonly HitInfo Info;
        public EV_ProjectileHit(HitInfo i) => Info = i;
    }
}