namespace XSkillSystem
{
    /// <summary>
    /// 一个简单的控制台追踪器，可自行替换
    /// </summary>
    public sealed class ConsoleTracer : IBTTracer
    {
        public void Enter(string nodeName) => System.Diagnostics.Debug.WriteLine($"> Enter {nodeName}");

        public void Exit(string nodeName, BTStatus status) =>
            System.Diagnostics.Debug.WriteLine($"< Exit {nodeName} = {status}");

        public void Abort(string nodeName) => System.Diagnostics.Debug.WriteLine($"! Abort {nodeName}");
    }
}