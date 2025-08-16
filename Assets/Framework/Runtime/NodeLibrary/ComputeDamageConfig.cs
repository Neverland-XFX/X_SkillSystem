using System;
using UnityEngine;

namespace XSkillSystem
{
    [Serializable]
    public sealed class ComputeDamageConfig : ScriptableObject
    {
        public DamageFormulaDef Formula;
        public NumbersConfig Numbers;
        public string[] Tags;
        public bool WriteToBB = true;
    }
}