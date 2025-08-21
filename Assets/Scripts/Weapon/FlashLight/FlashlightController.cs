using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;
    public KeyCode toggleKey = KeyCode.L;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && flashlight != null)
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}
