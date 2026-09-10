using System.Collections;
using UnityEngine;

/// Boss 技能控制器：冲刺斩 / 砸地 AOE / 召唤，血量低于阈值进入狂暴
/// 挂在 Boss 身上（与 EnemyMovement / EnemyHealth / EnemyCombat 共存）。
/// 技能用协程按顺序循环释放，释放期间锁定 EnemyMovement 的移动。
public class BossController : MonoBehaviour
{
    [Header("技能参数")]
    public float skillInterval = 3f;      // 两次技能之间的间隔
    public float detectRange = 8f;        // 玩家进入此范围才放技能
    public float dashWindup = 0.4f;       // 冲刺前摇
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float aoeWarningTime = 1f;     // 砸地预警时长
    public float aoeRadius = 2f;
    public float aoeDamage = 25f;

    [Header("引用")]
    public GameObject warningCircle;      // 砸地预警圈（半透明圆 Sprite）
    public GameObject minionPrefab;       // 召唤的小怪
    public Transform[] summonPoints;      // 召唤出生位

    [Header("狂暴")]
    public float rageThreshold = 0.5f;    // 血量低于此比例进入狂暴
    public float rageSpeedMul = 1.3f;
    public float rageCooldownMul = 0.7f;

    private Rigidbody2D rb;
    private EnemyMovement movement;
    private EnemyHealth health;
    private Transform player;
    private bool enraged;
    private bool busy;   // 技能释放中

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<EnemyMovement>();
        health = GetComponent<EnemyHealth>();

        var ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) player = ph.transform;

        StartCoroutine(SkillLoop());
    }

    private void Update()
    {
        if (enraged || health == null) return;

        if (health.currentHealth <= health.maxHealth * rageThreshold)
        {
            enraged = true;
            if (movement != null)
            {
                movement.speed *= rageSpeedMul;
                movement.attackCooldown *= rageCooldownMul;
            }
        }
    }

    /// 技能循环：等间隔依次放 冲刺 → 砸地 → 召唤
    private IEnumerator SkillLoop()
    {
        yield return new WaitForSeconds(skillInterval); // 开局给玩家反应时间

        int step = 0;
        while (true)
        {
            if (!busy && health != null && health.IsAlive && PlayerInRange())
            {
                busy = true;
                SetLocked(true);

                switch (step % 3)
                {
                    case 0: yield return StartCoroutine(Dash()); break;
                    case 1: yield return StartCoroutine(Slam()); break;
                    default: yield return StartCoroutine(Summon()); break;
                }

                SetLocked(false);
                busy = false;
                step++;
            }

            yield return new WaitForSeconds(skillInterval);
        }
    }

    private bool PlayerInRange()
    {
        if (player == null)
        {
            var ph = FindObjectOfType<PlayerHealth>();
            if (ph == null) return false;
            player = ph.transform;
        }
        return Vector2.Distance(transform.position, player.position) <= detectRange;
    }

    private void SetLocked(bool locked)
    {
        if (movement != null) movement.SetLocked(locked); // 锁住 EnemyMovement 的 AI 移动
        if (rb != null) rb.velocity = Vector2.zero;
    }

    /// 技能 1：冲刺斩
    private IEnumerator Dash()
    {
        yield return new WaitForSeconds(dashWindup);
        if (player == null || rb == null) yield break;

        Vector2 dir = ((Vector2)(player.position - transform.position)).normalized;

        float t = 0f;
        while (t < dashDuration)
        {
            rb.velocity = dir * dashSpeed;
            t += Time.deltaTime;
            yield return null;
        }
        rb.velocity = Vector2.zero;
    }

    /// 技能 2：砸地 AOE（预警圈 → 延迟 → 圆形范围伤害）
    private IEnumerator Slam()
    {
        GameObject circle = null;
        if (warningCircle != null)
            circle = Instantiate(warningCircle, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(aoeWarningTime);

        if (circle != null) Destroy(circle);

        var hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (var h in hits)
        {
            var ph = h.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(Mathf.RoundToInt(aoeDamage), transform);
        }

        if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.3f, 0.2f);
    }

    /// 技能 3：召唤小怪
    private IEnumerator Summon()
    {
        if (minionPrefab != null && summonPoints != null)
        {
            foreach (var p in summonPoints)
                if (p != null) Instantiate(minionPrefab, p.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(0.5f);
    }
}
