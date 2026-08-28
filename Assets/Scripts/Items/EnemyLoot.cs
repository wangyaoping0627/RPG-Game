using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class DropEntry
{
    public ItemData item;        // 掉什么
    [Range(0f, 1f)] public float chance = 1f; // 掉率（0-1）
    public int minCount = 1;     // 数量范围
    public int maxCount = 1;
}

/// 挂在敌人身上：敌人死亡时按掉落表在尸体位置生成掉落物。
/// 无需 prefab/资源，掉落物由 PickupSpawner 运行时动态创建。
public class EnemyLoot : MonoBehaviour
{
    [Header("掉落表（策划：每种敌人一张表）")]
    public DropEntry[] drops;

    // 无数据则不掉落，返回 false 供调试
    public bool HasDrops => drops != null && drops.Length > 0;

    /// 按掉落表随机生成，返回是否掉了东西
    public bool SpawnDrops(Vector3 position)
    {
        bool dropped = false;
        foreach (var e in drops)
        {
            if (e == null || e.item == null) continue;
            if (Random.value > e.chance) continue; // 未命中掉率
            int count = Random.Range(e.minCount, Mathf.Max(e.minCount + 1, e.maxCount + 1));
            PickupSpawner.Spawn(e.item, position, count);
            dropped = true;
        }
        return dropped;
    }
}
