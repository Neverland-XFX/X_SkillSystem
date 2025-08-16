namespace XSkillSystem
{
    // 树开始/结束
    public readonly struct EV_BTTreeStart
    {
        public readonly BTTreeInfo Tree;

        public EV_BTTreeStart(BTTreeInfo t)
        {
            Tree = t;
        }
    }
}