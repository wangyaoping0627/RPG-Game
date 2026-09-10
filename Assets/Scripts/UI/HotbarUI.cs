using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// 快捷栏：底部 4 格，数字键 1-4 使用消耗品
/// hotbarItems 里拖入 4 个消耗品 SO 资产（如小/大生命药水）
public class HotbarUI : MonoBehaviour
{
    public ItemData[] hotbarItems = new ItemData[4];
    public Image[] slotIcons = new Image[4];        // 格子图标（可空）
    public TMP_Text[] slotCounts = new TMP_Text[4]; // 格子数量（可空）

    private void Start()
    {
        Inventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDestroy() => Inventory.OnChanged -= Refresh;

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) UseSlot(i);
        }
    }

    private void UseSlot(int index)
    {
        if (hotbarItems == null || index >= hotbarItems.Length) return;

        var data = hotbarItems[index];
        if (data == null || data.type != ItemType.Consumable) return;

        // 背包里没有就不给用
        int bagIndex = FindInBag(data);
        if (bagIndex < 0) return;

        var ph = FindObjectOfType<PlayerHealth>();
        if (ph == null) return;

        ph.ChangeHealth(data.healAmount);
        Inventory.RemoveAt(bagIndex, 1);
    }

    private int FindInBag(ItemData data)
    {
        for (int i = 0; i < Inventory.Items.Count; i++)
            if (Inventory.Items[i].data == data) return i;
        return -1;
    }

    private void Refresh()
    {
        for (int i = 0; i < 4; i++)
        {
            var data = (hotbarItems != null && i < hotbarItems.Length) ? hotbarItems[i] : null;
            int bagIndex = data != null ? FindInBag(data) : -1;
            int count = bagIndex >= 0 ? Inventory.Items[bagIndex].count : 0;

            if (slotIcons != null && i < slotIcons.Length && slotIcons[i] != null)
            {
                slotIcons[i].enabled = data != null;
                if (data != null)
                {
                    slotIcons[i].sprite = data.icon;
                    slotIcons[i].color = data.icon != null ? Color.white : QualityConfig.GetColor(data.quality);
                }
            }

            if (slotCounts != null && i < slotCounts.Length && slotCounts[i] != null)
                slotCounts[i].text = count > 0 ? count.ToString() : "";
        }
    }
}
