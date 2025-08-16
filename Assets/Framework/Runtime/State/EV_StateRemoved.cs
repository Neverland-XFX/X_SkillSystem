namespace XSkillSystem
{
    public readonly struct EV_StateRemoved
    {
        public readonly UnityEngine.GameObject Owner;
        public readonly StateId Removed;

        public EV_StateRemoved(UnityEngine.GameObject o, StateId r)
        {
            Owner = o;
            Removed = r;
        }
    }
}