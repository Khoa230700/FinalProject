using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemySoundController : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip attackClip;
    public AudioClip attackClip2;
    public AudioClip deathClip;

    [Header("Global Limits")]
    public static int CurrentPlayingSounds = 0;
    public static int MaxPlayingSounds = 1;
    public static int CurrentDeathSounds = 0;
    public static int MaxDeathSounds = 2;

    private AudioSource _audio;
    private bool _isDead;

    // Các cờ để biết đã “tăng count” hay chưa, tránh double-increment và giúp trừ đúng khi hủy sớm
    private bool _countedGeneral;
    private bool _countedDeath;

    // Giữ tham chiếu coroutine để có thể hủy khi cần (ví dụ chuyển clip)
    private Coroutine _generalResetCo;
    private Coroutine _deathResetCo;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.loop = false;
        _audio.playOnAwake = false;
        _isDead = false;
        _countedGeneral = false;
        _countedDeath = false;
    }

    void OnDisable()
    {
        // Nếu object bị tắt/hủy trước khi coroutine chạy xong, tự trừ về để không kẹt giới hạn
        CleanupCounters();
    }

    void OnDestroy()
    {
        CleanupCounters();
    }

    private void CleanupCounters()
    {
        if (_generalResetCo != null) { StopCoroutine(_generalResetCo); _generalResetCo = null; }
        if (_deathResetCo != null) { StopCoroutine(_deathResetCo); _deathResetCo = null; }

        if (_countedGeneral)
        {
            CurrentPlayingSounds = Mathf.Max(0, CurrentPlayingSounds - 1);
            _countedGeneral = false;
        }
        if (_countedDeath)
        {
            CurrentDeathSounds = Mathf.Max(0, CurrentDeathSounds - 1);
            _countedDeath = false;
        }
    }

    // ================== ATTACK 1 ==================
    public void PlayAttackSound()
    {
        if (_isDead || attackClip == null) return;

        // Chặn theo giới hạn toàn cục
        if (CurrentPlayingSounds >= MaxPlayingSounds) return;

        // Không phát đè chính source đang chạy
        if (_audio.isPlaying) return;

        // Phát và đếm
        _audio.volume = AudioManager.Instance.GetSFXVolume();
        _audio.clip = attackClip;
        _audio.Play();

        IncrementGeneralOnce();
        RestartGeneralReset(attackClip.length);
    }

    // ================== ATTACK 2 ==================
    public void PlayAttackSound2()
    {
        if (_isDead || attackClip2 == null) return;
        if (CurrentPlayingSounds >= MaxPlayingSounds) return;
        if (_audio.isPlaying) return;

        _audio.volume = AudioManager.Instance.GetSFXVolume();
        _audio.clip = attackClip2;
        _audio.Play();

        IncrementGeneralOnce();
        RestartGeneralReset(attackClip2.length);
    }

    // ================== DEATH ==================
    public void PlayDeathSound()
    {
        if (_isDead || deathClip == null) return;

        // Giới hạn death riêng
        if (CurrentDeathSounds >= MaxDeathSounds) return;

        _isDead = true;

        // Nếu đang phát attack và đã được tính general → trừ ngay để không bị +2
        if (_audio.isPlaying && _countedGeneral)
        {
            CurrentPlayingSounds = Mathf.Max(0, CurrentPlayingSounds - 1);
            _countedGeneral = false;

            if (_generalResetCo != null)
            {
                StopCoroutine(_generalResetCo);
                _generalResetCo = null;
            }
        }

        _audio.Stop();
        _audio.volume = AudioManager.Instance.GetSFXVolume();
        _audio.clip = deathClip;
        _audio.Play();

        // --- Lựa chọn chính sách đếm death ---
        // ĐỀ XUẤT (A): KHÔNG tính death vào general để tiếng chết không khóa các tiếng khác
        // => CHỈ đếm death riêng:
        IncrementDeathOnce();
        RestartDeathReset(deathClip.length);

        // Nếu bạn muốn death cũng tính vào general (chính sách B),
        // bỏ comment 3 dòng sau:
        // if (CurrentPlayingSounds < MaxPlayingSounds) {
        //     IncrementGeneralOnce();
        //     RestartGeneralReset(deathClip.length);
        // }
    }

    // ----------------- Helpers -----------------
    private void IncrementGeneralOnce()
    {
        if (_countedGeneral) return;
        CurrentPlayingSounds++;
        _countedGeneral = true;
    }

    private void IncrementDeathOnce()
    {
        if (_countedDeath) return;
        CurrentDeathSounds++;
        _countedDeath = true;
    }

    private void RestartGeneralReset(float seconds)
    {
        if (_generalResetCo != null) StopCoroutine(_generalResetCo);
        _generalResetCo = StartCoroutine(ResetGeneralAfter(seconds));
    }

    private void RestartDeathReset(float seconds)
    {
        if (_deathResetCo != null) StopCoroutine(_deathResetCo);
        _deathResetCo = StartCoroutine(ResetDeathAfter(seconds));
    }

    private IEnumerator ResetGeneralAfter(float seconds)
    {
        // Dùng realtime để không kẹt khi timeScale = 0
        yield return WaitForSecondsRealtimeSafe(seconds);
        if (_countedGeneral)
        {
            CurrentPlayingSounds = Mathf.Max(0, CurrentPlayingSounds - 1);
            _countedGeneral = false;
        }
        _generalResetCo = null;
    }

    private IEnumerator ResetDeathAfter(float seconds)
    {
        yield return WaitForSecondsRealtimeSafe(seconds);
        if (_countedDeath)
        {
            CurrentDeathSounds = Mathf.Max(0, CurrentDeathSounds - 1);
            _countedDeath = false;
        }
        _deathResetCo = null;
    }

    // Hỗ trợ cả khi seconds <= 0 (clip rỗng) để không treo counter
    private static IEnumerator WaitForSecondsRealtimeSafe(float seconds)
    {
        if (seconds <= 0f) yield break;
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end)
            yield return null;
    }

    // (Tuỳ chọn) Reset biến tĩnh khi load scene mới:
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    // private static void ResetStaticsOnLoad() {
    //     CurrentPlayingSounds = 0;
    //     CurrentDeathSounds = 0;
    // }
}
