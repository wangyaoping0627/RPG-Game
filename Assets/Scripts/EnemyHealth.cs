using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// 敌人血量系统：TakeDamage 统一入口，受击硬直，死亡状态，事件广播
public class EnemyHealth : MonoBehaviour
{
    [Header("血量")]
    public int currentHealth = 10;
    public int maxHealth = 10;
    public int defense = 2;
    public int expValue = 3; // 击杀后给玩家的经验值

    [Header("受击参数")]
    public float hitStunDuration = 0.3f;
    public float knockbackForce = 2f;

    [Header("UI")]
    public Image Image; // 血条fill
    public GameObject BloodUI; // 血条UI根节点

    // 实例事件：当前敌人死亡时触发
    public event Action<int> OnDeath;
    // 静态事件：任意敌人死亡时触发，供ExpController订阅
    public static event Action<int> OnAnyEnemyDeath;

    private EnemyMovement enemyMovement;
    private bool isDead = false;

    /// 是否存活
    public bool IsAlive => !isDead && currentHealth > 0;

    private void Start()
    {
        currentHealth = maxHealth;
        enemyMovement = GetComponent<EnemyMovement>();
    }

    /// 受击统一入口：扣血 → 受击硬直 / 死亡
    public void TakeDamage(int damage, Transform source, bool isCrit)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (Image != null)
            Image.fillAmount = (float)currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            EnterDeath();

            // 击杀反馈：更强顿帧 + 震动
            if (HitstopController.Instance != null)
                HitstopController.Instance.Hitstop(0.15f);
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(0.3f, 0.15f);
        }
        else
        {
            // 进入受击硬直 + 击退
            if (enemyMovement != null)
                enemyMovement.EnterHitStagger(source, knockbackForce, hitStunDuration);
        }
    }

    private void EnterDeath()
    {
        isDead = true;

        // 广播死亡事件，传递经验值给订阅者
        OnDeath?.Invoke(expValue);
        OnAnyEnemyDeath?.Invoke(expValue);

        // 通知状态机进入死亡
        if (enemyMovement != null)
            enemyMovement.EnterDeath();

        // 隐藏血条UI
        if (BloodUI != null)
            BloodUI.SetActive(false);

        // 保底：2秒后销毁，防止没有死亡动画事件导致敌人不消失
        StartCoroutine(DestroyAfterDelay(2f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // 动画事件：死亡动画播完 → 销毁
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}