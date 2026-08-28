using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 离开高处区域：恢复山体碰撞、禁用边界碰撞、降低渲染层级
public class MountainCollidersExit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || collision.tag == "Enemy")
        {
         
                foreach (Collider2D mountain in mountainColliders)
                {
                    mountain.enabled = true;
                }
                foreach (Collider2D boundary in boundaryColliders)
                {
                    boundary.enabled = false;
                }
            // 降低渲染层级，让角色回到山体下方
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }
}
