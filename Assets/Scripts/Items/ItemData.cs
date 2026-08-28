using UnityEngine;

/// 道具大类
public enum ItemType { Equipment, Consumable, Material }

/// 装备槽位
public enum EquipmentSlot { Weapon, Helmet, Armor, Boots, Accessory }

/// 装备品质（属性倍率 + 颜色）
public enum Quality { Common = 0, Rare, Epic, Legendary }

/// 品质静态配置（对应策划文档五色品质表）
public static class QualityConfig
{
    public static float Multiplier(Quality q) => q switch
    {
        Quality.Common => 1.0f,
        Quality.Rare => 1.3f,
        Quality.Epic => 1.6f,
        Quality.Legendary => 2.0f,
        _ => 1.0f,
    };

    /// 品质颜色（白/蓝/紫/金）
    public static Color GetColor(Quality q) => q switch
    {
        Quality.Common => Color.white,
        Quality.Rare => new Color(0.29f, 0.62f, 1f),
        Quality.Epic => new Color(0.69f, 0.28f, 0.94f),
        Quality.Legendary => new Color(1f, 0.69f, 0f),
        _ => Color.white,
    };

    public static string Name(Quality q) => q switch
    {
        Quality.Common => "普通",
        Quality.Rare => "优秀",
        Quality.Epic => "稀有",
        Quality.Legendary => "传说",
        _ => "",
    };
}

/// 可穿戴的装备等成为 ScriptableObject 资产：定义一件道具的全部静态数据
/// 生成方式：右键 Create > RPG > Item，或代码 AssetDatabase 创建
[CreateAssetMenu(fileName = "Item", menuName = "RPG/Item", order = 0)]
public class ItemData : ScriptableObject
{
    [Header("基础")]
    public string id;             // 唯一标识（存档/识别用），留空则用 name
    public string displayName;    // 显示名
    [TextArea] public string description;
    public Sprite icon;           // 可选，背包图标（从简 UI 可不填）

    [Header("分类")]
    public ItemType type = ItemType.Equipment;
    public EquipmentSlot slot;    // 仅装备有效
    public Quality quality = Quality.Common;

    [Header("属性加成（装备，倍率自动按品质套用）")]
    public float addDamage;
    public int addDefense;
    public float addSpeed;
    public int addMaxHealth;
    public float addCritRate;
    public float addWeaponRange;

    [Header("消耗品")]
    public int healAmount; // 生命药水回复量；增益类留 0

    /// 品质倍率应用到属性加成上（策划：品质决定最终属性倍率）
    public float DamageValue => addDamage * QualityConfig.Multiplier(quality);
    public int DefenseValue => Mathf.RoundToInt(addDefense * QualityConfig.Multiplier(quality));
    public float SpeedValue => addSpeed * QualityConfig.Multiplier(quality);
    public int MaxHealthValue => Mathf.RoundToInt(addMaxHealth * QualityConfig.Multiplier(quality));
    public float CritRateValue => addCritRate * QualityConfig.Multiplier(quality);
    public float WeaponRangeValue => addWeaponRange * QualityConfig.Multiplier(quality);

    void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id)) id = name;
    }
}
