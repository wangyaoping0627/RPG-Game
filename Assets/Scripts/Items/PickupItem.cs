using UnityEngine;

/// 掉落物拾取：玩家走过自动拾取进背包。背包满则忽略并提示。
public class PickupItem : MonoBehaviour
{
    private ItemData _data;
    private int _count;
    private bool _picked;

    public void Init(ItemData data, int count)
    {
        _data = data;
        _count = count;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_picked || _data == null) return;
        if (!other.CompareTag("Player")) return;

        if (Inventory.Add(_data, _count))
        {
            _picked = true;
            InventoryChangedToast.Instance?.Show($"获得 [{_data.displayName}] x{_count}");
            Destroy(gameObject);
        }
        else
        {
            // 背包满：不拾取，飘提示
            InventoryChangedToast.Instance?.Show("背包已满");
        }
    }
}
