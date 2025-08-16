using UnityEngine;

namespace XSkillSystem
{
    // 事件：弹道生成/命中；范围查询结果
    public readonly struct EV_ProjectileSpawn
    {
        public readonly GameObject GO;
        public EV_ProjectileSpawn(GameObject go) => GO = go;
    }
}