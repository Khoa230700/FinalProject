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
        yield return new WaitForSeconds(7.5f);
        gameObject.SetActive(false);
    }

    [ContextMenu("Clear PlayerPrefs")]
    public void ClearKeyNow()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}