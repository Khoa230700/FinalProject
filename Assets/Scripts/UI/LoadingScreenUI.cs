using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI instance;

    [Header("RESOURCES")]
    public CanvasGroup canvasGroup;
    public CanvasGroup backgroundCanvasGroup;
    public CanvasGroup contentCanvasGroup;
    public CanvasGroup pakCanvasGroup;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Slider progressBar;
    public Transform spinnerParent;
    public TextMeshProUGUI hintsText;
    public Image imageObject;
    public TextMeshProUGUI pakTextObj;
    public TextMeshProUGUI pakCountdownLabel;
    public Slider pakCountdownSlider;

    [Space(10)]
    [Header("HINTS SETTINGS")]
    [SerializeField] private bool enableRandomHints = true;
    [Range(1, 5)] public float hintTimerValue = 5;
    [Range(0.1f, 10)] public float hintRevealDuration = 1.5f;
    [TextArea] public List<string> hintList = new List<string>();
    int currentHintIndex = 0;

    [Space(10)]
    [Header("BACKGROUND IMAGES SETTINGS")]
    [SerializeField] private bool enableRandomImages = true;
    [Range(1, 5)] public float imageTimerValue = 5;
    [Range(0.1f, 10)] public float imageFadingSpeed = 4;
    public List<Sprite> imageList = new List<Sprite>();
    int currentImageIndex = 0;

    [Space(10)]
    [Header("PAK SETTINGS")]
    public bool useCountdown = true;
    public bool waitForPlayerInput = false;
    [Range(1, 30)] public int pakCountdownTimer = 5;

    [Space(10)]
    [Header("VIRTUAL SETTINGS")]
    public bool enableVirtualLoading = false;
    public float virtualLoadingTimer = 5;
    public float currentVirtualTime;
    [Range(0.25f, 10)] public float fadeSpeed = 4;
    [Range(0.25f, 10)] public float backgroundFadeSpeed = 2;
    [Range(0.25f, 10)] public float contentFadeSpeed = 2;

    bool processLoading = false;
    public AsyncOperation loadingProcess = new AsyncOperation();

    private void Start()
    {
        if (enableRandomHints && hintList.Count > 0)
        {
            StartCoroutine(RandomHint());
        }

        if (enableRandomImages && imageList.Count > 0)
        {
            StartCoroutine(RandomImage());
        }

        if (useCountdown)
        {
            pakCountdownSlider.maxValue = pakCountdownTimer;
            pakCountdownSlider.value = pakCountdownTimer;
            pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();
        }
        else
        {
            pakCountdownSlider.gameObject.SetActive(false);
        }

        statusText.text = "0%";
        progressBar.value = 0;
    }

    public static void LoadScene(string targetScene)
    {
        try
        {
            instance = Instantiate(Resources.Load<GameObject>("Standard").GetComponent<LoadingScreenUI>());
            instance.gameObject.SetActive(true);
            DontDestroyOnLoad(instance.gameObject);

            Time.timeScale = 1; 

            instance.processLoading = true;
            instance.loadingProcess = SceneManager.LoadSceneAsync(targetScene);
            instance.loadingProcess.allowSceneActivation = false;
        }

        catch
        {
            Debug.Log("Load");
            instance.processLoading = false;

            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }
    }

    public static void LoadSceneAdditive(string targetScene)
    {
        try
        {
            instance = Instantiate(Resources.Load<GameObject>("Standard").GetComponent<LoadingScreenUI>());
            instance.gameObject.SetActive(true);
            DontDestroyOnLoad(instance.gameObject);

            Time.timeScale = 1;

            instance.canvasGroup.alpha = 0f;
            instance.backgroundCanvasGroup.alpha = 0;
            instance.contentCanvasGroup.alpha = 0;
            instance.pakCanvasGroup.alpha = 0;

            instance.processLoading = true;
            instance.loadingProcess = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            instance.loadingProcess.allowSceneActivation = false;
        }

        catch
        {
            Debug.Log("Load Additive");
            instance.processLoading = false;

            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }
    }

    public static void PerformVirtualTransition(float transitionDuration)
    {
        instance = Instantiate(Resources.Load<GameObject>("Standard").GetComponent<LoadingScreenUI>());
        instance.gameObject.SetActive(true);
        DontDestroyOnLoad(instance.gameObject);

        Time.timeScale = 1;

        instance.StopCoroutine("FadeInLoadingScreen");
        instance.StartCoroutine("FadeInLoadingScreen");
        instance.StartCoroutine("HandleVirtualTransition", transitionDuration);
    }

    public static void EndVirtualTransition()
    {
        instance.StopCoroutine("HandleVirtualTransition");
        instance.StartCoroutine("FadeOutContentScreen", true);
    }

    void Update()
    {
        if (!enableVirtualLoading) { ProcessLoading(); }
        else { ProcessVirtualLoading(); }
    }

    void ProcessLoading()
    {
        if (!processLoading)
            return;

        if (!loadingProcess.allowSceneActivation) { loadingProcess.allowSceneActivation = true; }
        else if (!loadingProcess.allowSceneActivation && !waitForPlayerInput) { loadingProcess.allowSceneActivation = true; }

        if (progressBar != null) { progressBar.value = Mathf.Lerp(progressBar.value, loadingProcess.progress, 0.1f); }
        if (statusText != null && progressBar != null) { statusText.text = Mathf.Round(progressBar.value * 100).ToString() + "%"; }

        if (canvasGroup.alpha == 0)
        {
            StopCoroutine(FadeInLoadingScreen());
            StartCoroutine(FadeInLoadingScreen());
        }

        else if (loadingProcess.progress == 0.9f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            StopCoroutine(FadeOutBackgroundScreen());
            StartCoroutine(FadeOutBackgroundScreen());

            StopCoroutine(FadeOutContentScreen(true));
            StartCoroutine(FadeOutContentScreen(true));
        }

        else if (loadingProcess.isDone
            && loadingProcess.allowSceneActivation
)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            StopCoroutine(FadeOutBackgroundScreen());
            StartCoroutine(FadeOutBackgroundScreen());

            StopCoroutine(FadeOutContentScreen(true));
            StartCoroutine(FadeOutContentScreen(true));
        }

        else if (waitForPlayerInput && loadingProcess.progress == 0.9f)
        {
                        if (useCountdown)
            {
                pakCountdownSlider.value -= Time.unscaledDeltaTime;
                pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                if (pakCountdownSlider.value == 0)
                {
                    loadingProcess.allowSceneActivation = true;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine(FadeOutBackgroundScreen());
                    StartCoroutine(FadeOutBackgroundScreen());

                    StopCoroutine(FadeOutPAKScreen(true));
                    StartCoroutine(FadeOutPAKScreen(true));
                }
            }
            else 
            {
                StopCoroutine(FadeOutContentScreen(false));
                StartCoroutine(FadeOutContentScreen(false));

                StopCoroutine(FadeInPAKScreen());
                StartCoroutine(FadeInPAKScreen());
            }

            if (Input.anyKeyDown)
            {
                loadingProcess.allowSceneActivation = true;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                StopCoroutine(FadeOutBackgroundScreen());
                StartCoroutine(FadeOutBackgroundScreen());

                StopCoroutine(FadeOutPAKScreen(true));
                StartCoroutine(FadeOutPAKScreen(true));
            }
            else if (!waitForPlayerInput && loadingProcess.isDone)
            {
                                if (useCountdown)
                {
                    pakCountdownSlider.value -= Time.unscaledDeltaTime;
                    pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                    if (pakCountdownSlider.value == 0)
                    {
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine(FadeOutBackgroundScreen());
                        StartCoroutine(FadeOutBackgroundScreen());

                        StopCoroutine(FadeOutPAKScreen(true));
                        StartCoroutine(FadeOutPAKScreen(true));
                    }
                }
                else
                {
                    StopCoroutine(FadeInPAKScreen());
                    StartCoroutine(FadeInPAKScreen());
                }

                if (Input.anyKeyDown)
                {
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine(FadeOutBackgroundScreen());
                    StartCoroutine(FadeOutBackgroundScreen());

                    StopCoroutine(FadeOutPAKScreen(true));
                    StartCoroutine(FadeOutPAKScreen(true));
                }
            }
        }
    }

    void ProcessVirtualLoading()
    {
        if (!processLoading)
            return;

        if (progressBar != null) { progressBar.value += 1 / virtualLoadingTimer * Time.unscaledDeltaTime; }
        if (statusText != null && progressBar != null) { statusText.text = Mathf.Round(progressBar.value * 100).ToString() + "%"; }

        currentVirtualTime += Time.unscaledDeltaTime;

        if (canvasGroup.alpha == 0)
        {
            StopCoroutine(FadeInLoadingScreen());
            StartCoroutine(FadeInLoadingScreen());
        }

        if (currentVirtualTime >= virtualLoadingTimer)
        {
            if (!loadingProcess.allowSceneActivation) { loadingProcess.allowSceneActivation = true; }
            else if (!loadingProcess.allowSceneActivation && !waitForPlayerInput) { loadingProcess.allowSceneActivation = true; }

            if (loadingProcess.progress == 0.9f)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                StopCoroutine(FadeOutBackgroundScreen());
                StartCoroutine(FadeOutBackgroundScreen());

                StopCoroutine(FadeOutContentScreen(true));
                StartCoroutine(FadeOutContentScreen(true));
            }

            else if (loadingProcess.isDone)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                StopCoroutine(FadeOutBackgroundScreen());
                StartCoroutine(FadeOutBackgroundScreen());

                StopCoroutine(FadeOutContentScreen(true));
                StartCoroutine(FadeOutContentScreen(true));
            }

            else if (waitForPlayerInput && loadingProcess.progress == 0.9f)
            {
                

                if (useCountdown)
                {
                    pakCountdownSlider.value -= Time.unscaledDeltaTime;
                    pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                    if (pakCountdownSlider.value == 0)
                    {
                        loadingProcess.allowSceneActivation = true;
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine(FadeOutBackgroundScreen());
                        StartCoroutine(FadeOutBackgroundScreen());

                        StopCoroutine(FadeOutPAKScreen(true));
                        StartCoroutine(FadeOutPAKScreen(true));
                    }
                }
                else
                {
                    StopCoroutine(FadeOutContentScreen(false));
                    StartCoroutine(FadeOutContentScreen(false));

                    StopCoroutine(FadeInPAKScreen());
                    StartCoroutine(FadeInPAKScreen());
                }


                if (Input.anyKeyDown)
                {
                    loadingProcess.allowSceneActivation = true;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;

                    StopCoroutine(FadeOutBackgroundScreen());
                    StartCoroutine(FadeOutBackgroundScreen());

                    StopCoroutine(FadeOutPAKScreen(true));
                    StartCoroutine(FadeOutPAKScreen(true));
                }
                else if (!waitForPlayerInput)
                {
                    if (useCountdown)
                    {
                        pakCountdownSlider.value -= Time.unscaledDeltaTime;
                        pakCountdownLabel.text = Mathf.Round(pakCountdownSlider.value * 1).ToString();

                        if (pakCountdownSlider.value == 0)
                        {
                            canvasGroup.interactable = false;
                            canvasGroup.blocksRaycasts = false;

                            StopCoroutine(FadeOutBackgroundScreen());
                            StartCoroutine(FadeOutBackgroundScreen());

                            StopCoroutine(FadeOutPAKScreen(true));
                            StartCoroutine(FadeOutPAKScreen(true));
                        }
                    }
                    else
                    {
                        StopCoroutine(FadeInPAKScreen());
                        StartCoroutine(FadeInPAKScreen());
                    }

                    if (Input.anyKeyDown)
                    {
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;

                        StopCoroutine(FadeOutBackgroundScreen());
                        StartCoroutine(FadeOutBackgroundScreen());

                        StopCoroutine(FadeOutPAKScreen(true));
                        StartCoroutine(FadeOutPAKScreen(true));
                    }
                }
            }
        }
    }

    IEnumerator FadeInLoadingScreen()
    {
        canvasGroup.alpha = 0;

        while (canvasGroup.alpha < 0.99f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        canvasGroup.alpha = 1;

        StopCoroutine(FadeInBackgroundScreen());
        StartCoroutine(FadeInBackgroundScreen());

        StopCoroutine(FadeInContentScreen());
        StartCoroutine(FadeInContentScreen());
    }

    IEnumerator FadeOutLoadingScreen()
    {
        backgroundCanvasGroup.gameObject.SetActive(false);
        contentCanvasGroup.gameObject.SetActive(false);
        pakCanvasGroup.gameObject.SetActive(false);

        while (canvasGroup.alpha > 0.01f)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator FadeInContentScreen()
    {
        while (contentCanvasGroup.alpha < 0.99f)
        {
            contentCanvasGroup.alpha += Time.unscaledDeltaTime * contentFadeSpeed;
            yield return null;
        }
        contentCanvasGroup.alpha = 1;
    }

    IEnumerator FadeOutContentScreen(bool fadeOutScreenAfter)
    {

        while (contentCanvasGroup.alpha > 0.01f)
        {
            contentCanvasGroup.alpha -= Time.unscaledDeltaTime * contentFadeSpeed;
            yield return null;
        }

        contentCanvasGroup.alpha = 0;

        if (fadeOutScreenAfter)
        {
            StopCoroutine(FadeOutLoadingScreen());
            StartCoroutine(FadeOutLoadingScreen());
        }
    }

    IEnumerator FadeInBackgroundScreen()
    {

        while (backgroundCanvasGroup.alpha < 0.99f)
        {
            backgroundCanvasGroup.alpha += Time.unscaledDeltaTime * backgroundFadeSpeed;
            yield return null;
        }

        backgroundCanvasGroup.alpha = 1;
    }

    IEnumerator FadeOutBackgroundScreen()
    {

        while (backgroundCanvasGroup.alpha > 0.01f)
        {
            backgroundCanvasGroup.alpha -= Time.unscaledDeltaTime * backgroundFadeSpeed;
            yield return null;
        }

        backgroundCanvasGroup.alpha = 0;
    }

    IEnumerator FadeInPAKScreen()
    {
        pakCanvasGroup.alpha = 0;

        StopCoroutine(FadeOutContentScreen(false));
        StartCoroutine(FadeOutContentScreen(false));

        while (pakCanvasGroup.alpha < 0.99f)
        {
            pakCanvasGroup.alpha += Time.unscaledDeltaTime * contentFadeSpeed;
            yield return null;
        }

        pakCanvasGroup.alpha = 1;
    }

    IEnumerator FadeOutPAKScreen(bool fadeOutScreenAfter)
    {

        while (pakCanvasGroup.alpha > 0.01f)
        {
            pakCanvasGroup.alpha -= Time.unscaledDeltaTime * contentFadeSpeed;
            yield return null;
        }
        pakCanvasGroup.alpha = 0;

        if (fadeOutScreenAfter)
        {
            StopCoroutine(FadeOutLoadingScreen());
            StartCoroutine(FadeOutLoadingScreen());
        }
    }

    IEnumerator HandleVirtualTransition(float timer)
    {
        yield return new WaitForSeconds(timer);
        StartCoroutine(FadeOutContentScreen(true));
    }

    private IEnumerator RandomHint()
    {
        while (true)
        {
            string hint = GetRandomItem(hintList, ref currentHintIndex);
            hintsText.text = hint;
            hintsText.maxVisibleCharacters = 0;

            int totalChars = hint.Length;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / hintRevealDuration;
                int visibleChars = Mathf.FloorToInt(totalChars * Mathf.Clamp01(t));
                hintsText.maxVisibleCharacters = visibleChars;
                yield return null;
            }

            hintsText.maxVisibleCharacters = totalChars;

            yield return new WaitForSecondsRealtime(hintTimerValue);
        }
    }

    private IEnumerator RandomImage()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(imageTimerValue);

            while (imageObject.color.a > 0.01f)
            {
                imageObject.color = Color.Lerp(
                    imageObject.color,
                    new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 0),
                    imageFadingSpeed / 30f
                );
                yield return new WaitForFixedUpdate();
            }

            imageObject.color = new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 0);

            imageObject.sprite = GetRandomItem(imageList, ref currentImageIndex);

            while (imageObject.color.a < 0.99f)
            {
                imageObject.color = Color.Lerp(
                    imageObject.color,
                    new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 1),
                    imageFadingSpeed / 30f
                );
                yield return new WaitForFixedUpdate();
            }

            imageObject.color = new Color(imageObject.color.r, imageObject.color.g, imageObject.color.b, 1);
        }
    }

    private T GetRandomItem<T>(List<T> list, ref int lastIndex)
    {
        if (list.Count <= 1) return list[0];

        int newIndex;
        do
        {
            newIndex = Random.Range(0, list.Count);
        } while (newIndex == lastIndex);

        lastIndex = newIndex;
        return list[newIndex];
    }
}