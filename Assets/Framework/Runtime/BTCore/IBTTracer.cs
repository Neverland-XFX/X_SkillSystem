namespace XSkillSystem
{
    /// <summary>
    /// 诊断追踪：可用于可视化与日志。
    /// </summary>
    public interface IBTTracer
    {
        void Enter(string nodeName);
        void Exit(string nodeName, BTStatus status);
        void Abort(string nodeName);
    }
}