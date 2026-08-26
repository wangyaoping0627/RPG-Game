using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// 敌人血量系统：管理生命值、血条UI、死亡事件广播
public class EnemyHealth : MonoBehaviour
{
    public int currentHealth = 10;
    public int maxHealth = 10;
    public int expValue = 3; // 击杀后给玩家的经验值
    public Image Image; // 血条fill
    public GameObject BloodUI; // 血条UI根节点

    // 实例事件：当前敌人死亡时触发
    public event Action<int> OnDeath;
    // 静态事件：任意敌人死亡时触发，供ExpController订阅
    public static event Action<int> OnAnyEnemyDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    // amount为负=受伤，为正=治疗
    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // 广播死亡事件，传递经验值给订阅者
            OnDeath?.Invoke(expValue);
            OnAnyEnemyDeath?.Invoke(expValue);
            gameObject.SetActive(false);
            BloodUI.gameObject.SetActive(false);
        }

        Image.fillAmount = (float)currentHealth / maxHealth;
    }
}
