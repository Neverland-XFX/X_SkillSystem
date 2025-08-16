using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace XSkillSystem
{
    // —— 报告 —— //
    [Serializable]
    public sealed class BTCompileReport
    {
        public bool Success;
        public List<string> Errors = new();
        public List<string> Warnings = new();

        public static BTCompileReport Fail(string err)
        {
            return new BTCompileReport { Success = false, Errors = new List<string> { err } };
        }
    }
}
