namespace XSkillSystem
{
    // 节点退出
    public readonly struct EV_BTNodeExit
    {
        public readonly BTTreeInfo Tree;
        public readonly string NodeName;
        // BTStatus 的 byte
        public readonly byte Status;
        public readonly int Frame;
        public readonly double Time;

        public EV_BTNodeExit(BTTreeInfo tree, string nodeName, byte status, int frame, double time)
        {
            Tree = tree;
            NodeName = nodeName;
            Status = status;
            Frame = frame;
            Time = time;
        }
    }
}