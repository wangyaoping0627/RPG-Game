using UnityEngine;

/// 飘字生成器：世界坐标转 UI 坐标，在指定位置生成伤害数字
public class DamageTextSpawner : MonoBehaviour
{
    public static DamageTextSpawner Instance;

    public GameObject damageTextPrefab;  // DamageText 预制体（TMP_Text 子物体）
    public Canvas targetCanvas;          // 目标 Canvas（Screen Space Overlay）

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// 在世界坐标处生成伤害飘字
    public void Spawn(Vector3 worldPosition, int damage, bool isCrit)
    {
        if (damageTextPrefab == null || targetCanvas == null) return;

        // 世界坐标 → 屏幕坐标
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        // 屏幕坐标 → Canvas 内的本地坐标
        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, null, out Vector2 localPos);

        // 生成飘字
        GameObject obj = Instantiate(damageTextPrefab, targetCanvas.transform);
        obj.GetComponent<RectTransform>().localPosition = localPos;

        // 在敌人头顶偏移一点
        var rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition += new Vector2(0, 30f);

        DamageText dt = obj.GetComponent<DamageText>();
        if (dt != null) dt.Setup(damage, isCrit);
    }
}