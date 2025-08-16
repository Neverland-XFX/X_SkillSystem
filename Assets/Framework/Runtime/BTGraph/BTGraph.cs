using System.Collections.Generic;
using UnityEngine;
using XNode;
using Sirenix.OdinInspector;

namespace XSkillSystem
{
    [CreateAssetMenu(menuName = "XSkillSystem/BT/Graph", fileName = "BTGraph")]
    public sealed class BTGraph : NodeGraph
    {
        [BoxGroup("Info"), LabelText("Graph Id"), GUIColor(0.8f, 1f, 1f)]
        public string GraphId = "Skill.Graph";

        [BoxGroup("Info"), LabelText("Version"), Min(1)]
        public int Version = 1;

        [BoxGroup("Defaults"), InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden), HideLabel,
         ShowIf("@BlackboardDefaults!=null")]
        public BlackboardAsset BlackboardDefaults;

        // —— 只读：根节点 —— //
        [ShowInInspector, ReadOnly, LabelText("Root"), PropertyOrder(100)]
        public BTNodeBase Root
        {
            get
            {
                for (int i = 0; i < nodes.Count; i++)
                    if (nodes[i] is RootNode)
                        return (BTNodeBase)nodes[i];
                return null;
            }
        }

        // —— 上次校验/编译的报告—— //
        [ShowInInspector, ReadOnly, LabelText("Last Validate Issues"), PropertyOrder(200)]
        [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, NumberOfItemsPerPage = 8)]
        public List<string> LastValidateIssues { get; private set; } = new();

        [ShowInInspector, ReadOnly, LabelText("Last Build Report"), PropertyOrder(201)]
        public BTCompileReport LastBuildReport { get; private set; } = new();

        // —— Odin 按钮 —— //
        [Button(ButtonSizes.Large), GUIColor(0.3f, 0.9f, 0.6f), PropertyOrder(20)]
        public void ValidateGraph()
        {
            LastValidateIssues.Clear();
            var ok = BTCompiler.ValidateGraph(this, LastValidateIssues);
            if (ok) LastValidateIssues.Add("✅ Graph OK");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [Button(ButtonSizes.Large), GUIColor(0.6f, 0.8f, 1f), PropertyOrder(21)]
        [InfoBox("需要 XSkillInstaller.Library 就绪。若在编辑器下未运行，仍可编译（不跑时序）。", InfoMessageType.None)]
        public void BuildPreview()
        {
            if (XSkillInstaller.Library == null)
            {
                LastBuildReport = BTCompileReport.Fail("Library 未初始化（请在场景中放置 XSkillInstaller）。");
            }
            else
            {
                LastBuildReport =
                    BTCompiler.BuildWithReport<XContext>(this, XSkillInstaller.Library, new ConsoleTracer());
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public IEnumerable<BTNodeBase> AllNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i] is BTNodeBase b)
                    yield return b;
        }
    }
}