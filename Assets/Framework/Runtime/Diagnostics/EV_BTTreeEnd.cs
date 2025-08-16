namespace XSkillSystem
{
    public readonly struct EV_BTTreeEnd
    {
        public readonly BTTreeInfo Tree;
        public readonly byte Status;

        public EV_BTTreeEnd(BTTreeInfo t, byte s)
        {
            Tree = t;
            Status = s;
        }
    }
}