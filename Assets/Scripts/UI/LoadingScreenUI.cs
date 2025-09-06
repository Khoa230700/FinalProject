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

    // === PRELOAD HANDLER ===
    private static LoadingScreenUI preloadInstance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Đảm bảo Canvas luôn ở trên cùng
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 9999;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Stop();
            audioSource.time = 0;
            audioSource.Play();
            audioSource.volume = 0.5f;
        }
    }

    public static void Preload()
    {
        if (preloadInstance != null) return;

        var prefab = Resources.Load<GameObject>("Loading");
        if (prefab == null)
        {
            Debug.LogError("Loading prefab not found in Resources!");
            return;
        }

        var go = Instantiate(prefab);
        preloadInstance = go.GetComponent<LoadingScreenUI>();
        preloadInstance.gameObject.SetActive(false);
        DontDestroyOnLoad(preloadInstance.gameObject);
    }

    public static void LoadScene(string targetScene)
    {
        if (preloadInstance == null)
        {
            Preload();
        }

        // Force activate UI immediately
        preloadInstance.gameObject.SetActive(true);
        preloadInstance.StartCoroutine(preloadInstance.LoadSceneRoutine(targetScene));
    }

    IEnumerator LoadSceneRoutine(string targetScene)
    {
        Debug.Log("Loading screen started");
        
        // CRITICAL: Ensure UI is visible IMMEDIATELY
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // Force canvas to update
        Canvas.ForceUpdateCanvases();
        
        isProcessingLoad = true;
        isDestroying = false;

        // Set initial values
        statusText.text = "Loading... 0%";
        progressBar.value = 0f;
        currentVirtualTime = 0f;

        if (imageList.Count > 0)
            imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);

        // Start hint and image cycling
        if (enableRandomHints && hintList.Count > 0)
            StartCoroutine(CycleHints());

        if (enableRandomImages && imageList.Count > 1)
            StartCoroutine(CycleImages());

        // CRITICAL: Wait multiple frames to ensure UI is rendered
        yield return null; // Wait 1 frame
        yield return null; // Wait another frame
        yield return new WaitForSecondsRealtime(0.1f); // Extra safety delay

        Debug.Log("Starting scene load...");

        // Start loading the scene
        loadingProcess = SceneManager.LoadSceneAsync(targetScene);
        loadingProcess.allowSceneActivation = false;

        // Play entrance animation if available
        if (animator != null)
            animator.Play("In");

        Debug.Log("Scene loading in progress...");
    }

    void Update()
    {
        if (!isProcessingLoad || isDestroying) return;

        // Ensure UI remains visible
        if (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha = 1f;
        }

        if (enableVirtualLoading) ProcessVirtualLoading();
        else ProcessRealLoading();
    }

    void ProcessRealLoading()
    {
        if (loadingProcess == null) return;

        float targetProgress = Mathf.Clamp01(loadingProcess.progress / 0.9f);
        progressBar.value = Mathf.Lerp(progressBar.value, targetProgress, Time.unscaledDeltaTime * fadeSpeed);
        statusText.text = "Loading... " + Mathf.RoundToInt(progressBar.value * 100f) + "%";

        if (loadingProcess.progress >= 0.9f && progressBar.value >= 0.95f)
        {
            progressBar.value = 1f;
            statusText.text = "Loading... 100%";
            loadingProcess.allowSceneActivation = true;
            FinishLoading();
        }
    }

    void ProcessVirtualLoading()
    {
        currentVirtualTime += Time.unscaledDeltaTime;
        float progress = Mathf.Clamp01(currentVirtualTime / virtualLoadingTimer);

        progressBar.value = progress;
        statusText.text = "Loading... " + Mathf.RoundToInt(progress * 100f) + "%";

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
        
        Debug.Log("Loading finished");
        isProcessingLoad = false;
        
        // Reset loading priority back to normal
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        if (audioSource != null && audioSource.volume > 0f)
            StartCoroutine(FadeAudio(audioSource.volume, 0f, 1.0f));

        if (animator != null)
        {
            animator.Play("Out");
            StartCoroutine(DestroyAfterDelay());
        }
        else
        {
            CleanupAndDestroy();
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        
        // Get animation length safely
        float length = 1f; // Default fallback
        try
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Out"))
                length = stateInfo.length;
        }
        catch
        {
            Debug.LogWarning("Could not get animator state info, using default delay");
        }
        
        yield return new WaitForSecondsRealtime(length);
        CleanupAndDestroy();
    }

    void CleanupAndDestroy()
    {
        if (isDestroying) return;
        isDestroying = true;

        Debug.Log("Loading screen cleanup");
        gameObject.SetActive(false);
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