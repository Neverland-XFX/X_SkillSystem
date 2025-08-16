namespace XSkillSystem
{
    public readonly struct EV_ChannelAcquired
    {
        public readonly UnityEngine.GameObject Owner;
        public readonly Channel Channel;
        public readonly int Handle;
        public readonly int Priority;

        public EV_ChannelAcquired(UnityEngine.GameObject o, Channel ch, int h, int p)
        {
            Owner = o;
            Channel = ch;
            Handle = h;
            Priority = p;
        }
    }
}