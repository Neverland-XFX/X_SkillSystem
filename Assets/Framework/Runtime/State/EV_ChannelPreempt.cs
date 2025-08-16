namespace XSkillSystem
{
    public readonly struct EV_ChannelPreempt
    {
        public readonly UnityEngine.GameObject Owner;
        public readonly Channel Channel;
        public readonly int OldHandle;
        public readonly int NewHandle;

        public EV_ChannelPreempt(UnityEngine.GameObject o, Channel ch, int oldH, int newH)
        {
            Owner = o;
            Channel = ch;
            OldHandle = oldH;
            NewHandle = newH;
        }
    }
}