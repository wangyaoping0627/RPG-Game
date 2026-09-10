using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// 装备面板：C 键开关，5 槽位 + 属性总览
/// slotIcons 顺序对应 EquipmentSlot 枚举：0武器 1头盔 2护甲 3靴子 4饰品
/// 每个槽位挂 Button，OnClick 绑定 OnSlotClick 并填参数 0-4
public class EquipmentUI : MonoBehaviour
{
    public GameObject panel;
    public Image[] slotIcons = new Image[5];
    public TMP_Text statsText;

    private void Start()
    {
        EquipmentManager.OnChanged += Refresh;
        StatsManager.OnStatsChanged += Refresh;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        EquipmentManager.OnChanged -= Refresh;
        StatsManager.OnStatsChanged -= Refresh;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) Toggle();
    }

    private void Toggle()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
        Time.timeScale = panel.activeSelf ? 0f : 1f;
        if (panel.activeSelf) Refresh();
    }

    private void Refresh()
    {
        if (slotIcons != null)
        {
            for (int i = 0; i < slotIcons.Length && i < 5; i++)
            {
                if (slotIcons[i] == null) continue;
                var data = EquipmentManager.Get((EquipmentSlot)i);
                slotIcons[i].enabled = data != null;
                if (data != null)
                {
                    slotIcons[i].sprite = data.icon;
                    slotIcons[i].color = data.icon != null ? Color.white : QualityConfig.GetColor(data.quality);
                }
            }
        }

        if (statsText != null && StatsManager.Instance != null)
        {
            var s = StatsManager.Instance;
            statsText.text =
                $"攻击 {s.damage}\n" +
                $"防御 {s.defense}\n" +
                $"生命 {s.currentHealth}/{s.maxHealth}\n" +
                $"移速 {s.speed}\n" +
                $"暴击 {(int)(s.critRate * 100)}%\n" +
                $"范围 {s.weaponRange}";
        }
    }

    /// 点击槽位：脱下该槽位装备并放回背包
    public void OnSlotClick(int slot)
    {
        if (slot < 0 || slot > 4) return;

        var data = EquipmentManager.Get((EquipmentSlot)slot);
        if (data == null) return;

        EquipmentManager.Unequip((EquipmentSlot)slot);
        Inventory.Add(data); // 关键：Unequip 不会自动把装备放回背包
    }
}
