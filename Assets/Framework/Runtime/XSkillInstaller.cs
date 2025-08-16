using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XSkillSystem
{
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    public sealed class XSkillInstaller : MonoBehaviour
    {
        [Header("Core Assets")] public NumbersConfig Numbers;
        public ProjectilePool ProjectilePool;

        [Header("Event Bus")] [Tooltip("（可选）场景中的 EventBusHost；若留空，将自动查找或创建。")]
        public EventBusHost BusHost;

        [Header("Options")] public bool MakePersistent = true;
        public bool VerboseLog = false;

        public static INodeLibrary<XContext> Library { get; private set; }
        public static EventBus Bus { get; set; } // 由 Host 或本类设置
        public static XSkillInstaller Instance { get; private set; }

        private static readonly List<INodeLibrary<XContext>> s_extraLibs = new();
        private static bool s_inited;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (VerboseLog) Debug.Log("[XSkillInstaller] Duplicate installer detected, destroying this one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (MakePersistent) DontDestroyOnLoad(gameObject);

            EnsureAssets();
            BuildLibrary();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Library = null;
                // 不强制清空 Bus；由 Host 生命周期管理
                s_inited = false;
                s_extraLibs.Clear();
            }
        }

        public static void RegisterLib(INodeLibrary<XContext> lib)
        {
            if (lib == null) return;
            s_extraLibs.Add(lib);
            if (s_inited && Library != null && Instance != null)
                Instance.BuildLibrary();
        }

        public static void EnsureInitialized()
        {
            if (s_inited) return;
            var inst = FindObjectOfType<XSkillInstaller>();
            if (inst == null)
            {
                var go = new GameObject("[XSkillInstaller]");
                inst = go.AddComponent<XSkillInstaller>();
                inst.MakePersistent = true;
            }
            // Awake 会继续初始化
        }

        void EnsureAssets()
        {
            // --- EventBus：优先用 BusHost，其次查找 Host，再不行则创建一个 Host ---
            if (BusHost == null) BusHost = FindObjectOfType<EventBusHost>();
            if (BusHost == null)
            {
                var go = new GameObject("[EventBusHost]");
                BusHost = go.AddComponent<EventBusHost>(); // 会在 Awake 里把 Bus 写到 XSkillInstaller.Bus
            }

            Bus = BusHost.Bus;

            // Numbers：字段为空则尝试 Resources
            if (Numbers == null)
            {
                Numbers = Resources.Load<NumbersConfig>("NumbersConfig");
                if (Numbers == null && VerboseLog)
                    Debug.LogWarning("[XSkillInstaller] NumbersConfig 未设置，建议创建并放到 Resources/NumbersConfig.asset。");
            }

            // ProjectilePool：字段为空则在场景找
            if (ProjectilePool == null)
            {
                ProjectilePool = FindObjectOfType<ProjectilePool>();
                if (ProjectilePool == null && VerboseLog)
                    Debug.LogWarning("[XSkillInstaller] 未找到 ProjectilePool，SpawnProjectile 节点将无法工作。");
            }
        }

        void BuildLibrary()
        {
            var baseLib = new DefaultNodeLibrary();
            var buffLib = new BuffNodeLibrary(Numbers);
            var stateLib = new StateNodeLibrary();
            var tlLib = new TimelineNodeLibrary();
            var colLib = new CollisionNodeLibrary(ProjectilePool, Bus, baseLib, buffLib, Numbers);

            var libs = new List<INodeLibrary<XContext>>(8)
                { baseLib, buffLib, colLib, stateLib, tlLib };
            if (s_extraLibs.Count > 0) libs.AddRange(s_extraLibs);

            Library = XSLibMux.For(libs.ToArray());
            s_inited = true;

            if (VerboseLog)
            {
                Debug.Log(
                    $"[XSkillInstaller] Ready. Numbers={(Numbers ? Numbers.name : "NULL")}, Pool={(ProjectilePool ? ProjectilePool.name : "NULL")}, BusHost={(BusHost ? BusHost.name : "NULL")}, ExtraLibs={s_extraLibs.Count}");
            }
        }

        public bool ValidateSetup(out string message)
        {
            if (Numbers == null)
            {
                message = "NumbersConfig 未设置。";
                return false;
            }

            if (ProjectilePool == null)
            {
                message = "ProjectilePool 未设置。";
                return false;
            }

            if (Bus == null)
            {
                message = "EventBus 未就绪。";
                return false;
            }

            if (Library == null)
            {
                message = "Library 未构建。";
                return false;
            }

            message = "OK";
            return true;
        }

#if UNITY_EDITOR
        [MenuItem("XSkillSystem/Setup/Create Installer In Scene", priority = 1)]
        private static void CreateInstallerInScene()
        {
            var exist = FindObjectOfType<XSkillInstaller>();
            if (exist != null)
            {
                Selection.activeObject = exist.gameObject;
                EditorUtility.DisplayDialog("XSkillSystem", "场景已存在 XSkillInstaller。", "OK");
                return;
            }

            var go = new GameObject("[XSkillInstaller]");
            var inst = go.AddComponent<XSkillInstaller>();

            // 自动放置一个 BusHost（若场景没有）
            inst.BusHost = FindObjectOfType<EventBusHost>();
            if (inst.BusHost == null)
            {
                var hostGo = new GameObject("[EventBusHost]");
                inst.BusHost = hostGo.AddComponent<EventBusHost>();
            }

            inst.Numbers = Resources.Load<NumbersConfig>("NumbersConfig");
            inst.ProjectilePool = FindObjectOfType<ProjectilePool>();

            Selection.activeObject = go;
            EditorUtility.DisplayDialog("XSkillSystem",
                "已创建 XSkillInstaller + EventBusHost。请在 Inspector 确认 Numbers / ProjectilePool。", "OK");
        }

        [MenuItem("XSkillSystem/Setup/Validate Installer", priority = 2)]
        private static void ValidateInstallerMenu()
        {
            var inst = FindObjectOfType<XSkillInstaller>();
            if (inst == null)
            {
                EditorUtility.DisplayDialog("XSkillSystem", "场景中未找到 XSkillInstaller。", "OK");
                return;
            }

            if (inst.ValidateSetup(out var msg))
                EditorUtility.DisplayDialog("XSkillSystem", "安装就绪\n" + msg, "OK");
            else
                EditorUtility.DisplayDialog("XSkillSystem", "安装不完整：\n" + msg, "OK");
        }
#endif
    }
}