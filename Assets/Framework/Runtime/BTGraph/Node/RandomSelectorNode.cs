using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Composite/RandomSelector")]
    public sealed class RandomSelectorNode : BTNodeBase
    {
        [LabelText("Children"), Output(dynamicPortList = true)]
        [ListDrawerSettings(DraggableItems = true, DefaultExpandedState = true,
            CustomRemoveIndexFunction = nameof(RemoveAt))]
        public List<BTNodeBase> Children = new();

        [LabelText("Weights"), Tooltip("与 Children 一一对应，不足默认1")]
        [ListDrawerSettings(DraggableItems = true, DefaultExpandedState = true)]
        public List<float> Weights = new();

        void RemoveAt(int index)
        {
            if (index >= 0 && index < Weights.Count) Weights.RemoveAt(index);
            if (index >= 0 && index < Children.Count) Children.RemoveAt(index);
        }

        private void OnValidate()
        {
            while (Weights.Count < Children.Count) Weights.Add(1f);
            while (Weights.Count > Children.Count && Weights.Count > 0) Weights.RemoveAt(Weights.Count - 1);
        }
    }
}