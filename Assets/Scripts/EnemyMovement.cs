using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 敌人状态机：待机→追击→攻击→受击硬直→死亡
public enum EnemyState
{
    Stand = 0,      // 待机，玩家未进入检测范围
    Chase = 1,      // 追击，向玩家移动
    Attack = 2,     // 攻击，停止移动并出招
    HitStagger = 3, // 受击硬直，被攻击时打断当前动作
    Death = 4,      // 死亡，播放死亡动画后销毁
}

/// 敌人移动AI：状态机驱动移动、朝向、攻击切换、受击击退
public class EnemyMovement : MonoBehaviour
{
    private float distance = 10; // 与玩家的距离，初始设大值避免开局误判进入攻击范围
    public float speed = 2;
    public float attackCooldown = 0.4f; // 攻击间隔：一次攻击结束后需等待才能再次攻击
    public Animator enemyAnim;
    public EnemyCombat enemyCombat;
    private Transform player;
    private Rigidbody2D rb;
    private int facingDirection = 1;
    private bool enmity; // 是否已发现玩家（进入过检测范围）
    private bool isAttacked = false; // 被击退时锁定行为
    private float attackStartTime;  // 攻击开始时间，用于超时保底
    private float nextAttackTime;   // 下次允许攻击的时间（冷却）
    private const float MaxAttackDuration = 2f; // 单次攻击最长持续时间
    EnemyState state = EnemyState.Stand;

    // 对外暴露玩家引用，供EnemyCombat读取距离
    public Transform Player => player;
    // 对外暴露当前状态，供EnemyHealth调用
    public EnemyState State => state;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCombat = GetComponent<EnemyCombat>();
        facingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    void Update()
    {
        // 受击硬直和死亡状态下，跳过所有 AI 逻辑
        if (isAttacked || state == EnemyState.HitStagger || state == EnemyState.Death) return;

        // 攻击状态超时保底：ResetToStand 动画事件未触发则强制恢复
        if (state == EnemyState.Attack && Time.time - attackStartTime > MaxAttackDuration)
        {
            ResetToStand();
        }

        if (player != null) distance = (player.position - transform.position).magnitude;

        // 在攻击范围内：冷却结束才出招，冷却期间原地待命（不发呆也不贴脸挤）
        if (state != EnemyState.Attack && distance <= enemyCombat.attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                state = EnemyState.Attack;
                attackStartTime = Time.time;
                rb.velocity = Vector2.zero;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        else if (state == EnemyState.Chase)
        {
            ChaseTurn();
        }
        else if (distance > enemyCombat.attackRange && enmity == true)
        {
            state = EnemyState.Chase;
        }

        ToAnimator();
    }

    // 玩家进入检测范围 → 记录引用，切换到追击
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (player == null)
            {
                player = collision.transform;
            }
            state = EnemyState.Chase;
            enmity = true;
        }
    }

    // 玩家离开检测范围 → 回到待机，清空引用
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            state = EnemyState.Stand;
            enmity = false;
            player = null;
            rb.velocity = new Vector2(0, 0);
        }
    }

    // 追击：朝向玩家并移动
    void ChaseTurn()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        if (player.position.x < transform.position.x && facingDirection == 1)
        {
            Turn();
        }
        else if (player.position.x > transform.position.x && facingDirection == -1)
        {
            Turn();
        }
        rb.velocity = direction * speed;
    }

    // 动画事件回调 → 攻击动画播完时调用，进入攻击冷却
    public void ResetToStand()
    {
        state = EnemyState.Stand;
        nextAttackTime = Time.time + attackCooldown; // 攻击冷却，冷却内 Update 不会重进 Attack
        rb.velocity = Vector2.zero;
        ToAnimator();
    }

    // 同步状态到Animator
    private void ToAnimator()
    {
        enemyAnim.SetInteger("EnemyState", (int)state);
    }

    // 翻转scale.x实现朝向切换
    void Turn()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    // === 受击硬直：被玩家攻击时调用 ===
    public void EnterHitStagger(Transform source, float force, float stunTime)
    {
        state = EnemyState.HitStagger;
        enemyAnim.SetInteger("EnemyState", (int)EnemyState.HitStagger);
        KnockBack(source, force, stunTime);
    }

    // === 死亡状态 ===
    public void EnterDeath()
    {
        state = EnemyState.Death;
        rb.velocity = Vector2.zero;
        enemyAnim.SetInteger("EnemyState", (int)EnemyState.Death);
    }

    // 被击退时调用：锁定行为，施加击退力（EaseOut 减速曲线），硬直结束后恢复
    public void KnockBack(Transform player, float force, float stunTime)
    {
        isAttacked = true;
        Vector2 direction = (transform.position - player.position).normalized;
        StartCoroutine(KnockbackCoroutine(direction, force, stunTime));
    }

    // 协程：击退力随时间衰减，硬直结束后恢复到追击
    IEnumerator KnockbackCoroutine(Vector2 direction, float force, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // EaseOut 曲线：开始快，结束慢
            float currentForce = force * (1f - t * t);
            rb.velocity = direction * currentForce;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.velocity = Vector2.zero;
        isAttacked = false;

        // 受击硬直结束后恢复到追击（如果已经发现玩家）
        if (state == EnemyState.HitStagger)
        {
            state = enmity ? EnemyState.Chase : EnemyState.Stand;
        }
    }
}