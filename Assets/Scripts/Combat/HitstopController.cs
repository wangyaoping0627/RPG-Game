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
        float prevTimeScale = Time.timeScale; // 记录进入前的 timeScale
        Time.timeScale = 0.25f;  // 减速到 25%，保留顿帧手感，不再降到 10% 造成明显卡顿
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = prevTimeScale; // 恢复原值，避免与暂停菜单(timeScale=0)打架
        isStopped = false;
    }
}