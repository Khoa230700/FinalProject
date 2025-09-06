using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider countdown;
    [SerializeField] private TextMeshProUGUI number;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private float timer = 5f;

    private Animator animator;
    private bool isMissionFailed;

    void OnEnable()
    {
        animator = GetComponent<Animator>();

        countdown.maxValue = timer;
        countdown.value = timer;
        number.text = timer.ToString();
        titleText.text = isMissionFailed ? "MISSION FAILED" : "YOU DIED";
    }

    void Update()
    {
        if (Input.anyKeyDown && canvasGroup.interactable)
        {
            StartCoroutine(PlayOutAnimation());
        }
    }

    public void Show(bool missionFailed = false)
    {
        isMissionFailed = missionFailed;
        pauseCanvas.SetActive(false);
        gameObject.SetActive(true);
    }

    private IEnumerator Countdown()
    {
        float t = timer;

        {
            while (t > 0f)
                t -= Time.unscaledDeltaTime;

            countdown.value = t;
            number.text = t.ToString("F0");

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

        if (isMissionFailed)
        {
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("Mission Failed");
        }
        else
        {
            Debug.Log("Death");

            // SelectorSpawner.Instance.Player.GetComponent<PlayerHealthSystem>().Respawn();
        }

        pauseCanvas.SetActive(true);
        gameObject.SetActive(false);
    }
}
