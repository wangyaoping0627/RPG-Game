using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
    {
        Stand=0,
        Chase=1,
        Attack=2,
    }
public class EnemyMovement : MonoBehaviour
{
    
    private float attackCooldown = 1.5f;      // 攻击间隔（秒）
    private float lastAttackTime = -10f;      // 上次攻击时间
    public int damage = 1;
    public float knockBackForce = 1.5f;
    public float stunTime = 0.5f;
    private float distance = 10;
    public float speed = 2;
    public Animator enemyAnim;
    private Transform player;
    private Rigidbody2D rb;
    private int facingDirection = 1;
    private float attackRange = 1.5f;//定义攻击范围
    public PlayerHealth playerHealth;
    public PlayerMove playerMove;
    private bool enmity;//判断是否在仇恨范围内
    private bool isAttacked = false;
    EnemyState state=EnemyState.Stand;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        facingDirection = transform.localScale.x > 0 ? 1 : -1;
    }
    void Update()
    {
        if (!isAttacked)
        {
            if (player != null) distance = (player.position - transform.position).magnitude;

            // 如果不在攻击状态且进入攻击范围，触发攻击
            if (state!=EnemyState.Attack && distance <= attackRange)
            {
                AttackAnim();
            }
            // 如果不在攻击状态且正在追逐，才移动
            else if (state==EnemyState.Chase)
            {
                ChaseTurn();
            }
            else if (distance > attackRange && enmity == true)
            {
                state = EnemyState.Chase;
            }

            ToAnimator();
        }
    }
    //判断玩家进入仇恨范围
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (player == null)
            {
                player = collision.transform;
            }
            state=EnemyState.Chase;
            enmity = true;
        }
    }
    //判断玩家退出仇恨范围
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            state = EnemyState.Stand;
            enmity = false;
            player = null;
            //玩家退出仇恨范围后移速设置为零
            rb.velocity = new Vector2(0, 0);
        }

    }
    //此方法用来实现追逐 转向
    void ChaseTurn()
    {
         Vector2 direction = (player.position - transform.position).normalized;
        if (player.position.x < transform.position.x && facingDirection == 1)
        {
            Turn(); // 玩家在左侧，且当前朝右 → 转向左
        }
        else if (player.position.x > transform.position.x && facingDirection == -1)
        {
            Turn(); // 玩家在右侧，且当前朝左 → 转向右
        }
           
            rb.velocity = direction * speed;
        
    }
    void AttackAnim()
    {
        if (Time.time - lastAttackTime < attackCooldown) return; // 冷却中

        lastAttackTime = Time.time;
        state=EnemyState.Attack;
        rb.velocity = Vector2.zero;
        // 攻击动画触发，动画事件结束时调用 ResetToStand
    }
    void Damage()
    {
        if (player != null)
        {
            distance = (player.position - transform.position).magnitude;
        }
        // 第一步：检查玩家血量组件是否存在，避免空引用
        if (playerHealth == null)
        {
            Debug.LogWarning("玩家的PlayerHealth组件未找到！", this);
            // 兜底：重新尝试获取
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
            return;
        }

        // 第二步：调用玩家的扣血方法
        if (distance <= attackRange)//添加IF判断 如果攻击时人物离开攻击范围 则没打出伤害
        {
            playerHealth.ChangeHealth(-damage);
            Debug.Log($"敌人攻击玩家，造成{damage}点伤害");
            playerMove.KnockBack(transform,knockBackForce,stunTime);
        }
    }//这个方法在动画界面添加事件调用

    public void ResetToStand()
    {
       // Debug.Log("ResetToStand 被调用");
        state=EnemyState.Stand;
        rb.velocity = Vector2.zero;
    }


    //此方法用来上传状态数据到动画控制
   private void ToAnimator()
    {
       enemyAnim.SetInteger("EnemyState",(int)state);
    }


    void Turn()
    {
        //改变朝向标识
        facingDirection *= -1;
        //改变朝向
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }//改变朝向


    public void KnockBack(Transform player,float force,float stunTime)
    {
        isAttacked=true;
        Vector2 direction = (transform.position - player.position).normalized;
        rb.velocity = direction * force;
        StartCoroutine(UnlockedMove(stunTime));
    }


    IEnumerator UnlockedMove(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.velocity = Vector2.zero;
        isAttacked = false;
    }
}
