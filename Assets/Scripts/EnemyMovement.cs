using System.Collections;
using UnityEngine;

/// 敌人状态机：待机→追击→攻击三种状态互斥
public enum EnemyState
{
    Stand = 0,  // 待机，玩家未进入检测范围
    Chase = 1,  // 追击，向玩家移动
    Attack = 2, // 攻击，停止移动并出招
}

/// 敌人移动AI：状态机驱动移动、朝向、攻击切换、受击击退
public class EnemyMovement : MonoBehaviour
{
    private float distance = 10; // 与玩家的距离，初始设大值避免开局误判进入攻击范围
    public float speed = 2;
    public Animator enemyAnim;
    public EnemyCombat enemyCombat;
    private Transform player;
    private Rigidbody2D rb;
    private int facingDirection = 1;
    private bool enmity; // 是否已发现玩家（进入过检测范围）
    private bool isAttacked = false; // 被击退时锁定行为
    EnemyState state = EnemyState.Stand;

    // 对外暴露玩家引用，供EnemyCombat读取距离
    public Transform Player => player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCombat = GetComponent<EnemyCombat>();
        facingDirection = transform.localScale.x > 0 ? 1 : -1;
    }

    void Update()
    {
        if (!isAttacked)
        {
            if (player != null) distance = (player.position - transform.position).magnitude;

            // 在攻击范围外且已发现玩家 → 切到追击
            // 在攻击范围内 → 尝试攻击，冷却好了切到攻击
            if (state != EnemyState.Attack && distance <= enemyCombat.attackRange)
            {
                if (enemyCombat.TryAttack())
                {
                    state = EnemyState.Attack;
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

    // 动画事件回调 (GoblinCombat.anim) → 攻击动画播完时调用，切回待机状态
    public void ResetToStand()
    {
        state = EnemyState.Stand;
        rb.velocity = Vector2.zero;
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

    // 被玩家击退时调用：锁定行为，施加击退力，硬直结束后解锁
    public void KnockBack(Transform player, float force, float stunTime)
    {
        isAttacked = true;
        Vector2 direction = (transform.position - player.position).normalized;
        rb.velocity = direction * force;
        StartCoroutine(UnlockedMove(stunTime));
    }

    // 协程：等待硬直时间后停止移动并解锁
    IEnumerator UnlockedMove(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.velocity = Vector2.zero;
        isAttacked = false;
    }
}
