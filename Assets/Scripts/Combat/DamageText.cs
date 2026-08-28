using TMPro;
using UnityEngine;

/// 伤害飘字：上浮 + 淡出，暴击显示更大更亮
public class DamageText : MonoBehaviour
{
    public TMP_Text text;
    public float floatSpeed = 60f;
    public float lifeTime = 0.8f;

    private float elapsed = 0f;

    private void Awake()
    {
        // 自动获取同物体上的 TMP_Text，防止 Inspector 漏赋值
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    public void Setup(int damage, bool isCrit)
    {
        if (text == null) return;

        text.text = damage.ToString();

        if (isCrit)
        {
            text.color = new Color(1f, 0.85f, 0f);       // 金色
            text.fontSize = 68;
            text.text += "!";                              // 暴击加感叹号
        }
        else
        {
            text.color = Color.white;
            text.fontSize = 40;
        }

        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    private void Update()
    {
        if (text == null) { Destroy(gameObject); return; }

        // 上浮
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 淡出：后半段开始透明度递减
        elapsed += Time.deltaTime;
        if (elapsed > lifeTime * 0.5f)
        {
            float alpha = 1f - (elapsed - lifeTime * 0.5f) / (lifeTime * 0.5f);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
        }

        if (elapsed >= lifeTime)
            Destroy(gameObject);
    }
}