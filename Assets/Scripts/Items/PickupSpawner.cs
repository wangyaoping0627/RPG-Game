using UnityEngine;

/// 静态掉落物生成器：运行时动态 new 一个带碰撞和拾取的物体，无需 prefab。
/// 永久存在于场景（也可挂在任意 DontDestroyOnLoad 对象），静态类即可。
public static class PickupSpawner
{
    /// 在 position 生成一个掉落物（一个物体代表一批同类物品）
    public static void Spawn(ItemData data, Vector3 position, int count)
    {
        var go = new GameObject($"Pickup_{data.displayName}");
        go.transform.position = position;

        // 随机散布一点，避免多件重叠
        go.transform.position += (Vector3)(Random.insideUnitCircle * 0.3f);

        // 视觉：quality 颜色方块示意（无 icon 时兜底；美术资源到位可换成 Sprite）
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = data.icon;
        renderer.color = QualityConfig.GetColor(data.quality);
        if (renderer.sprite == null)
        {
            // 无图标：生成一个纯色单位方块占位
            renderer.sprite = Sprite.Create(
                Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        }
        renderer.sortingOrder = 5; // 显示在地面之上

        // 物理：触发碰撞实现自动拾取
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.6f);
        col.isTrigger = true;

        var pickup = go.AddComponent<PickupItem>();
        pickup.Init(data, count);

        // 未拾取则限时消失
        Object.Destroy(go, 30f);
    }
}
