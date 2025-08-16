namespace XSkillSystem
{
    public readonly struct EV_ChannelReleased
    {
        public readonly UnityEngine.GameObject Owner;
        public readonly Channel Channel;
        public readonly int Handle;

        public EV_ChannelReleased(UnityEngine.GameObject o, Channel ch, int h)
        {
            Owner = o;
            Channel = ch;
            Handle = h;
        }
    }
}