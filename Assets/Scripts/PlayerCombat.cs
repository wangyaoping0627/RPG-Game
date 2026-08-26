using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 玩家攻击系统：冷却计时、动画触发、范围伤害判定
public class PlayerCombat : MonoBehaviour
{
    public Animator playerAnim;
    public float cooldown=2;//冷却时间
    private float timer = 0;//倒计时器  
    public Transform attackPoint;
    public LayerMask enemyLayer;
    private void Update()
    {
        // 冷却计时器递减
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            
        }
    }


    public void Attack()
    {
        // 冷却结束后才能再次攻击
        if (timer <= 0)
        {
            playerAnim.SetBool("isAttack", true);
            timer = cooldown;
        }
    }
    // 动画事件回调 (PlayerCombat.anim) → 攻击动画播完时调用，退出攻击状态
    public void FinishAttack()
    {
        playerAnim.SetBool("isAttack", false);
    }
    // 动画事件回调 (PlayerCombat.anim) → 命中帧触发，对所有范围内敌人造成伤害和击退
    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, StatsManager.Instance.weaponRange, enemyLayer);
        foreach (Collider2D enemy in enemies)
        {
            enemy.GetComponent<EnemyMovement>().KnockBack(transform, StatsManager.Instance.KnockBackForce, StatsManager.Instance.stunTime);
            enemy.GetComponent<EnemyHealth>().ChangeHealth(-StatsManager.Instance.damage);
        }
        if (enemies.Length > 0)
            Debug.Log("造成伤害");
    }
    // 编辑器中可视化攻击范围
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, StatsManager.Instance.weaponRange);
    }

}
