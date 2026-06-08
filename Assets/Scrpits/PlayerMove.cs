using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    bool isAttacked = false;
    public Rigidbody2D RB;
    public Animator anim;
    public int facingDirection = 1;   
    public PlayerCombat playerCombat;
   
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
    
    void Turn()
    {
        //改变朝向标识
        facingDirection *= -1;
        //改变朝向
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
   public void KnockBack(Transform enemy,float force,float stunTime)
    {
       isAttacked = true;
        Vector2 direction = (transform.position - enemy.position).normalized;
        RB.velocity = direction * force;
        StartCoroutine(UnlockedMove(stunTime));
   }
    IEnumerator UnlockedMove(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        RB.velocity = Vector2.zero;
        isAttacked = false;
    }
}
