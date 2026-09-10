using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// 刷怪点：开局刷满 maxAlive 只，敌人死亡后冷却 respawnCooldown 秒补刷
/// 用法：场景空物体挂本脚本，enemyPrefab 拖敌人预制体，spawnPoints 拖出生位（留空则用自身位置）
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int maxAlive = 3;               // 同时存活上限
    public float respawnCooldown = 60f;    // 死亡后补刷冷却（秒）
    public Transform[] spawnPoints;        // 出生位，留空则用自身位置

    private readonly List<EnemyHealth> alive = new List<EnemyHealth>();

    private void Start()
    {
        for (int i = 0; i < maxAlive; i++) Spawn();
    }

    private void OnDestroy() => UnsubscribeAll();

    private void Spawn()
    {
        if (enemyPrefab == null) return;

        Vector3 pos = (spawnPoints != null && spawnPoints.Length > 0)
            ? spawnPoints[Random.Range(0, spawnPoints.Length)].position
            : transform.position;

        var go = Instantiate(enemyPrefab, pos, Quaternion.identity);

        var hp = go.GetComponent<EnemyHealth>();
        if (hp != null)
        {
            alive.Add(hp);
            hp.OnDeath += OnEnemyDeath; // 订阅该实例的死亡事件
        }
    }

    private void OnEnemyDeath(int exp)
    {
        Prune();
        StartCoroutine(RespawnLater());
    }

    private IEnumerator RespawnLater()
    {
        yield return new WaitForSeconds(respawnCooldown);
        Prune();
        if (alive.Count < maxAlive) Spawn();
    }

    /// 剔除已死亡/已销毁的引用
    private void Prune()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i] == null || !alive[i].IsAlive)
            {
                if (alive[i] != null) alive[i].OnDeath -= OnEnemyDeath;
                alive.RemoveAt(i);
            }
        }
    }

    private void UnsubscribeAll()
    {
        foreach (var hp in alive)
            if (hp != null) hp.OnDeath -= OnEnemyDeath;
        alive.Clear();
    }
}
