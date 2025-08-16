namespace XSkillSystem
{
    // 统一的总线录制器接口
    public interface IBusRecorder
    {
        void OnPublish<TEvent>(TEvent e);
        void Clear();
    }
}