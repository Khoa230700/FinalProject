using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI instance;

    [Header("UI COMPONENTS")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI statusText, titleText, descriptionText, hintsText;
    public Slider progressBar;
    public Transform spinnerParent;
    public Image imageObject;
    public Animator animator;
    public AudioSource audioSource;

    [Header("SETTINGS")]
    [Range(0.25f, 10)] public float fadeSpeed = 4f;
    [SerializeField] private bool enableRandomHints = true, enableRandomImages = true;
    [Range(1f, 10f)] public float hintTimer = 5f, imageTimer = 5f;
    [Range(0.1f, 10f)] public float hintFadeDuration = 1.5f, imageFadingSpeed = 4f;
    [TextArea] public List<string> hintList = new();
    public List<Sprite> imageList = new();

    [Header("VIRTUAL LOADING")]
    public bool enableVirtualLoading = false;
    [Range(1f, 20f)] public float virtualLoadingTimer = 5f;

    private bool isProcessingLoad, isDestroying;
    private AsyncOperation loadingProcess;
    private float currentVirtualTime;
    private int currentHintIndex = -1, currentImageIndex = -1;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        StopAllCoroutines();
        statusText.text = "0%";
        progressBar.value = 0f;
        currentVirtualTime = 0f;

        if (imageList.Count > 0)
            imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);

        if (enableRandomHints && hintList.Count > 0)
            StartCoroutine(CycleHints());

        if (enableRandomImages && imageList.Count > 1)
            StartCoroutine(CycleImages());
    }

    void OnDestroy()
    {
        isDestroying = true;
        StopAllCoroutines();
        if (instance == this) instance = null;
    }

    public static void LoadScene(string targetScene)
    {
        if (instance != null) instance.CleanupAndDestroy();

        var prefab = Resources.Load<GameObject>("Loading");
        if (prefab == null)
        {
            Debug.LogError("Loading screen prefab not found in Resources!");
            return;
        }

        var loadingUI = Instantiate(prefab).GetComponent<LoadingScreenUI>();
        loadingUI?.StartCoroutine(loadingUI.LoadSceneRoutine(targetScene));
    }

    IEnumerator LoadSceneRoutine(string targetScene)
    {
        gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.1f);

        isProcessingLoad = true;
        loadingProcess = SceneManager.LoadSceneAsync(targetScene);
        loadingProcess.allowSceneActivation = false;

        if (animator != null && canvasGroup.alpha < 1f)
            animator.Play("In");
    }

    void Update()
    {
        if (!isProcessingLoad || isDestroying) return;

        if (enableVirtualLoading) ProcessVirtualLoading();
        else ProcessRealLoading();
    }

    void ProcessRealLoading()
    {
        if (loadingProcess == null) return;

        progressBar.value = Mathf.Lerp(progressBar.value, loadingProcess.progress, Time.unscaledDeltaTime * fadeSpeed);
        statusText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";

        if (loadingProcess.progress >= 0.9f)
        {
            progressBar.value = 1f;
            statusText.text = "100%";
            loadingProcess.allowSceneActivation = true;
            FinishLoading();
        }
    }

    void ProcessVirtualLoading()
    {
        currentVirtualTime += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(currentVirtualTime / virtualLoadingTimer);

        progressBar.value = progress;
        statusText.text = Mathf.RoundToInt(progress * 100f) + "%";

        if (currentVirtualTime >= virtualLoadingTimer)
        {
            if (loadingProcess != null && !loadingProcess.allowSceneActivation)
                loadingProcess.allowSceneActivation = true;

            if (loadingProcess == null || loadingProcess.progress >= 0.9f)
                FinishLoading();
        }
    }

    void FinishLoading()
{
        if (isDestroying) return;
        isProcessingLoad = false;

        // 👇 fade âm thanh khi gần xong
        if (audioSource != null && audioSource.volume > 0f)
            StartCoroutine(FadeAudio(audioSource.volume, 0f, 1.0f)); // 1.0f = thời gian fade (giây)

        if (animator != null)
        {
            animator.Play("Out");
            StartCoroutine(DestroyAfterDelay());
        }
        else
        {
            Destroy(gameObject, 0.5f);
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSecondsRealtime(length);
        CleanupAndDestroy();
    }

    void CleanupAndDestroy()
    {
        if (isDestroying) return;
        isDestroying = true;
        Time.timeScale = 1f;
        Destroy(gameObject);
    }

    IEnumerator CycleHints()
    {
        if (hintList.Count > 0)
        {
            hintsText.text = GetRandomItem(hintList, ref currentHintIndex);
            yield return FadeText(hintsText, 0f, 1f, hintFadeDuration);
        }

        while (!isDestroying)
        {
            if (hintList.Count == 0) yield break;

            yield return new WaitForSecondsRealtime(hintTimer);
            if (isDestroying) yield break;

            yield return FadeText(hintsText, 1f, 0f, hintFadeDuration);
            if (isDestroying) yield break;

            hintsText.text = GetRandomItem(hintList, ref currentHintIndex);
            yield return FadeText(hintsText, 0f, 1f, hintFadeDuration);
        }
    }

    IEnumerator CycleImages()
    {
        while (!isDestroying)
        {
            yield return new WaitForSecondsRealtime(imageTimer);
            if (imageList.Count <= 1 || isDestroying) yield break;

            yield return FadeImage(imageObject, 1f, 0f, 1f / imageFadingSpeed);
            if (isDestroying) yield break;

            imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);
            yield return FadeImage(imageObject, 0f, 1f, 1f / imageFadingSpeed);
        }
    }

    IEnumerator FadeText(TextMeshProUGUI text, float from, float to, float duration)
    {
        if (text == null) yield break;

        float elapsed = 0f;
        Color color = text.color;

        while (elapsed < duration && !isDestroying)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, elapsed / duration);
            text.color = color;
            yield return null;
        }

        if (!isDestroying)
        {
            color.a = to;
            text.color = color;
        }
    }

    IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        Color color = img.color;

        while (elapsed < duration && !isDestroying)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, elapsed / duration);
            img.color = color;
            yield return null;
        }

        if (!isDestroying)
        {
            color.a = to;
            img.color = color;
        }
    }

    IEnumerator FadeAudio(float from, float to, float duration)
    {
        if (audioSource == null) yield break;

        float elapsed = 0f;
        audioSource.volume = from;

        while (elapsed < duration && !isDestroying)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        audioSource.volume = to;
    }


    T GetRandomItem<T>(List<T> list, ref int lastIndex)
    {
        if (list?.Count == 0) return default;
        if (list.Count == 1) return list[0];

        int newIndex;
        int attempts = 0;
        do
        {
            newIndex = Random.Range(0, list.Count);
            attempts++;
        }
        while (newIndex == lastIndex && attempts < 10);

        lastIndex = newIndex;
        return list[newIndex];
    }
}
