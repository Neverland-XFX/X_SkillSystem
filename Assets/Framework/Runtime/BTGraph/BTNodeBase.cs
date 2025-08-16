using System.Linq;
using UnityEngine;
using XNode;
using Sirenix.OdinInspector;

namespace XSkillSystem
{
    /// <summary>
    /// BT 节点基类：提供稳定 GUID、通用输入端口、Odin 友好显示。
    /// </summary>
    public abstract class BTNodeBase : Node, ISerializationCallbackReceiver
    {
        // 给所有节点一个输入端口，供 Composite 的 Children 输出连入
        [Input(connectionType: ConnectionType.Override)] [LabelText("In"), GUIColor(0.75f, 0.9f, 1f)]
        public BTNodeBase In;

        [SerializeField, HideInInspector] private string _guid;

        // [ShowInInspector, ReadOnly, LabelText("Guid")]
        public string Guid => _guid;

        // [ShowInInspector, ReadOnly, LabelText("RuntimeName")]
        public virtual string RuntimeName => $"{GetType().Name}#{ShortGuid}";

        // [ShowInInspector, ReadOnly, LabelText("Short")]
        public string ShortGuid => !string.IsNullOrEmpty(_guid) && _guid.Length >= 8 ? _guid[..8] : "00000000";

        protected override void OnEnable() => EnsureGuidUnique();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize() => EnsureGuidUnique();

//         [Button(ButtonSizes.Small), GUIColor(0.9f,0.5f,0.5f)]
//         private void RegenerateGUID()
//         {
//             _guid = System.Guid.NewGuid().ToString("N");
// #if UNITY_EDITOR
//             UnityEditor.EditorUtility.SetDirty(this);
// #endif
//         }

        // 所有 BT 控制流节点不提供“端口值”，统一返回 null，避免编辑器警告
        public override object GetValue(NodePort port) => null;

        [Button(ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
        void RegenerateGuid()
        {
            _guid = System.Guid.NewGuid().ToString("N");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void EnsureGuidUnique()
        {
            if (string.IsNullOrEmpty(_guid))
            {
                _guid = System.Guid.NewGuid().ToString("N");
                return;
            }

            if (graph == null) return;
            var list = graph.nodes;
            foreach (var node in list.Where(node => !ReferenceEquals(node, this)))
            {
                if (node is not BTNodeBase other || other._guid != _guid) continue;
                _guid = System.Guid.NewGuid().ToString("N");
                break;
            }
        }
    }
}