namespace XSkillSystem
{
    // 节点中断
    public readonly struct EV_BTNodeAbort
    {
        public readonly BTTreeInfo Tree;
        public readonly string NodeName;
        public readonly int Frame;
        public readonly double Time;

        public EV_BTNodeAbort(BTTreeInfo tree, string nodeName, int frame, double time)
        {
            Tree = tree;
            NodeName = nodeName;
            Frame = frame;
            Time = time;
        }
    }
}