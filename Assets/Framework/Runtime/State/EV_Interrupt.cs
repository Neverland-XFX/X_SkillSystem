namespace XSkillSystem
{
    public readonly struct EV_Interrupt
    {
        public readonly UnityEngine.GameObject Owner;
        public readonly InterruptReason Reason;
        public readonly Channel Channel;
        public readonly StateId State;

        public EV_Interrupt(UnityEngine.GameObject o, InterruptReason r, Channel ch, StateId st)
        {
            Owner = o;
            Reason = r;
            Channel = ch;
            State = st;
        }
    }
}