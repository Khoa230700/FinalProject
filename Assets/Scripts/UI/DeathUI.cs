using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeathUI : MonoBehaviour
{
    public static GameObject instance;

    [SerializeField] private Slider countdown;
    [SerializeField] private TextMeshProUGUI number;
    [SerializeField] private float timer = 5f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        countdown.maxValue = timer;
        countdown.value = timer;
        number.text = timer.ToString();
    }

    public static void Test()
    {
        if (instance != null) Destroy(instance);
        instance = Instantiate(Resources.Load<GameObject>("Death"));
    }

    private IEnumerator Countdown()
    {
        float t = timer;

        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;

            countdown.value = t;
            number.text = t.ToString("F0");

            yield return null;
        }

        countdown.value = 0;
        number.text = "0";

        animator.Play("Out");
        var length = animator.GetCurrentAnimatorClipInfo(0).Length;
        Destroy(gameObject, length);
    }
}
