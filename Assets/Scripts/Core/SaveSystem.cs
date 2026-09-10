using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// 存档系统：JSON 序列化 玩家属性 + 背包 + 装备，存 Application.persistentDataPath
/// 物品以 ItemData.id 字符串引用；加载时从 Resources/Items 建 id→SO 字典查回。
/// 前置条件：所有 ItemData 资产必须放在 Assets/Resources/Items/ 下。
public static class SaveSystem
{
    private const string FileName = "save.json";
    private static string FilePath => Application.persistentDataPath + "/" + FileName;

    private static Dictionary<string, ItemData> _itemDb;

    public static bool HasSave => File.Exists(FilePath);

    [Serializable]
    public class SaveData
    {
        public int level;
        public int currentExp;
        public int maxExp;
        public int currentHealth;
        public List<StackData> inventory = new List<StackData>();
        public List<SlotData> equipment = new List<SlotData>();
    }

    [Serializable]
    public class StackData
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public class SlotData
    {
        public int slot;        // (int)EquipmentSlot
        public string itemId;
    }

    /// id → ItemData 映射（Resources/Items 下所有 SO）
    private static Dictionary<string, ItemData> ItemDb()
    {
        if (_itemDb != null) return _itemDb;

        _itemDb = new Dictionary<string, ItemData>();
        foreach (var data in Resources.LoadAll<ItemData>("Items"))
        {
            if (data == null || string.IsNullOrEmpty(data.id)) continue;
            _itemDb[data.id] = data;
        }
        return _itemDb;
    }

    public static void Save()
    {
        var s = StatsManager.Instance;
        if (s == null)
        {
            Debug.LogWarning("[SaveSystem] StatsManager 不存在，取消保存");
            return;
        }

        var d = new SaveData
        {
            level = s.level,
            currentExp = s.currentExp,
            maxExp = s.maxExp,
            currentHealth = s.currentHealth,
        };

        foreach (var stack in Inventory.Items)
        {
            if (stack == null || stack.data == null) continue;
            d.inventory.Add(new StackData { itemId = stack.data.id, count = stack.count });
        }

        foreach (var kv in EquipmentManager.equipped)
        {
            if (kv.Value == null) continue;
            d.equipment.Add(new SlotData { slot = (int)kv.Key, itemId = kv.Value.id });
        }

        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(d));
            Debug.Log($"[SaveSystem] 已保存（{d.inventory.Count} 件背包物品 / {d.equipment.Count} 件装备）→ {FilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 保存失败：{e.Message}");
        }
    }

    public static void Load()
    {
        if (!HasSave)
        {
            Debug.Log("[SaveSystem] 无存档，跳过加载");
            return;
        }

        SaveData d;
        try
        {
            d = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 存档解析失败：{e.Message}");
            return;
        }
        if (d == null) return;

        var s = StatsManager.Instance;
        if (s != null)
        {
            s.level = d.level;
            s.currentExp = d.currentExp;
            s.maxExp = d.maxExp;
            s.currentHealth = d.currentHealth;
        }

        // 先清空装备（Unequip 不会把装备放回背包，正好用于干净重置），再重建
        foreach (var slot in new List<EquipmentSlot>(EquipmentManager.equipped.Keys))
            EquipmentManager.Unequip(slot);

        Inventory.Items.Clear();

        var db = ItemDb();
        foreach (var st in d.inventory)
        {
            if (db.TryGetValue(st.itemId, out var data))
                Inventory.Add(data, st.count);
            else
                Debug.LogWarning($"[SaveSystem] 找不到物品 id：{st.itemId}");
        }

        foreach (var sl in d.equipment)
        {
            if (db.TryGetValue(sl.itemId, out var data))
                EquipmentManager.Equip(data);
            else
                Debug.LogWarning($"[SaveSystem] 找不到装备 id：{sl.itemId}");
        }

        s?.NotifyStatsChanged();
        Debug.Log("[SaveSystem] 已加载存档");
    }

    public static void Delete()
    {
        if (HasSave) File.Delete(FilePath);
        Debug.Log("[SaveSystem] 已删除存档");
    }
}
