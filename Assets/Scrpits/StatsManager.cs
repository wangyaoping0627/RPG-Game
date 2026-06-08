using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    //数值管理单例
    public static StatsManager Instance;
    [Header("战斗数值")]
    public int damage = 2;
    public float stunTime = 0.5f;
    public float KnockBackForce = 2;
    public float weaponRange = 2 ;
    [Header("生命数值")]
    public int maxHealth;
    public int currentHealth;
    [Header("移动数值")]
    public float speed = 5;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
}
