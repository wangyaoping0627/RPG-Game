using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// 经验/等级UI控制器：订阅杀敌事件自动加经验并刷新UI
public class ExpController : MonoBehaviour
{
    public Image expBar;
    public TMPro.TMP_Text levelText;

    private void Start()
    {
        // 订阅全局杀敌事件 — 任意敌人死亡时自动获得经验
        EnemyHealth.OnAnyEnemyDeath += OnEnemyKilled;
        RefreshUI();
    }

    private void OnDestroy()
    {
        // 取消订阅，避免对象销毁后事件仍尝试回调
        EnemyHealth.OnAnyEnemyDeath -= OnEnemyKilled;
    }

    private void OnEnemyKilled(int expValue)
    {
        StatsManager.Instance.GainExp(expValue);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (expBar != null)
            expBar.fillAmount = (float)StatsManager.Instance.currentExp / StatsManager.Instance.maxExp;
        if (levelText != null)
            levelText.text = "Lv." + StatsManager.Instance.level;
    }
}
