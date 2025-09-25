using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class DeathUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider countdown;
    [SerializeField] private TextMeshProUGUI number;
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private float timer = 5f;

    private Animator animator;
    private AudioSource audioSource;

    void OnEnable()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // đồng bộ volume ngay khi bật UI
        ApplyVolume();

        countdown.maxValue = timer;
        countdown.value = timer;
        number.text = timer.ToString();

        // bắt đầu đếm ngược
        StartCoroutine(Countdown());
    }

    void Update()
    {
        // cập nhật volume liên tục theo AudioManager
        ApplyVolume();

        if (Input.anyKeyDown && canvasGroup.interactable)
        {
            StartCoroutine(PlayOutAnimation());
        }
    }

    private void ApplyVolume()
    {
        if (AudioManager.Instance != null && audioSource != null)
        {
            audioSource.volume = AudioManager.Instance.GetSFXVolume();
        }
    }

    public void Show()
    {
        pauseCanvas.SetActive(false);
        gameObject.SetActive(true);
    }

    private IEnumerator Countdown()
    {
        float t = timer;

        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;

            countdown.value = t;
            number.text = Mathf.CeilToInt(t).ToString();

            yield return null;
        }

        countdown.value = 0;
        number.text = "0";

        StartCoroutine(PlayOutAnimation());
    }

    private IEnumerator PlayOutAnimation()
    {
        animator.Play("Out");
        yield return null;

        var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            float duration = clipInfo[0].clip.length;
            yield return new WaitForSecondsRealtime(duration);
        }

        SelectorSpawner.Instance.Player.GetComponent<PlayerHealthSystem>().Respawn();

        pauseCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
