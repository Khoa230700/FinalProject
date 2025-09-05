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

    private bool isProcessingLoad;
    private AsyncOperation loadingProcess;
    private float currentVirtualTime;
    private int currentHintIndex = -1, currentImageIndex = -1;

    // --- TOI UU ---
    private int lastDisplayedPercent = -1;
    private Coroutine hintCoroutine, imageCoroutine;
    private Dictionary<float, WaitForSecondsRealtime> waitCache = new();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 9999;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        gameObject.SetActive(false);
    }

    #region Preload & LoadScene
    public static void Preload()
    {
        if (instance != null) return;

        var prefab = Resources.Load<GameObject>("Loading");
        if (prefab == null)
        {
            Debug.LogError("Loading prefab not found in Resources!");
            return;
        }

        var go = Instantiate(prefab);
        instance = go.GetComponent<LoadingScreenUI>();
    }

    public static void LoadScene(string targetScene)
    {
        if (instance == null) Preload();
        if (instance.isProcessingLoad) return; // Ngăn trùng lặp

        instance.gameObject.SetActive(true);
        instance.StartCoroutine(instance.LoadSceneRoutine(targetScene));
    }
    #endregion

    IEnumerator LoadSceneRoutine(string targetScene)
    {
        isProcessingLoad = true;

        // Reset trạng thái
        progressBar.value = 0f;
        statusText.text = "0%";
        lastDisplayedPercent = 0;
        currentVirtualTime = 0f;

        // --- BƯỚC QUAN TRỌNG: Đảm bảo UI render trước ---
        yield return null; // Cho Unity render UI lần đầu

        // Tắt raycast tạm thời
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;

        // --- 1. FADE IN HOÀN TOÀN TRƯỚC KHI LOAD ---
        StartCoroutine(FadeCanvasGroup(0f, 1f, 1f / fadeSpeed, () =>
        {
            // Chỉ bắt đầu load khi fade-in xong
            StartLoadingProcess(targetScene);
        }));
    }

    // --- Hàm gọi chỉ khi fade-in xong ---
    void StartLoadingProcess(string targetScene)
    {
        if (!isProcessingLoad) return;

        // --- BẮT ĐẦU QUÁ TRÌNH LOAD ---
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        loadingProcess = SceneManager.LoadSceneAsync(targetScene);
        loadingProcess.allowSceneActivation = false;

        // --- Cài đặt âm thanh ---
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
            audioSource.volume = 0.5f;
        }

        // --- Cập nhật UI ---
        if (imageList.Count > 0)
            imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);

        // --- Bắt đầu hint/image nếu cần ---
        if (enableRandomHints && hintList.Count > 0)
            hintCoroutine = StartCoroutine(CycleHints());

        if (enableRandomImages && imageList.Count > 1)
            imageCoroutine = StartCoroutine(CycleImages());

        // --- Bắt đầu vòng lặp kiểm tra tiến độ ---
        StartCoroutine(UpdateLoadingLoop());
    }

    // --- Vòng lặp chính kiểm tra progress mỗi frame ---
    IEnumerator UpdateLoadingLoop()
    {
        while (isProcessingLoad)
        {
            UpdateLoadingProgress();
            yield return null;
        }
    }

    void UpdateLoadingProgress()
    {
        float progress = 0f;

        if (enableVirtualLoading)
        {
            currentVirtualTime += Time.unscaledDeltaTime;
            progress = Mathf.Clamp01(currentVirtualTime / virtualLoadingTimer);
        }
        else
        {
            progress = loadingProcess != null ? Mathf.Clamp01(loadingProcess.progress / 0.9f) : 0f;
        }

        // Smooth lerp progress bar
        progressBar.value = Mathf.Lerp(progressBar.value, progress, Time.unscaledDeltaTime * fadeSpeed);

        // Cập nhật text chỉ khi % thay đổi
        int currentPercent = Mathf.RoundToInt(progressBar.value * 100f);
        if (currentPercent != lastDisplayedPercent)
        {
            statusText.text = currentPercent + "%";
            lastDisplayedPercent = currentPercent;
        }

        // Fade âm thanh
        if (audioSource != null)
            audioSource.volume = Mathf.Lerp(0.5f, 0f, progressBar.value);

        // Kiểm tra hoàn thành
        bool isFinished = 
            (!enableVirtualLoading && loadingProcess.progress >= 0.9f && progressBar.value >= 0.99f) ||
            (enableVirtualLoading && currentVirtualTime >= virtualLoadingTimer);

        if (isFinished)
        {
            FinishLoading();
        }
    }

    void FinishLoading()
    {
        if (!isProcessingLoad) return;
        isProcessingLoad = false;

        // Đảm bảo 100%
        progressBar.value = 1f;
        if (lastDisplayedPercent != 100) statusText.text = "100%";

        if (loadingProcess != null)
            loadingProcess.allowSceneActivation = true;

        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        // Fade out và hủy
        StartCoroutine(FadeCanvasGroup(1f, 0f, 1f / fadeSpeed, () =>
        {
            gameObject.SetActive(false);
            if (hintCoroutine != null) StopCoroutine(hintCoroutine);
            if (imageCoroutine != null) StopCoroutine(imageCoroutine);
        }));
    }

    #region Hint & Image Cycling
    IEnumerator CycleHints()
    {
        hintsText.gameObject.SetActive(hintList.Count > 0);
        if (hintList.Count == 0) yield break;

        hintsText.text = GetRandomItem(hintList, ref currentHintIndex);
        yield return FadeText(hintsText, 0f, 1f, hintFadeDuration);

        while (true)
        {
            yield return GetWaiter(hintTimer);
            yield return FadeText(hintsText, 1f, 0f, hintFadeDuration);
            hintsText.text = GetRandomItem(hintList, ref currentHintIndex);
            yield return FadeText(hintsText, 0f, 1f, hintFadeDuration);
        }
    }

    IEnumerator CycleImages()
    {
        if (imageList.Count <= 1) yield break;

        while (true)
        {
            yield return GetWaiter(imageTimer);
            yield return FadeImage(imageObject, 1f, 0f, 1f / imageFadingSpeed);
            imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);
            yield return FadeImage(imageObject, 0f, 1f, 1f / imageFadingSpeed);
        }
    }
    #endregion

    #region Fade Coroutines & Helpers
    private WaitForSecondsRealtime GetWaiter(float seconds)
    {
        if (!waitCache.ContainsKey(seconds))
        {
            waitCache[seconds] = new WaitForSecondsRealtime(seconds);
        }
        return waitCache[seconds];
    }

    IEnumerator FadeCanvasGroup(float from, float to, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
        canvasGroup.blocksRaycasts = (to > 0.1f);
        onComplete?.Invoke();
    }

    IEnumerator FadeText(TextMeshProUGUI text, float from, float to, float duration)
    {
        if (text == null) yield break;
        float elapsed = 0f;
        Color color = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, elapsed / duration);
            text.color = color;
            yield return null;
        }
        color.a = to;
        text.color = color;
    }

    IEnumerator FadeImage(Image img, float from, float to, float duration)
    {
        if (img == null) yield break;
        float elapsed = 0f;
        Color color = img.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, elapsed / duration);
            img.color = color;
            yield return null;
        }
        color.a = to;
        img.color = color;
    }
    #endregion

    T GetRandomItem<T>(List<T> list, ref int lastIndex)
    {
        if (list?.Count == 0) return default;
        if (list.Count == 1) return list[0];

        int newIndex;
        do {
            newIndex = Random.Range(0, list.Count);
        } while (newIndex == lastIndex);

        lastIndex = newIndex;
        return list[newIndex];
    }
}