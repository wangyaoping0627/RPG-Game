using UnityEngine;

/// 音频管理器：单例，统一管理 BGM 与 SFX
/// 音频片段留空时静默跳过，不影响游戏运行（方便先跑通再接音频资源）
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音源")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip bgmMenu;
    public AudioClip bgmBattle;

    [Header("SFX")]
    public AudioClip sfxHit;      // 普通命中
    public AudioClip sfxCrit;     // 暴击命中
    public AudioClip sfxHurt;     // 玩家受击
    public AudioClip sfxPickup;   // 拾取
    public AudioClip sfxLevelUp;  // 升级
    public AudioClip sfxDeath;    // 敌人死亡

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return; // 同一首不重播

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // === 便捷方法（供外部调用；Instance 为空时静默忽略）===
    public static void PlayMenuBGM() => Instance?.PlayBGM(Instance.bgmMenu);
    public static void PlayBattleBGM() => Instance?.PlayBGM(Instance.bgmBattle);
    public static void Hit(bool isCrit) => Instance?.PlaySFX(isCrit ? Instance.sfxCrit : Instance.sfxHit);
    public static void Hurt() => Instance?.PlaySFX(Instance.sfxHurt);
    public static void Pickup() => Instance?.PlaySFX(Instance.sfxPickup);
    public static void LevelUp() => Instance?.PlaySFX(Instance.sfxLevelUp);
    public static void EnemyDeath() => Instance?.PlaySFX(Instance.sfxDeath);
}
