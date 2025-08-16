using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace XSkillSystem
{
    /// <summary>
    /// 统一时钟接口（确定性/可回放）。
    /// </summary>
    public interface IClock
    {
        double Time { get; }
        double DeltaTime { get; }
    }
}