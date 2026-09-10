using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 全局数值管理单例：存储玩家所有属性，跨场景不销毁
public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    // 升级时广播通知，订阅者（如PlayerHealth）自动刷新UI
    public static event Action OnStatsChanged;

    [Header("战斗数值")]
    public int damage = 10;
    public int defense = 0;
    public float stunTime = 0.5f;
    public float KnockBackForce = 2;
    public float weaponRange = 1.5f;
    public float critRate = 0.05f;
    public float critMultiplier = 1.5f;

    [Header("生命数值")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("移动数值")]
    public float speed = 5;

    [Header("经验数值")]
    public int currentExp = 0;
    public int maxExp = 10;
    public int level = 0;

    /// 外部触发属性变更广播（供装备系统等写回属性后刷新 UI）
    public void NotifyStatsChanged() => OnStatsChanged?.Invoke();

    public void GainExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= maxExp)
        {
            LevelUp();
        }
    }

    // 升级：提升等级/伤害/血量上限，重置经验并回满血
    private void LevelUp()
    {
        level++;
        currentExp -= maxExp;
        maxExp += level + 3;
        damage += 3;
        maxHealth += 20;
        currentHealth = maxHealth;
        AudioManager.LevelUp();
        OnStatsChanged?.Invoke();
    }

    private void Awake()
    {
        // 单例模式 — 第一个实例保留并跨场景存活，后续重复实例销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}
