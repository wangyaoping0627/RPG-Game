using UnityEditor;
using UnityEngine;

/// 编辑器菜单：一键生成示例装备/药水 SO 资产，便于立刻测试掉装→拾取→穿戴链路。
/// 产物在 Assets/_SampleItems/ 下，可随时删。目录名为 Editor 使 UnityEditor 仅在编辑器编译。
public static class SampleItemsEditor
{
    private const string Dir = "Assets/_SampleItems";

    [MenuItem("RPG/Create Sample Items")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder(Dir))
            AssetDatabase.CreateFolder("Assets", "_SampleItems");

        MakeWeapon("木剑", Quality.Common, 3, 0, 2);
        MakeWeapon("铁剑", Quality.Rare, 4, 0, 2);
        MakeWeapon("霜之哀草", Quality.Epic, 5, 0, 3);
        MakeWeapon("传说之刃", Quality.Legendary, 6, 0.2f, 3);

        MakeGear("布帽", Quality.Common, EquipmentSlot.Helmet, 0, 1, 3);
        MakeGear("精铁盔", Quality.Rare, EquipmentSlot.Helmet, 0, 2, 5);

        MakeGear("皮甲", Quality.Common, EquipmentSlot.Armor, 0, 1, 5);
        MakeGear("板甲", Quality.Rare, EquipmentSlot.Armor, 0, 2, 8);

        MakeGear("草鞋", Quality.Common, EquipmentSlot.Boots, 0, 0, 0, 0.5f);
        MakeGear("疾风靴", Quality.Epic, EquipmentSlot.Boots, 0, 0, 0, 1f);

        MakePotion("小生命药水", 10);
        MakePotion("大生命药水", 30);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RPG] 示例道具已生成到 Assets/_SampleItems");
    }

    private static void MakeWeapon(string name, Quality q, float dmg, float crit, float range)
    {
        var item = ScriptableObject.CreateInstance<ItemData>();
        item.id = name;
        item.displayName = name;
        item.type = ItemType.Equipment;
        item.slot = EquipmentSlot.Weapon;
        item.quality = q;
        item.description = $"{QualityConfig.Name(q)}武器";
        item.addDamage = dmg;
        item.addCritRate = crit;
        item.addWeaponRange = range;
        Save(item, name);
    }

    private static void MakeGear(string name, Quality q, EquipmentSlot slot,
        float dmg = 0, int def = 0, int hp = 0, float speed = 0)
    {
        var item = ScriptableObject.CreateInstance<ItemData>();
        item.id = name;
        item.displayName = name;
        item.type = ItemType.Equipment;
        item.slot = slot;
        item.quality = q;
        item.description = $"{QualityConfig.Name(q)}{slot}";
        item.addDamage = dmg;
        item.addDefense = def;
        item.addMaxHealth = hp;
        item.addSpeed = speed;
        Save(item, name);
    }

    private static void MakePotion(string name, int heal)
    {
        var item = ScriptableObject.CreateInstance<ItemData>();
        item.id = name;
        item.displayName = name;
        item.type = ItemType.Consumable;
        item.quality = Quality.Common;
        item.description = $"恢复 {heal} 点生命";
        item.healAmount = heal;
        Save(item, name);
    }

    private static void Save(ItemData item, string assetName)
        => AssetDatabase.CreateAsset(item, $"{Dir}/{assetName}.asset");
}
