using System.Collections;
using UnityEngine;
using TMPro;

/// 拾取提示：订阅 Inventory.OnChanged，背包格数增加时显示「获得 XXX」
/// 简化实现：以「格数增加」判断拾取。
/// 已知边界：穿戴替换装备时旧装备回包也会触发一次提示（影响极小，可忽略）
public class PickupToast : MonoBehaviour
{
    public TMP_Text text;
    public float duration = 1.2f;

    private int lastCount;
    private Coroutine showing;

    private void Start()
    {
        lastCount = Inventory.Items.Count;
        if (text != null) text.text = "";
        Inventory.OnChanged += OnInventoryChanged;
    }

    private void OnDestroy() => Inventory.OnChanged -= OnInventoryChanged;

    private void OnInventoryChanged()
    {
        if (Inventory.Items.Count > lastCount && Inventory.Items.Count > 0)
        {
            var last = Inventory.Items[Inventory.Items.Count - 1];
            Show($"获得 {last.data.displayName}");
        }
        lastCount = Inventory.Items.Count;
    }

    private void Show(string msg)
    {
        if (text == null) return;
        if (showing != null) StopCoroutine(showing);
        showing = StartCoroutine(ShowRoutine(msg));
    }

    private IEnumerator ShowRoutine(string msg)
    {
        text.text = msg;
        // Realtime：背包/暂停把 timeScale 设为 0 时提示仍能正常消失
        yield return new WaitForSecondsRealtime(duration);
        text.text = "";
        showing = null;
    }
}
