using System;

namespace XSkillSystem
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent e);
        int Subscribe<TEvent>(Action<TEvent> handler);
        void Unsubscribe<TEvent>(int token);
    }
}