using UnityEngine;

public class tam : MonoBehaviour
{
    public Texture2D crosshairTexture;
    public float size = 16f;

    void OnGUI()
    {
        if (crosshairTexture != null)
        {
            float xMin = (Screen.width - size) / 2;
            float yMin = (Screen.height - size) / 2;
            GUI.DrawTexture(new Rect(xMin, yMin, size, size), crosshairTexture);
        }
    }
}
