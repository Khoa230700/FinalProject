using System.Collections;
using UnityEngine;

public class SplashScreenUI : MonoBehaviour
{
    private static string Key =>
#if UNITY_EDITOR
        "HasPlayedSplash_Editor";
#else
        "HasPlayedSplash_" + Application.version;
#endif

    private void Awake()
    {
        if (PlayerPrefs.GetInt(Key, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        PlayerPrefs.SetInt(Key, 1);
        PlayerPrefs.Save();
        StartCoroutine(ShowSplash());
    }

    private IEnumerator ShowSplash()
    {
        var length = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        Debug.Log(length);
        yield return new WaitForSeconds(length);
        gameObject.SetActive(false);
    }

    [ContextMenu("Clear PlayerPrefs")]
    public void ClearKeyNow()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}