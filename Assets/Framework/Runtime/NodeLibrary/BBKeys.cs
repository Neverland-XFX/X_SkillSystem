using System;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 黑板常用Key集中管理。
    /// </summary>
    public static class BBKeys
    {
        // 追加到 BBKeys 中
        public static readonly BBKey<float> k_LastDamage = new("LastDamage");
        public static readonly BBKey<bool> k_LastCrit = new("LastCrit");
        public static readonly BBKey<DamageFormulaDef> k_Formula = new("Formula");
        public static readonly BBKey<NumbersConfig> k_NumCfg = new("NumbersCfg");

        public static readonly BBKey<bool> k_HasTarget = new("HasTarget");
        public static readonly BBKey<Vector3> k_AimPoint = new("AimPoint");
        public static readonly BBKey<float> k_TempFloat = new("TempFloat");
        public static readonly BBKey<bool> k_LastRoll = new("LastRoll");
        public static readonly BBKey<GameObject> k_TargetGO = new("TargetGO");
    }
}