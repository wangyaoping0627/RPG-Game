using UnityEngine;

/// 伤害计算工具：攻防减伤/暴击/浮动
public static class CombatCalculator
{
    /// 计算最终伤害
    /// 减伤公式：防御力越高减伤越多，但收益递减
    /// 浮动范围：±10%
    /// 暴击：按暴击率判定，暴击伤害 ×1.5
    public static int CalculateDamage(
        int attack, int defense,
        float critRate, float critMultiplier,
        out bool isCrit)
    {
        float baseDamage = attack;

        // 防御减伤：defense / (defense + 100)
        float reduction = baseDamage * (defense / (defense + 100f));
        float afterDefense = baseDamage - reduction;

        // 伤害浮动 ±10%
        float fluctuation = Random.Range(0.9f, 1.1f);
        float finalDamage = afterDefense * fluctuation;

        // 暴击判定
        isCrit = Random.value < critRate;
        if (isCrit) finalDamage *= critMultiplier;

        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }
}