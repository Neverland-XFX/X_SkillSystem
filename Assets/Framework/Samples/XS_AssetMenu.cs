#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace XSkillSystem
{
    public static class XS_AssetMenu
    {
        // 通用创建函数
        static T CreateSO<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            string folder = GetSelectedFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        static string GetSelectedFolder()
        {
            var obj = Selection.activeObject;
            string path = obj ? AssetDatabase.GetAssetPath(obj) : "Assets";
            if (System.IO.File.Exists(path)) path = System.IO.Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(path) ? "Assets" : path;
        }

        // ====== Numbers / Damage ======
        [MenuItem("Assets/Create/XSkillSystem/Numbers/NumbersConfig", false, 10)]
        static void CreateNumbers() => CreateSO<NumbersConfig>("NumbersConfig");

        [MenuItem("Assets/Create/XSkillSystem/Numbers/DamageFormulaDef", false, 11)]
        static void CreateFormula() => CreateSO<DamageFormulaDef>("DamageFormula");

        // ====== Buff ======
        [MenuItem("Assets/Create/XSkillSystem/Buff/BuffDef", false, 20)]
        static void CreateBuff() => CreateSO<BuffDef>("Buff");

        // ====== Collision / Targeting / Projectile / Aura ======
        [MenuItem("Assets/Create/XSkillSystem/Collision/TargetingRule", false, 30)]
        static void CreateTargeting() => CreateSO<TargetingRule>("TargetingRule");

        [MenuItem("Assets/Create/XSkillSystem/Collision/ProjectileDef", false, 31)]
        static void CreateProjectile() => CreateSO<ProjectileDef>("ProjectileDef");

        [MenuItem("Assets/Create/XSkillSystem/Collision/AuraDef", false, 32)]
        static void CreateAura() => CreateSO<AuraDef>("AuraDef");

        // 常用节点 UserData（行为树里拖到节点上用）
        [MenuItem("Assets/Create/XSkillSystem/Collision/AreaQueryConfig", false, 40)]
        static void CreateAreaCfg() => CreateSO<AreaQueryConfig>("AreaQueryConfig");

        [MenuItem("Assets/Create/XSkillSystem/Collision/SpawnProjectileConfig", false, 41)]
        static void CreateSpawnCfg() => CreateSO<SpawnProjectileConfig>("SpawnProjectileConfig");

        [MenuItem("Assets/Create/XSkillSystem/Collision/AttachAuraConfig", false, 42)]
        static void CreateAuraCfg2() => CreateSO<AttachAuraConfig>("AttachAuraConfig");

        // ====== Buff 节点配置 ======
        [MenuItem("Assets/Create/XSkillSystem/Buff/ApplyBuffConfig", false, 50)]
        static void CreateApplyBuffCfg() => CreateSO<ApplyBuffConfig>("ApplyBuffConfig");

        [MenuItem("Assets/Create/XSkillSystem/Buff/DispelConfig", false, 51)]
        static void CreateDispelCfg() => CreateSO<DispelConfig>("DispelConfig");

        // ====== State 节点配置 ======
        [MenuItem("Assets/Create/XSkillSystem/State/ApplyStateConfig", false, 60)]
        static void CreateApplyStateCfg() => CreateSO<ApplyStateConfig>("ApplyStateConfig");

        [MenuItem("Assets/Create/XSkillSystem/State/RemoveStateConfig", false, 61)]
        static void CreateRemoveStateCfg() => CreateSO<RemoveStateConfig>("RemoveStateConfig");

        [MenuItem("Assets/Create/XSkillSystem/State/DispelStateConfig", false, 62)]
        static void CreateDispelStateCfg() => CreateSO<DispelStateConfig>("DispelStateConfig");

        // ====== Timeline / Animator ======
        [MenuItem("Assets/Create/XSkillSystem/Timeline/TimelineDef", false, 70)]
        static void CreateTimelineDef() => CreateSO<TimelineDef>("TimelineDef");

        [MenuItem("Assets/Create/XSkillSystem/Timeline/PlayTimelineConfig", false, 71)]
        static void CreatePlayTLCfg() => CreateSO<PlayTimelineConfig>("PlayTimelineConfig");

        [MenuItem("Assets/Create/XSkillSystem/Timeline/StopTimelineConfig", false, 72)]
        static void CreateStopTLCfg() => CreateSO<StopTimelineConfig>("StopTimelineConfig");

        [MenuItem("Assets/Create/XSkillSystem/Timeline/PauseTimelineConfig", false, 73)]
        static void CreatePauseTLCfg() => CreateSO<PauseTimelineConfig>("PauseTimelineConfig");

        [MenuItem("Assets/Create/XSkillSystem/Timeline/SetTimelineTimeConfig", false, 74)]
        static void CreateSetTimeCfg() => CreateSO<SetTLTimeConfig>("SetTimelineTimeConfig");

        [MenuItem("Assets/Create/XSkillSystem/Timeline/SetTimelineSpeedConfig", false, 75)]
        static void CreateSetSpeedCfg() => CreateSO<SetTLSpeedConfig>("SetTimelineSpeedConfig");

        [MenuItem("Assets/Create/XSkillSystem/Timeline/WaitSignalConfig", false, 76)]
        static void CreateWaitSigCfg() => CreateSO<WaitSignalConfig>("WaitSignalConfig");

        [MenuItem("Assets/Create/XSkillSystem/Animator/AnimatorParamConfig", false, 77)]
        static void CreateAnimParamCfg() => CreateSO<AnimatorParamConfig>("AnimatorParamConfig");

        // ====== Numbers 节点配置（计算伤害节点用）======
        [MenuItem("Assets/Create/XSkillSystem/Numbers/ComputeDamageConfig", false, 80)]
        static void CreateComputeCfg() => CreateSO<ComputeDamageConfig>("ComputeDamageConfig");

        // ====== 快速生成标准 Timeline（你也可用之前的 TimelineWizard） ======
        [MenuItem("XSkillSystem/Create/Skill Timeline (Standard)", false, 5)]
        static void CreateStandardTimeline() => TimelineWizard.CreateStandardSkillTimeline();
    }
}
#endif