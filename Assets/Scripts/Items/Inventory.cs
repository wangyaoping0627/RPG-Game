using System;
using System.Collections;
using System.Collections.Generic;

/// 背包单例（纯 C# 静态，无需场景接线）：网格上限内增删查，同类可堆叠
/// 策划文档：5x8=40 格
public static class Inventory
{
    public static readonly int Capacity = 40;
    public static readonly List<ItemStack> Items = new List<ItemStack>();

    /// 全静态事件发往 UI。因为类静态、事件委托静态即可，无需清理。
    public static event Action OnChanged;

    public static bool IsFull => Items.Count >= Capacity;

    /// 添加物品：堆叠或占用新格。成功返回 true（满格返回 false）
    public static bool Add(ItemData data, int count = 1)
    {
        // 非消耗品/材料也可堆叠，由 count 控制；装备默认1件占1格
        foreach (var stack in Items)
        {
            if (stack.data == data)
            {
                stack.count += count;
                OnChanged?.Invoke();
                return true;
            }
        }

        if (IsFull) return false;
        Items.Add(new ItemStack(data, count));
        OnChanged?.Invoke();
        return true;
    }

    /// 从指定格子扣除数量，空则移除该格
    public static void RemoveAt(int index, int amount = 1)
    {
        if (index < 0 || index >= Items.Count) return;
        Items[index].count -= amount;
        if (Items[index].count <= 0) Items.RemoveAt(index);
        OnChanged?.Invoke();
    }

    public static ItemStack Get(int index)
        => index >= 0 && index < Items.Count ? Items[index] : null;
}
