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
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // 订阅升级事件 — 升级后maxHealth变化时自动刷新HP显示
        StatsManager.OnStatsChanged += RefreshHealthUI;
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    /// 受击统一入口：扣血 + 击退 + 无敌帧 + 打击感
    public void TakeDamage(int damage, Transform source, bool isCrit = false)
    {
        if (isInvincible) return;

        ChangeHealth(-damage);

        // 音效：受击
        AudioManager.Hurt();

        // 飘字：红色，在玩家头顶
        if (DamageTextSpawner.Instance != null)
            DamageTextSpawner.Instance.Spawn(transform.position, damage, false);

        // 顿帧：玩家受击 0.05s
        if (HitstopController.Instance != null)
            HitstopController.Instance.Hitstop(0.05f);

        // 屏幕震动：中幅
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.1f, 0.1f);

        // 击退
        GetComponent<PlayerMove>().KnockBack(source, StatsManager.Instance.KnockBackForce, StatsManager.Instance.stunTime);

        // 无敌帧
        StartCoroutine(InvincibilityFrames(0.5f));
    }

    /// 无敌帧：受击后短暂无敌 + 红色闪烁
    IEnumerator InvincibilityFrames(float duration)
    {
        isInvincible = true;

        if (spriteRenderer != null)
        {
            int blinks = 5;
            float blinkInterval = duration / (blinks * 2);
            for (int i = 0; i < blinks; i++)
            {
                spriteRenderer.color = new Color(1, 0.3f, 0.3f, 0.5f);
                yield return new WaitForSeconds(blinkInterval);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(blinkInterval);
            }
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        isInvincible = false;
    }
}
