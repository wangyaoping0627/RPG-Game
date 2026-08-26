using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 攻击阶段：前摇 → 判定帧 → 后摇
public enum AttackPhase { Idle, Windup, Active, Recovery }

/// 玩家攻击系统：状态机驱动攻击阶段，Collider2D 判定框替代 OverlapCircle，支持输入缓存
public class PlayerCombat : MonoBehaviour
{
    [Header("引用")]
    public Animator playerAnim;
    public Collider2D hitboxCollider; // 判定框（AttackPoint 子物体上的 Trigger Collider2D）
    public float hitboxActiveDuration = 0.1f; // 判定帧持续时长

    [Header("攻击范围")]
    public Transform attackPoint;     // 保留，用于 Gizmo 可视化
    public LayerMask enemyLayer;      // 保留，用于 Gizmo 可视化

    private AttackPhase phase = AttackPhase.Idle;
    private bool inputBuffered = false; // 输入缓存
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // 本次攻击已命中的目标
    private float attackStartTime; // 攻击开始时间，用于超时保底
    private const float MaxAttackDuration = 1.5f; // 单次攻击最长持续时间，超过则强制重置

    /// 当前攻击阶段（外部只读）
    public AttackPhase Phase => phase;
    /// 是否在前摇或判定帧（锁定移动）
    public bool IsAttackLocked => phase == AttackPhase.Windup || phase == AttackPhase.Active;
    /// 是否正在攻击中
    public bool IsAttacking => phase != AttackPhase.Idle;

    private void Start()
    {
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }

    private void Update()
    {
        // 超时保底：如果攻击持续超过 MaxAttackDuration（FinishAttack 动画事件未触发的安全兜底）
        if (phase != AttackPhase.Idle && Time.time - attackStartTime > MaxAttackDuration)
        {
            ForceReset();
        }
    }

    /// 强制重置到 Idle（超时保底 / 外部调用）
    private void ForceReset()
    {
        phase = AttackPhase.Idle;
        playerAnim.SetBool("isAttack", false);
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
        inputBuffered = false;
    }

    /// 外部调用：尝试攻击，处理输入缓存
    public void Attack()
    {
        if (phase == AttackPhase.Idle)
        {
            StartAttack();
        }
        else
        {
            // 攻击中按键 → 缓存输入，后摇结束时自动触发下一次
            inputBuffered = true;
        }
    }

    private void StartAttack()
    {
        phase = AttackPhase.Windup;
        attackStartTime = Time.time;
        playerAnim.SetBool("isAttack", true);
    }

    // === 动画事件回调 ===

    // 动画事件：前摇结束 → 进入判定帧，启用判定框
    public void DealDamage()
    {
        if (phase != AttackPhase.Windup) return;

        phase = AttackPhase.Active;

        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
            hitTargets.Clear();
        }

        StartCoroutine(EndActivePhase());
    }

    // 判定帧持续时间结束后 → 禁用判定框，进入后摇
    private IEnumerator EndActivePhase()
    {
        yield return new WaitForSecondsRealtime(hitboxActiveDuration);

        if (hitboxCollider != null)
            hitboxCollider.enabled = false;

        phase = AttackPhase.Recovery;
    }

    // 动画事件：后摇结束 → 回到 Idle，检查输入缓存
    public void FinishAttack()
    {
        phase = AttackPhase.Idle;
        playerAnim.SetBool("isAttack", false);

        // 如果缓存了输入，立即开始下一次攻击
        if (inputBuffered)
        {
            inputBuffered = false;
            StartAttack();
        }
    }

    // 判定框碰到敌人时造成伤害（OnTriggerEnter2D 由 Player 的 Rigidbody2D 转发）
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (phase != AttackPhase.Active) return;
        if (!other.CompareTag("Enemy")) return;
        if (hitTargets.Contains(other.gameObject)) return;

        var enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null && enemyHealth.IsAlive)
        {
            int damage = CombatCalculator.CalculateDamage(
                StatsManager.Instance.damage,
                enemyHealth.defense,
                StatsManager.Instance.critRate,
                StatsManager.Instance.critMultiplier,
                out bool isCrit);

            enemyHealth.TakeDamage(damage, transform, isCrit);
            hitTargets.Add(other.gameObject);

            // === 打击感三件套 ===
            // 飘字：在敌人头顶弹出伤害数字
            if (DamageTextSpawner.Instance != null)
                DamageTextSpawner.Instance.Spawn(other.transform.position, damage, isCrit);
            // 顿帧：暴击 0.08s，普通 0.03s
            if (HitstopController.Instance != null)
                HitstopController.Instance.Hitstop(isCrit ? 0.08f : 0.03f);
            // 屏幕震动：暴击大幅，普通小幅
            if (CameraShake.Instance != null)
                CameraShake.Instance.Shake(isCrit ? 0.15f : 0.05f, isCrit ? 0.15f : 0.05f);
        }
    }

    // 编辑器中可视化攻击范围
    private void OnDrawGizmosSelected()
    {
        if (hitboxCollider != null)
        {
            Gizmos.color = Color.red;
            var bounds = hitboxCollider.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
        else if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, StatsManager.Instance != null ? StatsManager.Instance.weaponRange : 2f);
        }
    }
}