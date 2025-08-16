namespace XSkillSystem
{
    // 节点进入
    public readonly struct EV_BTNodeEnter
    {
        public readonly BTTreeInfo Tree;
        // 运行时名（含#短Guid）
        public readonly string NodeName;
        public readonly int Frame;
        public readonly double Time;

        public EV_BTNodeEnter(BTTreeInfo tree, string nodeName, int frame, double time)
        {
            Tree = tree;
            NodeName = nodeName;
            Frame = frame;
            Time = time;
        }
    }
}