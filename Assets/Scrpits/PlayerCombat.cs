using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator playerAnim;
    public float cooldown=2;//冷却时间
    private float timer = 0;//倒计时器  
    public EnemyHealth enemyHealth;
    public Transform attackPoint;
    public LayerMask enemyLayer;
    public EnemyMovement enemyMovement;
    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            
        }
    }


    public void Attack()
    {
        if (timer <= 0)
        {
            playerAnim.SetBool("isAttack", true);
            timer = cooldown;
        }
    }
    public void FinishAttack()
    {
        playerAnim.SetBool("isAttack", false);
    }
    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, StatsManager.Instance.weaponRange, enemyLayer);
        if (enemies.Length > 0)
        {
            enemies[0].GetComponent<EnemyMovement>().KnockBack(transform, StatsManager.Instance.KnockBackForce, StatsManager.Instance.stunTime);
            enemies[0].GetComponent<EnemyHealth>().ChangeHealth(-StatsManager.Instance.damage);
            Debug.Log("造成伤害");

        }

    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, StatsManager.Instance.weaponRange);
    }

}
