using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerLookController : MonoBehaviour
{
    public Transform playerBody;         // FPS Player Shooter (CharacterController)
    public Transform cameraTransform;    // Camera (Camera001)

    private MouseLook mouseLook = new MouseLook();  // Instantiate MouseLook

    void Start()
    {
        mouseLook.Init(playerBody, cameraTransform);
    }

    void Update()
    {
        mouseLook.LookRotation(playerBody, cameraTransform);
    }
}
