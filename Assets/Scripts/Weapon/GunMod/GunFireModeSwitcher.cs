using UnityEngine;

public class GunFireModeSwitcher : MonoBehaviour
{
    public GunFireMode currentFireMode = GunFireMode.FullAuto;

    // Toggle giữa Burst và FullAuto bằng phím B
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchFireMode();
        }
    }

    public void SwitchFireMode()
    {
        if (currentFireMode == GunFireMode.Burst)
            currentFireMode = GunFireMode.FullAuto;
        else if (currentFireMode == GunFireMode.FullAuto)
            currentFireMode = GunFireMode.Burst;

        Debug.Log($"Switched to {currentFireMode}");
    }
}
