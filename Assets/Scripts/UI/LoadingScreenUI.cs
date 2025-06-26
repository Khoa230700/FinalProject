using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    [Header("RESOURCES")]
    public CanvasGroup canvasGroup;
    public CanvasGroup backgroundCanvasGroup;
    public CanvasGroup contentCanvasGroup;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Slider progressBar;
    public Transform spinnerParent;
    public TextMeshProUGUI hintsText;
    public Image imageObject;

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

        statusText.text = "0%";
        progressBar.value = 0;
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
