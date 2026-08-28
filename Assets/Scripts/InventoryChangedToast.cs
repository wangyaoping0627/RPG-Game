using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// 屏幕中部拾取/提示飘字（对应策划 5.3「获得 [物品名]」）。单例，自动在 Canvas 上动态创建。
/// 挂在常驻对象上，运行时无需拖引用（自动找场景 Canvas）。
public class InventoryChangedToast : MonoBehaviour
{
    public static InventoryChangedToast Instance;

    private Text _text;
    private Coroutine _routine;

    private void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;

        // 找个可用的 Canvas（UICanvas / Canvas）
        var canvas = FindObjectOfType<Canvas>();
        var go = new GameObject("PickupToast", typeof(Text));
        go.transform.SetParent(canvas.transform, false);

        // 居中偏上
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.6f);
        rect.sizeDelta = new Vector2(600, 80);
        rect.localScale = Vector3.one;

        _text = go.GetComponent<Text>();
        _text.fontSize = 36;
        _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _text.alignment = TextAnchor.MiddleCenter;
        _text.color = Color.white;
        _text.text = "";
        _text.horizontalOverflow = HorizontalWrapMode.Overflow;
        _text.verticalOverflow = VerticalWrapMode.Overflow;
        _text.raycastTarget = false;
    }

    public void Show(string message)
    {
        if (_text == null) return;
        _text.text = message;
        _text.color = Color.white;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(1.2f);
        // 渐隐
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            _text.color = new Color(1, 1, 1, 1 - t / 0.5f);
            yield return null;
        }
        _text.text = "";
    }
}
