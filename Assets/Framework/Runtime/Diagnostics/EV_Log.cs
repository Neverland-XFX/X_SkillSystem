namespace XSkillSystem
{
    // 轻量日志（沿用第3步 EV_Log 亦可）
    public readonly struct EV_Log
    {
        public readonly string Message;
        public EV_Log(string m) => Message = m;
    }
}