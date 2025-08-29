using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI instance;

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI hintsText;
    public Slider progressBar;
    public Transform spinnerParent;
    public Image imageObject;
    public Animator animator;
    public AudioSource audioSource;

    [Range(0.25f, 10)] public float fadeSpeed = 4, backgroundFadeSpeed = 2, contentFadeSpeed = 2;

    [Header("HINTS SETTINGS")]
    [SerializeField] private bool enableRandomHints = true;
    [Range(1, 5)] public float hintTimer = 5;
    [Range(0.1f, 10)] public float hintFadeDuration = 1.5f;
    [TextArea] public List<string> hintList = new();
    private int currentHintIndex;

    [Header("BACKGROUND IMAGES SETTINGS")]
    [SerializeField] private bool enableRandomImages = true;
    [Range(1, 5)] public float imageTimer = 5;
    [Range(0.1f, 10)] public float imageFadingSpeed = 4;
    public List<Sprite> imageList = new();
    private int currentImageIndex;

    [Header("VIRTUAL SETTINGS")]
    public bool enableVirtualLoading = false;
    public float virtualLoadingTimer = 5;
    private float currentVirtualTime;

    private bool processLoading;
    private AsyncOperation loadingProcess = new();

    void OnEnable()
    {
        Time.timeScale = 0f;

        StopAllCoroutines();

        if (enableRandomHints && hintList.Count > 0) StartCoroutine(RandomHint());
        if (enableRandomImages && imageList.Count > 0) StartCoroutine(RandomImage());

        imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);
        statusText.text = "0%";
        progressBar.value = 0;
    }

    public static void LoadScene(string targetScene)
    {
        if (instance != null) Destroy(instance);
        instance = Instantiate(Resources.Load<GameObject>("Loading").GetComponent<LoadingScreenUI>());
        // instance = Instantiate(loadingScreenPrefab);
        instance.gameObject.SetActive(true);
        DontDestroyOnLoad(instance.gameObject);
        instance.StartCoroutine(instance.LoadSceneRoutine(targetScene));
    }

    public IEnumerator LoadSceneRoutine(string targetScene)
    {
        // Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(0.1f);

        processLoading = true;
        loadingProcess = SceneManager.LoadSceneAsync(targetScene);
        loadingProcess.allowSceneActivation = false;
    }

    void Update()
    {
        if (!processLoading) return;

        if (enableVirtualLoading)
            ProcessVirtualLoading();
        else
            ProcessLoading();
    }

    void ProcessLoading()
    {
        if (!loadingProcess.allowSceneActivation)
            loadingProcess.allowSceneActivation = true;

        progressBar.value = Mathf.Lerp(progressBar.value, loadingProcess.progress, 0.1f * Time.unscaledDeltaTime * 60);
        statusText.text = Mathf.Round(progressBar.value * 100) + "%";

        if (canvasGroup.alpha == 0) animator.Play("In");

        if (loadingProcess.progress >= 0.9f)
        {
            Time.timeScale = 1f;

            animator.Play("Out");
            var length = animator.GetCurrentAnimatorStateInfo(0).length;
            Destroy(gameObject, length);
        }
    }

    void ProcessVirtualLoading()
    {
        progressBar.value += 1 / virtualLoadingTimer * Time.unscaledDeltaTime;
        statusText.text = Mathf.Round(progressBar.value * 100) + "%";
        currentVirtualTime += Time.unscaledDeltaTime;

        if (canvasGroup.alpha == 0) animator.Play("In");

        if (currentVirtualTime >= virtualLoadingTimer)
        {
            if (!loadingProcess.allowSceneActivation) loadingProcess.allowSceneActivation = true;

            if (loadingProcess.progress >= 0.9f)
            {
                Time.timeScale = 1f;

                animator.Play("Out");
                var length = animator.GetCurrentAnimatorStateInfo(0).length;
                Destroy(gameObject, length);
            }
        }
    }

    private IEnumerator RandomHint()
    {
        while (true)
        {
            string hint = GetRandomItem(hintList, ref currentHintIndex);
            hintsText.text = hint;

            yield return FadeTextAlpha(0, 1);
            yield return new WaitForSecondsRealtime(hintTimer);
            yield return FadeTextAlpha(1, 0);
        }
    }

    private IEnumerator RandomImage()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(imageTimer);
            yield return FadeImageAlpha(1, 0);
            imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);
            yield return FadeImageAlpha(0, 1);
        }
    }

    private IEnumerator FadeTextAlpha(float from, float to)
    {
        float t = 0f;
        Color color = hintsText.color;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / hintFadeDuration;
            color.a = Mathf.Lerp(from, to, t);
            hintsText.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeImageAlpha(float from, float to)
    {
        float t = 0f;
        Color color = imageObject.color;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * imageFadingSpeed;
            color.a = Mathf.Lerp(from, to, t);
            imageObject.color = color;
            yield return null;
        }
    }

    private T GetRandomItem<T>(List<T> list, ref int lastIndex)
    {
        if (list.Count <= 1) return list[0];
        int newIndex;
        do
        {
            newIndex = Random.Range(0, list.Count);
        }
        while (newIndex == lastIndex);
        lastIndex = newIndex;

        return list[newIndex];
    }
}
