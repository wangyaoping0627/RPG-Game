using System.Collections.Generic;

/// 装备系统单例（纯 C# 静态）：5 槽位穿戴/脱下，用「增量法」把装备加成写回 StatsManager。
/// 增量法 = 记录当前装备总加成，穿/脱时只改差值，因此不破坏 LevelUp 直接改字段的升级逻辑。
public static class EquipmentManager
{
    public static readonly Dictionary<EquipmentSlot, ItemData> equipped =
        new Dictionary<EquipmentSlot, ItemData>();

    /// 事件：装备格子变化（UI 刷新用）
    public static event System.Action OnChanged;

    private static readonly Dictionary<string, float> _bonus = new Dictionary<string, float>
    {
        { nameof(StatsManager.damage), 0f },
        { nameof(StatsManager.defense), 0f },
        { nameof(StatsManager.speed), 0f },
        { nameof(StatsManager.maxHealth), 0f },
        { nameof(StatsManager.critRate), 0f },
        { nameof(StatsManager.weaponRange), 0f },
    };

    public static ItemData Get(EquipmentSlot slot)
        => equipped.TryGetValue(slot, out var d) ? d : null;

    /// 返回当前 slot 是否有装备
    public static bool IsEquipped(EquipmentSlot slot) => equipped.ContainsKey(slot);

    /// 穿戴：替换同槽位旧装备（旧装备回到背包）。成功返回 true。
    public static bool Equip(ItemData data)
    {
        if (data == null || data.type != ItemType.Equipment) return false;

        // 同槽位有旧装备：先卸下（回背包），再加成(值含在差值里自然抵消)
        if (equipped.TryGetValue(data.slot, out var old))
        {
            // 差值计算里用 newBonus - oldBonus，因此这里只需替换条目
            equipped[data.slot] = data;
            Inventory.Add(old);
        }
        else
        {
            equipped[data.slot] = data;
        }

        ReapplyToStats();
        return true;
    }

    /// 脱下指定槽位：装备加成移除并写回，装备回背包
    public static void Unequip(EquipmentSlot slot)
    {
        if (!equipped.Remove(slot)) return;
        ReapplyToStats();
    }

    /// 用 Sum(穿戴装备加成) 计算各属性差值并写回 StatsManager。
    /// 全量重算后再整体写回，比逐件增删更稳（避免旧值残留）。
    private static void ReapplyToStats()
    {
        // 全量重算当前总加成
        var total = new Dictionary<string, float>(_bonus);
        foreach (var kvp in _bonus)
            total[kvp.Key] = 0f;

        foreach (var data in equipped.Values)
        {
            total[nameof(StatsManager.damage)] += data.DamageValue;
            total[nameof(StatsManager.defense)] += data.DefenseValue;
            total[nameof(StatsManager.speed)] += data.SpeedValue;
            total[nameof(StatsManager.maxHealth)] += data.MaxHealthValue;
            total[nameof(StatsManager.critRate)] += data.CritRateValue;
            total[nameof(StatsManager.weaponRange)] += data.WeaponRangeValue;
        }

        // 差值写回：newTotal - oldTotal
        ApplyDelta(nameof(StatsManager.damage), total, ref StatsManager.Instance.damage);
        ApplyDelta(nameof(StatsManager.defense), total, ref StatsManager.Instance.defense);
        ApplyDelta(nameof(StatsManager.speed), total, ref StatsManager.Instance.speed);
        ApplyDelta(nameof(StatsManager.maxHealth), total, ref StatsManager.Instance.maxHealth);
        ApplyDelta(nameof(StatsManager.critRate), total, ref StatsManager.Instance.critRate);
        ApplyDelta(nameof(StatsManager.weaponRange), total, ref StatsManager.Instance.weaponRange);

        // 血量上限变化后，把当前血量钳到新上限
        if (StatsManager.Instance.currentHealth > StatsManager.Instance.maxHealth)
            StatsManager.Instance.currentHealth = StatsManager.Instance.maxHealth;

        // 把 total 存为新的“旧加成”基准
        foreach (var k in total.Keys)
            _bonus[k] = total[k];

        StatsManager.Instance.NotifyStatsChanged();
        OnChanged?.Invoke();
    }

    private static void ApplyDelta(string key, Dictionary<string, float> total, ref int field)
        => field += (int)(total[key] - _bonus[key]);

    private static void ApplyDelta(string key, Dictionary<string, float> total, ref float field)
        => field += total[key] - _bonus[key];
}
