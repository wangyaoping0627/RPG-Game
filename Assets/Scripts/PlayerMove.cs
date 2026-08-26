using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 玩家移动系统：输入处理、移动、朝向翻转、受击击退
public class PlayerMove : MonoBehaviour
{
    bool isAttacked = false; // 被击退时锁定移动
    public Rigidbody2D RB;
    public Animator anim;
    public int facingDirection = 1;   
    public PlayerCombat playerCombat;
   
    // 物理移动放在FixedUpdate，与物理引擎同步
    void FixedUpdate()
    {
        if(isAttacked == false)
            {
            //接收键盘输入的方向信息
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if (Input.GetButtonDown("Slash"))
            {
                playerCombat.Attack();
            }
            //if语句里判断依据是走向与朝向不一样

            bool isBoosting = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // 移动方向与当前朝向不一致时翻转
            if (horizontal > 0 && transform.localScale.x < 0
                ||
                horizontal < 0 && transform.localScale.x > 0)
                Turn();

            //将键盘输入移动的数据传输到Anmator
            anim.SetFloat("horizontal", Mathf.Abs(horizontal));
            anim.SetFloat("vertical", Mathf.Abs(vertical));
            //按住加速键加速
            if (isBoosting)
            {
                RB.velocity = new Vector2(horizontal * StatsManager.Instance.speed * 2, vertical * StatsManager.Instance.speed * 2);
            }
            else
            {
                RB.velocity = new Vector2(horizontal * StatsManager.Instance.speed, vertical * StatsManager.Instance.speed);
            }
        }
    }
    
    // 通过翻转scale.x实现朝向切换
    void Turn()
    {
        //改变朝向标识
        facingDirection *= -1;
        //改变朝向
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
    // 被敌人击退时调用：锁定移动，施加击退力，硬直结束后解锁
   public void KnockBack(Transform enemy,float force,float stunTime)
    {
       isAttacked = true;
        Vector2 direction = (transform.position - enemy.position).normalized;
        RB.velocity = direction * force;
        StartCoroutine(UnlockedMove(stunTime));
   }
    // 协程：等待硬直时间后停止移动并解锁
    IEnumerator UnlockedMove(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        RB.velocity = Vector2.zero;
        isAttacked = false;
    }
}
