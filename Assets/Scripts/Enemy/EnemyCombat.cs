using UnityEngine;

/// 敌人攻击系统：管理伤害判定
public class EnemyCombat : MonoBehaviour
{
    [Header("攻击数值")]
    public int damage = 1;
    public float attackRange = 1.5f;

    [Header("引用")]
    public PlayerHealth playerHealth;

    private EnemyMovement enemyMovement;

    void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    // 动画事件回调 → 攻击命中帧触发，检查距离后扣血
    public void Damage()
    {
        if (enemyMovement.Player == null) return;

        float distance = (enemyMovement.Player.position - transform.position).magnitude;

        // playerHealth可能未在Inspector中拖入，运行时动态获取
        if (playerHealth == null)
        {
            if (enemyMovement.Player != null)
            {
                playerHealth = enemyMovement.Player.GetComponent<PlayerHealth>();
            }
            if (playerHealth == null) return;
        }

        if (distance <= attackRange)
        {
            playerHealth.TakeDamage(damage, transform);
            Debug.Log($"敌人攻击玩家，造成{damage}点伤害");
        }
    }
}
