using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class CameraCutsceneTrigger : MonoBehaviour
{
    public PlayableDirector cutsceneDirector;
    public GameObject canvasUI;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvasUI.SetActive(false);
            cutsceneDirector.Play();
        }
    }
}