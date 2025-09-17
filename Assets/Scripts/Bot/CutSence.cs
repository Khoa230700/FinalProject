using UnityEngine;
using UnityEngine.Playables;

public class CutSence : MonoBehaviour
{
    public PlayableDirector cutscene;  // kéo PlayableDirector vào
    private bool hasPlayed = false;    // tránh play nhiều lần
    public GameObject botmelee;
    public GameObject botcutscene;
    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;

        if (other.CompareTag("BotMelee")) // nếu nhân vật có tag "Player"
        {
            botmelee.SetActive(false);
            botcutscene.SetActive(true);
            cutscene.Play();
            hasPlayed = true;
            Debug.Log("Cutscene Started!");
        }
    }
}
