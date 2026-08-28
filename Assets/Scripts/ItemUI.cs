using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// 背包/装备简单面板（从简 UI，纯代码动态生成，无需 prefab/拖引用）。
/// I：背包 ｜ C：装备面板 ｜ 数字键 1-4：快捷使用前4个消耗品
/// 点击物品：消耗品→使用；装备→穿戴（穿→脱下的回背包）。
/// 完整拖拽/图标版 UI 留到第三阶段，这里是跑通数据链路的占位面板。
public class ItemUI : MonoBehaviour
{
    private Text _inventoryText;
    private Text _equipText;
    private bool _invVisible;
    private bool _equipVisible;

    private void Awake()
    {
        var canvas = FindObjectOfType<Canvas>();
        var parent = canvas.transform;
        _inventoryText = MakeLabel(parent, "InventoryPanel");
        _equipText = MakeLabel(parent, "EquipPanel");
        _equipText.gameObject.SetActive(false);
        _inventoryText.gameObject.SetActive(false);
    }

    private static Text MakeLabel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0.15f);
        rect.anchorMax = new Vector2(0.5f, 0.85f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.transform.SetAsLastSibling();
        return text;
    }

    private void OnEnable()
    {
        Inventory.OnChanged += RefreshInventory;
        EquipmentManager.OnChanged += RefreshEquipment;
    }

    private void OnDisable()
    {
        Inventory.OnChanged -= RefreshInventory;
        EquipmentManager.OnChanged -= RefreshEquipment;
    }

    private void Update()
    {
        // 输入用 InputSystem 显示名不一致，直接取键位
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventory();
        if (Input.GetKeyDown(KeyCode.C)) ToggleEquipment();

        // 数字键 1-4：使用对应快捷栏（前4个消耗品）
        for (int i = 0; i < 4; i++)
        {
            KeyCode key = i switch { 0 => KeyCode.Alpha1, 1 => KeyCode.Alpha2, 2 => KeyCode.Alpha3, _ => KeyCode.Alpha4 };
            if (Input.GetKeyDown(key)) UseQuickSlot(i);
        }
    }

    private void ToggleInventory()
    {
        if (_equipVisible) ToggleEquipment(); // 同时只开一个面板
        _invVisible = !_invVisible;
        _inventoryText.gameObject.SetActive(_invVisible);
        if (_invVisible) RefreshInventory();
    }

    private void ToggleEquipment()
    {
        if (_invVisible) ToggleInventory();
        _equipVisible = !_equipVisible;
        _equipText.gameObject.SetActive(_equipVisible);
        if (_equipVisible) RefreshEquipment();
    }

    private void RefreshInventory()
    {
        if (_inventoryText == null) return;
        var sb = new StringBuilder();
        sb.AppendLine("背包（按I关闭）");
        if (Inventory.Items.Count == 0) { sb.AppendLine("（空）"); }
        for (int i = 0; i < Inventory.Items.Count; i++)
        {
            var s = Inventory.Items[i];
            if (s.data == null) continue;
            sb.Append($"{i + 1}. [{QualityConfig.Name(s.data.quality)}]{s.data.displayName} x{s.count}");
            if (s.data.type == ItemType.Consumable) sb.Append("  <使用>");
            else if (s.data.type == ItemType.Equipment) sb.Append("  <穿>");
            sb.AppendLine();
        }
        sb.AppendLine("\n点击数字键1-4使用前4个消耗品");
        _inventoryText.text = sb.ToString();
    }

    /// 点击或快捷键：index 为背包格序号
    private void UseSlot(int index)
    {
        var stack = Inventory.Get(index);
        if (stack?.data == null) return;

        switch (stack.data.type)
        {
            case ItemType.Consumable:
                UseConsumable(stack.data);
                Inventory.RemoveAt(index);
                break;
            case ItemType.Equipment:
                if (EquipmentManager.Equip(stack.data))
                    Inventory.RemoveAt(index, 1);
                break;
        }
    }

    private void UseQuickSlot(int slotIndex)
    {
        if (!_invVisible) return; // 快捷栏数字键仅在背包打开时可用（简化）
        // 从后往前找第 slotIndex 个消耗品（也可直接用格子序号，这里改：用索引即序号）
        for (int i = 0; i < Inventory.Items.Count; i++)
        {
            var s = Inventory.Items[i];
            if (s.data != null && s.data.type == ItemType.Consumable)
            {
                if (slotIndex == 0) { UseSlot(i); return; }
                slotIndex--;
            }
        }
    }

    // 从简消耗品：生命药水回血，调用 PlayerHealth.ChangeHealth；增益类后续扩展
    private void UseConsumable(ItemData data)
    {
        if (data.healAmount > 0 && PlayerHealth.Instance != null)
            PlayerHealth.Instance.ChangeHealth(data.healAmount);
        InventoryChangedToast.Instance?.Show($"使用 {data.displayName}");
    }

    private void RefreshEquipment()
    {
        if (_equipText == null) return;
        var sb = new StringBuilder();
        sb.AppendLine("装备面板（按C关闭）");
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            var item = EquipmentManager.Get(slot);
            sb.Append($"{slot}: ");
            sb.AppendLine(item == null ? "（无）" : $"[{QualityConfig.Name(item.quality)}]{item.displayName}");
        }
        sb.AppendLine("\n当前属性：");
        sb.AppendLine($"攻击 {StatsManager.Instance.damage} | 防御 {StatsManager.Instance.defense}");
        sb.AppendLine($"移速 {StatsManager.Instance.speed} | MaxHP {StatsManager.Instance.maxHealth}");
        sb.AppendLine($"暴击率 {StatsManager.Instance.critRate:P0} | 范围 {StatsManager.Instance.weaponRange}");
        _equipText.text = sb.ToString();
    }
}
