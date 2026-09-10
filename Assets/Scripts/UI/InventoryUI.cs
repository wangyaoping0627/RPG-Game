using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class InventoryUI : MonoBehaviour
{
    public GameObject panel;
    public Transform gridParent;    // 挂 GridLayoutGroup 的容器
    public GameObject slotPrefab;

    private void Start()
    {
        Inventory.OnChanged += Refresh;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy() => Inventory.OnChanged -= Refresh;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Toggle();
    }

    private void Toggle()
    {
        if (panel == null) return;
        panel.SetActive(!panel.activeSelf);
        Time.timeScale = panel.activeSelf ? 0f : 1f; // 打开背包暂停游戏
        if (panel.activeSelf) Refresh();
    }

    private void Refresh()
    {
        if (gridParent == null || slotPrefab == null) return;

        ClearGrid();

        for (int i = 0; i < Inventory.Items.Count; i++)
        {
            var stack = Inventory.Items[i];
            var go = Instantiate(slotPrefab, gridParent);

            var icon = Find<Image>(go, "Icon");
            if (icon != null)
            {
                if (stack.data.icon != null) icon.sprite = stack.data.icon;
                else icon.color = QualityConfig.GetColor(stack.data.quality); // 无图标用品质色块
            }

            var nameTxt = Find<TMP_Text>(go, "Name");
            if (nameTxt != null) nameTxt.text = stack.data.displayName;

            var countTxt = Find<TMP_Text>(go, "Count");
            if (countTxt != null) countTxt.text = stack.count > 1 ? stack.count.ToString() : "";

            int idx = i; // 闭包捕获，不能用循环变量本身
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => OnSlotClick(idx));
        }
    }

    /// 倒序 + 先 SetParent(null)：立即脱离布局，避免与新建格子同帧共存导致闪动
    private void ClearGrid()
    {
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            var child = gridParent.GetChild(i);
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }
    }

    private static T Find<T>(GameObject go, string childName) where T : Component
    {
        var t = go.transform.Find(childName);
        return t != null ? t.GetComponent<T>() : null;
    }

    /// 点击格子：装备→穿上；消耗品→使用
    private void OnSlotClick(int index)
    {
        var stack = Inventory.Get(index);
        if (stack == null || stack.data == null) return;
        var data = stack.data;

        if (data.type == ItemType.Equipment)
        {
            // 关键：先移出背包再穿戴 —— EquipmentManager.Equip 不会自动把物品移出背包
            Inventory.RemoveAt(index);
            EquipmentManager.Equip(data);
        }
        else if (data.type == ItemType.Consumable)
        {
            var ph = FindObjectOfType<PlayerHealth>();
            if (ph != null)
            {
                ph.ChangeHealth(data.healAmount);
                Inventory.RemoveAt(index);
            }
        }
    }
}
