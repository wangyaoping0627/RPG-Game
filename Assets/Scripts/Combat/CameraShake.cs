using System.Collections;
using UnityEngine;

/// 屏幕震动：挂在主相机上，命中时短暂抖动
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;
    private bool isShaking = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        originalPos = transform.localPosition;
    }

    /// 触发屏幕震动
    /// duration: 震动持续时间  magnitude: 震动幅度
    public void Shake(float duration, float magnitude)
    {
        if (!isShaking)
            StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 offset = Random.insideUnitCircle * magnitude;
            transform.localPosition = originalPos + new Vector3(offset.x, offset.y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        isShaking = false;
    }
}