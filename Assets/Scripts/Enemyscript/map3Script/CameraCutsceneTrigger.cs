using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class CameraCutsceneTrigger : MonoBehaviour
{
    public PlayableDirector cutsceneDirector;
    public GameObject playerCamera;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCamera.SetActive(false); // optional: disable player cam
            cutsceneDirector.Play();
        }
    }
}