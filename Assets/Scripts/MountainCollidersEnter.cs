using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 进入高处区域：禁用山体碰撞、启用边界碰撞、提高渲染层级
/// 实现"角色走到山体后面"的遮挡效果
public class MountainCollidersEnter : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Enemy")
        {

            foreach (Collider2D mountain in mountainColliders)
            {
                mountain.enabled = false;
            }
            foreach (Collider2D boundary in boundaryColliders)
            {
                boundary.enabled = true;
            }
            // 提高渲染层级，让角色渲染在山体上方
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 15;
        }
        
    }
    
}
