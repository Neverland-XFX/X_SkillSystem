namespace XSkillSystem
{
    /// <summary>
    /// 空实现，避免判空。
    /// </summary>
    public sealed class NullTracer : IBTTracer
    {
        public static readonly NullTracer Instance = new();

        public void Enter(string nodeName)
        {
        }

        public void Exit(string nodeName, BTStatus status)
        {
        }

        public void Abort(string nodeName)
        {
        }
    }
}