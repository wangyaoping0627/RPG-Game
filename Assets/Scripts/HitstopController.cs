using System.Collections;
using UnityEngine;

/// 顿帧控制器：命中瞬间短暂减慢时间，增强打击分量感
public class HitstopController : MonoBehaviour
{
    public static HitstopController Instance;

    private bool isStopped = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// 触发顿帧
    /// duration: 顿帧持续时间（秒），用 WaitForSecondsRealtime 不受 timeScale 影响
    public void Hitstop(float duration)
    {
        if (!isStopped)
            StartCoroutine(HitstopCoroutine(duration));
    }

    private IEnumerator HitstopCoroutine(float duration)
    {
        isStopped = true;
        Time.timeScale = 0.1f;  // 减速到 10%，不完全停止保持微弱动态
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        isStopped = false;
    }
}