using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XSkillSystem
{
    [CreateNodeMenu("BT/Composite/Parallel")]
    public sealed class ParallelNode : BTNodeBase
    {
        public enum Mode
        {
            AnySuccess,
            AllSuccess,
            Threshold
        }

        [LabelText("Children"), Output(dynamicPortList = true)]
        [ListDrawerSettings(DraggableItems = true, DefaultExpandedState = true)]
        public List<BTNodeBase> Children = new();

        [LabelText("Mode")] public Mode ModePolicy = Mode.AnySuccess;

        [Min(1), LabelText("Threshold"), ShowIf("@ModePolicy == Mode.Threshold")]
        public int Threshold = 1;

        [SerializeField, HideInInspector] private int CompletePolicy = -1;

        private void OnValidate()
        {
            if (CompletePolicy >= 0)
            {
                ModePolicy = (Mode)Mathf.Clamp(CompletePolicy, 0, 2);
                CompletePolicy = -1;
            }

            if (ModePolicy != Mode.Threshold) Threshold = Mathf.Max(1, Threshold);
        }
    }
}