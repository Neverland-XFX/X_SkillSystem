namespace XSkillSystem
{
    /// <summary>
    /// 手动推进（用于回放/服务器）。
    /// </summary>
    public sealed class ManualClock : IClock
    {
        double _t, _dt;
        public double Time => _t;
        public double DeltaTime => _dt;

        public void Step(double delta)
        {
            _dt = delta;
            _t += delta;
        }
    }
}