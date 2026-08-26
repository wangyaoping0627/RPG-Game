using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// 玩家血量系统：管理生命值并同步更新UI
public class PlayerHealth : MonoBehaviour
{
    public TMP_Text healthUI;
    public Animator healthUpdate;
    private void Start()
    {
        // 订阅升级事件 — 升级后maxHealth变化时自动刷新HP显示
        StatsManager.OnStatsChanged += RefreshHealthUI;
        RefreshHealthUI();
    }
    private void OnDestroy()
    {
        // 取消订阅，避免对象销毁后事件仍尝试回调
        StatsManager.OnStatsChanged -= RefreshHealthUI;
    }
    // 刷新HP文本，供初始化/受伤/升级共用
    private void RefreshHealthUI()
    {
        healthUI.text = "HP:" + StatsManager.Instance.currentHealth + "/" + StatsManager.Instance.maxHealth;
    }
    // amount为负=受伤，为正=治疗
    public void ChangeHealth(int amount)
    {
        StatsManager.Instance.currentHealth += amount;
        if (StatsManager.Instance.currentHealth > StatsManager.Instance.maxHealth) StatsManager.Instance.currentHealth = StatsManager.Instance.maxHealth;
        else if (StatsManager.Instance.currentHealth <= 0) {
            StatsManager.Instance.currentHealth = 0;
            gameObject.SetActive(false);
        }
        healthUpdate.Play("HealthUpdate");
        RefreshHealthUI();
    }
}
