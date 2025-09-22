using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroSequenceController : MonoBehaviour
{
    // Cờ "đã phát intro trong phiên chạy hiện tại"
    private static bool s_IntroPlayedThisSession = false;

    [Header("1) Splash")]
    [SerializeField] private GameObject splashRoot;
    [SerializeField] private Animator splashAnimator;
    [SerializeField] private float splashDurationOverride = 0f;

    [Header("2) Video")]
    [SerializeField] private GameObject videoRoot;     // RawImage + VideoPlayer
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private bool muteVideoAudio = false;
    [SerializeField] private AudioSource videoAudio;   // dùng khi AudioOutputMode = AudioSource

    [Header("2.1) Skip Button")]
    [SerializeField] private GameObject skipButtonRoot;
    [SerializeField] private Button skipButton;
    private bool _skipRequested;

    [Header("3) Dissolve (UI)")]
    [SerializeField] private DissolveEffectUI dissolveEffect;
    [SerializeField] private float dissolveDuration = 1.5f;
    [SerializeField] private float dissolveStart = 0f;
    [SerializeField] private float dissolveEnd = 1f;

    [Header("Debug / Editor")]
    [Tooltip("Chỉ để test trong Editor: ép phát lại intro dù đã phát trong phiên.")]
    [SerializeField] private bool forcePlayInEditor = false;

    private void Awake()
    {
        // Trạng thái UI mặc định
        if (skipButtonRoot) skipButtonRoot.SetActive(false);
        if (videoRoot) videoRoot.SetActive(false);
        if (splashRoot) splashRoot.SetActive(false);
        _skipRequested = false;

#if UNITY_EDITOR
        bool shouldPlayIntro = !s_IntroPlayedThisSession || forcePlayInEditor;
#else
        bool shouldPlayIntro = !s_IntroPlayedThisSession;
#endif

        if (shouldPlayIntro)
        {
            // LẦN ĐẦU trong phiên: phát Splash → Video → Dissolve
            s_IntroPlayedThisSession = true; // đặt sớm để các lần sau skip intro
            if (splashRoot) splashRoot.SetActive(true);
            StartCoroutine(RunSequence());
        }
        else
        {
            // ĐÃ xem intro trong phiên: chỉ chạy Dissolve
            StartCoroutine(RunDissolve());
        }
    }

    private IEnumerator RunSequence()
    {
        yield return ShowSplash();
        yield return PlayVideo();   // có thể bị skip
        yield return RunDissolve();
    }

    // ----------------- SPLASH -----------------
    private IEnumerator ShowSplash()
    {
        yield return null; // chờ Animator vào state

        float length = splashDurationOverride;
        if (length <= 0f && splashAnimator)
        {
            var infos = splashAnimator.GetCurrentAnimatorClipInfo(0);
            if (infos != null && infos.Length > 0 && infos[0].clip) length = infos[0].clip.length;
            else length = splashAnimator.GetCurrentAnimatorStateInfo(0).length;
        }
        if (length <= 0f) length = 2f;

        yield return new WaitForSeconds(length);
        if (splashRoot) splashRoot.SetActive(false);
    }

    // ----------------- VIDEO (+ SKIP) -----------------
    private IEnumerator PlayVideo()
    {
        if (!videoRoot || !videoPlayer) yield break;

        videoRoot.SetActive(true);

        // Audio setup
        if (muteVideoAudio) videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        else if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
            if (!videoAudio)
            {
                videoAudio = videoRoot.GetComponent<AudioSource>();
                if (!videoAudio) videoAudio = videoRoot.AddComponent<AudioSource>();
                videoAudio.playOnAwake = false;
            }
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, videoAudio);
        }

        // Bật nút Skip
        _skipRequested = false;
        if (skipButtonRoot) skipButtonRoot.SetActive(true);
        if (skipButton)
        {
            skipButton.onClick.RemoveListener(OnSkipClicked);
            skipButton.onClick.AddListener(OnSkipClicked);
            skipButton.interactable = true;
        }

        // Chuẩn bị và phát
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared && !_skipRequested) yield return null;

        if (_skipRequested) { CleanupVideoUI(); yield break; }

        videoPlayer.frame = 0;
        bool ended = false;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += _ => ended = true;

        videoPlayer.Play();
        if (!muteVideoAudio && videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource && videoAudio)
            videoAudio.Play();

        while (!ended && !_skipRequested) yield return null;

        if (_skipRequested)
        {
            if (videoPlayer.isPlaying) videoPlayer.Stop();
            if (videoAudio && videoAudio.isPlaying) videoAudio.Stop();
        }

        CleanupVideoUI();
    }

    private void OnSkipClicked()
    {
        _skipRequested = true;
        if (skipButton) skipButton.interactable = false;
    }

    private void CleanupVideoUI()
    {
        if (skipButton) skipButton.onClick.RemoveListener(OnSkipClicked);
        if (skipButtonRoot) skipButtonRoot.SetActive(false);
        if (videoRoot) videoRoot.SetActive(false);
    }

    // ----------------- DISSOLVE -----------------
    private IEnumerator RunDissolve()
    {
        if (!dissolveEffect) yield break;

        dissolveEffect.location = dissolveStart;

        float t = 0f;
        while (t < dissolveDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dissolveDuration);
            dissolveEffect.location = Mathf.Lerp(dissolveStart, dissolveEnd, k);
            yield return null;
        }
        dissolveEffect.location = dissolveEnd;
    }
}
