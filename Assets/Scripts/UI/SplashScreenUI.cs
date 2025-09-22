using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class IntroSequenceController : MonoBehaviour
{
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
    [SerializeField] private GameObject skipButtonRoot;   // UI container của nút skip (ẩn lúc đầu)
    [SerializeField] private Button skipButton;           // Component Button
    private bool _skipRequested;

    [Header("3) Dissolve (UI)")]
    [SerializeField] private DissolveEffectUI dissolveEffect;
    [SerializeField] private float dissolveDuration = 1.5f;
    [SerializeField] private float dissolveStart = 0f;
    [SerializeField] private float dissolveEnd = 1f;

    private void Awake()
    {
        if (splashRoot) splashRoot.SetActive(true);
        if (videoRoot) videoRoot.SetActive(false);
        if (skipButtonRoot) skipButtonRoot.SetActive(false);
        _skipRequested = false;

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        yield return ShowSplash();
        yield return PlayVideo();        // có thể bị bỏ qua nếu bấm Skip
        yield return RunDissolve();
    }

    // ----------------- SPLASH -----------------
    private IEnumerator ShowSplash()
    {
        yield return null; // cho Animator vào state

        float length = splashDurationOverride;
        if (length <= 0f && splashAnimator)
        {
            var infos = splashAnimator.GetCurrentAnimatorClipInfo(0);
            if (infos != null && infos.Length > 0 && infos[0].clip != null)
                length = infos[0].clip.length;
            else
                length = splashAnimator.GetCurrentAnimatorStateInfo(0).length;
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

        // setup audio
        if (muteVideoAudio)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }
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

        // bật nút skip
        _skipRequested = false;
        if (skipButtonRoot) skipButtonRoot.SetActive(true);
        if (skipButton)
        {
            skipButton.onClick.RemoveListener(OnSkipClicked);
            skipButton.onClick.AddListener(OnSkipClicked);
            skipButton.interactable = true;
        }

        // chuẩn bị video
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared && !_skipRequested)
            yield return null;

        if (_skipRequested)
        {
            // user bấm skip khi đang prepare
            CleanupVideoUI();
            yield break;
        }

        videoPlayer.frame = 0;
        bool ended = false;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += _ => ended = true;

        videoPlayer.Play();
        if (!muteVideoAudio && videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource && videoAudio)
            videoAudio.Play();

        // chờ kết thúc hoặc skip
        while (!ended && !_skipRequested)
            yield return null;

        // nếu skip khi đang phát
        if (_skipRequested)
        {
            // dừng phát ngay
            if (videoPlayer.isPlaying) videoPlayer.Stop();
            if (videoAudio && videoAudio.isPlaying) videoAudio.Stop();
        }

        CleanupVideoUI();
    }

    private void OnSkipClicked()
    {
        _skipRequested = true;
        if (skipButton) skipButton.interactable = false; // tránh double click
    }

    private void CleanupVideoUI()
    {
        if (skipButton)
            skipButton.onClick.RemoveListener(OnSkipClicked);

        if (skipButtonRoot)
            skipButtonRoot.SetActive(false);

        if (videoRoot)
            videoRoot.SetActive(false);
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
