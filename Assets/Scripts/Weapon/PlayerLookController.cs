using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerLookController : MonoBehaviour
{
    public Transform playerBody;
    public Transform cameraTransform;
    public Transform meshTransform;

    private MouseLook mouseLook = new MouseLook();

    void Start()
    {
        mouseLook.Init(playerBody, cameraTransform);
    }

    void Update()
    {
        mouseLook.LookRotation(playerBody, cameraTransform, meshTransform);
    }
}
