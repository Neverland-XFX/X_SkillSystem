#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace XSkillSystem
{
    public static class TimelineWizard
    {
        [MenuItem("XSkillSystem/Create/Skill Timeline (Standard)")]
        public static void CreateStandardSkillTimeline()
        {
            string folder = GetSelectedFolder();
            string tlName = "TL_Skill_Standard";
            string defName = "TLDef_Skill_Standard";

            // 1) 先创建 Timeline 资产并保存到磁盘（变为 persistent）
            var tl = ScriptableObject.CreateInstance<TimelineAsset>();
            tl.name = tlName;
            string tlPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{tlName}.playable");
            AssetDatabase.CreateAsset(tl, tlPath);
            AssetDatabase.SaveAssets(); // 关键：让 tl 成为 persistent

            // 2) 再创建轨道
            var animTrack = tl.CreateTrack<AnimationTrack>(null, "Anim");
            var sigTrack = tl.CreateTrack<SignalTrack>(null, "Signals");

            // 3) 再添加 Signal（SignalAsset 作为 "子资产" 加到 tl）
            AddSignal(sigTrack, tl, "Release", 0.5);
            AddSignal(sigTrack, tl, "End", 1.2);

            // 4) 创建 TimelineDef（引用刚才的 tl）
            var def = ScriptableObject.CreateInstance<TimelineDef>();
            def.name = defName;
            def.Id = "Skill.Cast";
            def.Asset = tl;
            // 默认绑定条目（运行时 Runner 会自动绑定，但保留名字方便覆盖）
            def.DefaultBindings.Add(new TimelineDef.BindingEntry { TrackName = "Anim" });
            def.DefaultBindings.Add(new TimelineDef.BindingEntry { TrackName = "Signals" });

            string defPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{defName}.asset");
            AssetDatabase.CreateAsset(def, defPath);

            // 5) 标脏 & 保存
            EditorUtility.SetDirty(tl);
            EditorUtility.SetDirty(def);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("XSkillSystem",
                $"已创建 Timeline 与 TimelineDef：\n{tlPath}\n{defPath}\n\n" +
                $"把 TimelineRunner 挂到角色上即可使用。", "OK");
        }

        /// <summary>
        /// 向指定 SignalTrack 添加一个名为 name 的 SignalEmitter，并将 SignalAsset 作为子资产添加到 tl。
        /// </summary>
        static void AddSignal(SignalTrack track, TimelineAsset tl, string name, double time)
        {
            if (track == null || tl == null) return;

            // 创建 Marker
            var emitter = track.CreateMarker<SignalEmitter>(time);
            emitter.retroactive = false;
            emitter.emitOnce = false;

            // 创建 SignalAsset，并作为“子资产”添加到 tl（确保 tl 已经 persistent）
            var sig = ScriptableObject.CreateInstance<SignalAsset>();
            sig.name = name;
            AssetDatabase.AddObjectToAsset(sig, tl); // 注意：加到 tl，而不是 track.timelineAsset
            emitter.asset = sig;

            EditorUtility.SetDirty(tl);
        }

        static string GetSelectedFolder()
        {
            var obj = Selection.activeObject;
            string path = obj ? AssetDatabase.GetAssetPath(obj) : "Assets";
            if (System.IO.File.Exists(path)) path = System.IO.Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(path) ? "Assets" : path;
        }
    }
}
#endif