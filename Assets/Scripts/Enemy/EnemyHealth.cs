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

        // 血条初始化为满血，不依赖场景里手设的 Fill Amount
        if (Image != null) Image.fillAmount = 1f;
        if (BloodUI != null) BloodUI.SetActive(true);
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

        // 音效：敌人死亡
        AudioManager.EnemyDeath();

        // 广播死亡事件，传递经验值给订阅者
        OnDeath?.Invoke(expValue);
        OnAnyEnemyDeath?.Invoke(expValue);

        // 通知状态机进入死亡
        if (enemyMovement != null)
            enemyMovement.EnterDeath();

        // 掉落系统：若敌人带有 EnemyLoot 组件，按掉落表生成掉落物
        GetComponent<EnemyLoot>()?.SpawnDrops(transform.position);

        // 血条不在这里隐藏：血条是场景里的独立对象（靠 PositionConstraint 跟随敌人），
        // 此刻隐藏会造成「血条先消失、敌人（死亡动画播完/2 秒保底后）才消失」的不同步。
        // 改为在 OnDestroy 里随敌人一起销毁，保证两者同生共死。

        // 保底：2秒后销毁，防止没有死亡动画事件导致敌人不消失
        StartCoroutine(DestroyAfterDelay(2f));
    }

    private void OnDestroy()
    {
        // 血条是独立对象，不会随敌人自动销毁，这里让它和敌人同时消失。
        // 若以后把血条改成敌人的子物体，则无需处理（随父物体一起销毁）。
        if (BloodUI != null && BloodUI.transform.parent != transform)
            Destroy(BloodUI);
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