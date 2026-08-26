using UnityEngine;

/// 敌人攻击系统：管理攻击冷却和伤害判定
public class EnemyCombat : MonoBehaviour
{
    [Header("攻击数值")]
    public int damage = 1;
    public float knockBackForce = 1.5f;
    public float stunTime = 0.5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("引用")]
    public PlayerHealth playerHealth;
    public PlayerMove playerMove;

    private EnemyMovement enemyMovement;
    private float lastAttackTime = -10f; // 初始设为负数，开局即可攻击

    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    // 尝试攻击：冷却中返回false，否则记录时间并返回true
    public bool TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return false;

        lastAttackTime = Time.time;
        return true;
    }

    // 动画事件回调 (GoblinCombat.anim) → 攻击命中帧触发，检查距离后扣血+击退玩家
    public void Damage()
    {
        if (enemyMovement.Player == null) return;

        float distance = (enemyMovement.Player.position - transform.position).magnitude;

        // playerHealth可能未在Inspector中拖入，运行时动态获取
        if (playerHealth == null)
        {
            Debug.LogWarning("玩家的PlayerHealth组件未找到！", this);
            if (enemyMovement.Player != null)
            {
                playerHealth = enemyMovement.Player.GetComponent<PlayerHealth>();
            }
            return;
        }

        if (distance <= attackRange)
        {
            playerHealth.ChangeHealth(-damage);
            Debug.Log($"敌人攻击玩家，造成{damage}点伤害-----------------");
            playerMove.KnockBack(transform, knockBackForce, stunTime);
        }
    }
}
