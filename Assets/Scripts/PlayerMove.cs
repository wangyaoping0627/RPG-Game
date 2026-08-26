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
        // 击退状态：移动和攻击都锁定
        if (isAttacked) return;

        // 攻击输入（非击退状态下才处理）
        if (Input.GetButtonDown("Slash"))
        {
            playerCombat.Attack();
        }

        // 攻击前摇和判定帧：移动锁定（后摇期间可以移动）
        if (playerCombat.IsAttackLocked)
        {
            anim.SetFloat("horizontal", 0);
            anim.SetFloat("vertical", 0);
            RB.velocity = Vector2.zero;
            return;
        }

        //接收键盘输入的方向信息
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

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

    // 通过翻转scale.x实现朝向切换
    void Turn()
    {
        //改变朝向标识
        facingDirection *= -1;
        //改变朝向
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    // 被击退时调用：锁定移动，施加击退力（EaseOut 减速曲线），硬直结束后解锁
    public void KnockBack(Transform source, float force, float stunTime)
    {
        isAttacked = true;
        Vector2 direction = ((Vector2)(transform.position - source.position)).normalized;
        StartCoroutine(KnockbackCoroutine(direction, force, stunTime));
    }

    // 协程：击退力随时间衰减，自然减速
    IEnumerator KnockbackCoroutine(Vector2 direction, float force, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // EaseOut 曲线：开始快，结束慢
            float currentForce = force * (1f - t * t);
            RB.velocity = direction * currentForce;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        RB.velocity = Vector2.zero;
        isAttacked = false;
    }
}