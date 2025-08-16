namespace XSkillSystem
{
    public readonly struct EV_StateApplied
    {
        public readonly UnityEngine.GameObject Owner;
        public readonly StateId Added;

        public EV_StateApplied(UnityEngine.GameObject o, StateId a)
        {
            Owner = o;
            Added = a;
        }
    }
}