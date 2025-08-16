using System;

namespace XSkillSystem
{
    public static class DamagePipeline
    {
        /// <summary>
        /// Compute-only：不做生命扣减（应用留到节点/Health系统）。支持 EventBus 发送“已计算”事件。
        /// </summary>
        public static DamageResult Compute(
            IStatProvider attacker, IStatProvider target,
            DamageFormulaDef f, NumbersConfig cfg, IBTRandom rng,
            string[] tags = null, IEventBus bus = null, BTTreeInfo? treeInfo = null)
        {
            if (attacker == null || target == null || f == null || cfg == null) return default;

            // 1) Base
            float atk = attacker.GetStat((int)StatId.Attack);
            float sp = attacker.GetStat((int)StatId.SpellPower);
            float lvl = attacker.GetStat((int)StatId.Level);
            float @base = f.Base + f.AttackCoeff * atk + f.SpellPowerCoeff * sp + f.LevelCoeff * lvl;

            // 2) Offense A（组内加法 → 1 + sum）
            float offA = 1f + attacker.GetStat((int)StatId.AdditiveDamageBonus);

            // 3) Crit（可被公式覆盖）
            float critChance = f.OverrideCrit ? f.CritChance : attacker.GetStat((int)StatId.CritChance);
            float critMul = f.OverrideCrit ? f.CritDamage : MathF.Max(1f, attacker.GetStat((int)StatId.CritDamage));
            bool isCrit = rng != null && rng.NextDouble() < critChance;
            float afterCrit = (@base * offA) * (isCrit ? critMul : 1f);

            // 4) Offense B（与标签相关的乘区）
            float offB = 1f;
            if (tags != null)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    switch (tags[i])
                    {
                        case "Fire": offB *= (1f + attacker.GetStat((int)StatId.ElementalBonus_Fire)); break;
                        case "Ice": offB *= (1f + attacker.GetStat((int)StatId.ElementalBonus_Ice)); break;
                    }
                }
            }

            offB *= (1f + attacker.GetStat((int)StatId.SkillBonus));

            float attackSide = afterCrit * offB;

            // 5) DefenseA：护甲/抗性（在穿透之后）
            float defA = 1f;
            if (f.Type == DamageType.Physical)
            {
                float armor = target.GetStat((int)StatId.Armor);
                // 穿透
                float penFlat = attacker.GetStat((int)StatId.ArmorPenetration);
                float penPct = attacker.GetStat((int)StatId.ArmorPenetrationPct);
                armor = MathF.Max(0f, armor - penFlat);
                armor *= MathF.Max(0f, 1f - penPct);

                float reduction = ResistModel.PhysicalReduction(armor, lvl, cfg.ArmorK, cfg.ArmorC);
                defA = MathF.Max(0.05f, 1f - reduction); // 物理保证下限
            }
            else if (f.Type == DamageType.Fire || f.Type == DamageType.Ice)
            {
                float resist = f.Type == DamageType.Fire
                    ? target.GetStat((int)StatId.Resist_Fire)
                    : target.GetStat((int)StatId.Resist_Ice);

                float pen = 0f;
                if (f.Type == DamageType.Fire) pen += attacker.GetStat((int)StatId.ResistPenetration_Fire);
                // Ice 穿透示例：可新增 StatId.ResistPenetration_Ice

                defA = ResistModel.ElementalMultiplier(resist, pen, cfg.ResistMin, cfg.ResistMax);
            }
            else if (f.Type == DamageType.True)
            {
                defA = 1f; // 纯真伤害无抗性
            }

            // 6) DefenseB：减伤&易伤乘区
            float defB = (1f - target.GetStat((int)StatId.DamageReduction)) *
                         (1f + target.GetStat((int)StatId.Vulnerability));
            if (defB < 0.05f) defB = 0.05f; // 下限保护

            // 7) PvP & Clamps
            float pvp = cfg.PvPScaling * attacker.GetStat((int)StatId.PvpScaling);
            if (pvp <= 0f) pvp = 1f; // 默认不衰减（如未设置）
            float final = attackSide * defA * defB * pvp;
            if (final < cfg.MinDamage) final = cfg.MinDamage;

            // 拆解记录
            var bd = new DamageBreakdown(@base, offA, isCrit ? critMul : 1f, offB, defA, defB, pvp, final, isCrit);

            // 可选事件：仅“已计算”
            if (bus != null && treeInfo.HasValue)
            {
                bus.Publish(new EV_Log($"[DMG] {treeInfo.Value.TreeName}: {bd}"));
            }

            return new DamageResult(final, isCrit, f.Type, bd);
        }
    }
}