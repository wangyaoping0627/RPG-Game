using UnityEngine;

/// 掉落物拾取：玩家走过自动拾取进背包。背包满则不拾取。
/// UI 反馈（获得提示/背包已满）由 UI 层订阅 Inventory.OnChanged 自行处理。
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
            AudioManager.Pickup();
            Destroy(gameObject);
        }
        // 背包满：不拾取（是否提示由 UI 决定）
    }
}
