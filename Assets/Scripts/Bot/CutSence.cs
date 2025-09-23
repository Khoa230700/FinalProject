using UnityEngine;
using UnityEngine.Playables;

public class CutSence : MonoBehaviour
{
    public PlayableDirector cutscene;  // kéo PlayableDirector vào
    private bool hasPlayed = false;    // tránh play nhiều lần
    public GameObject botmelee;
    public GameObject botcutscene;
    public EndUI endUI;                // tham chiếu tới EndUI

    private void Start()
    {
        // đăng ký sự kiện khi cutscene kết thúc
        if (cutscene != null)
        {
            cutscene.stopped += OnCutsceneFinished;
        }
    }

    private void OnDestroy()
    {
        if (cutscene != null)
        {
            cutscene.stopped -= OnCutsceneFinished;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;

        if (other.CompareTag("BotMelee"))
        {
            botmelee.SetActive(false);
            botcutscene.SetActive(true);
            cutscene.Play();
            hasPlayed = true;
            Debug.Log(cutscene.duration);
            Debug.Log("Cutscene Started!");
        }
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        Debug.Log("Cutscene Finished!");
        if (endUI != null)
        {
            endUI.ShowGain();
        }
    }
}
